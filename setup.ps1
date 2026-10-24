<#
.SYNOPSIS
Konfiguracja środowiska projektu WebStore na Windows.

.DESCRIPTION
Sprawdza Git, .NET SDK zgodny z global.json oraz opcjonalnie Node.js i Docker.
Brakujące .NET SDK i Node.js instaluje przez winget wyłącznie po zgodzie użytkownika.
Następnie wykonuje dotnet tool restore, restore, build i test.
Jeżeli działa usługa MSSQL$SQLEXPRESS, po zgodzie użytkownika ustawia user-secrets projektu WebStore.Web
(Database:Provider i ConnectionStrings:SqlServer), aby aplikacja używała SQL Server zamiast SQLite.

.PARAMETER CheckOnly
Tylko sprawdza narzędzia; niczego nie instaluje, o nic nie pyta, nie ustawia user-secrets
i nie uruchamia restore/build/test.

.EXAMPLE
powershell -ExecutionPolicy Bypass -File .\setup.ps1 -CheckOnly
#>
[CmdletBinding()]
param(
    [switch]$CheckOnly
)

$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot

$ok = New-Object System.Collections.Generic.List[string]
$missing = New-Object System.Collections.Generic.List[string]
$info = New-Object System.Collections.Generic.List[string]

function Write-Step([string]$Text) { Write-Host ""; Write-Host "== $Text ==" }

function Test-Command([string]$Name) {
    return [bool](Get-Command $Name -ErrorAction SilentlyContinue)
}

# Pyta [t/N]; bez terminala interaktywnego odpowiedź to zawsze "nie".
function Confirm-Action([string]$Question) {
    if (-not [Environment]::UserInteractive -or [Console]::IsInputRedirected) { return $false }
    $answer = Read-Host "$Question [t/N]"
    return $answer -match '^(t|tak)$'
}

# dotnet uruchomiony w katalogu repozytorium sam stosuje global.json (wersja + rollForward);
# brak zgodnego SDK kończy się błędem.
function Test-DotnetSdk {
    if (-not (Test-Command 'dotnet')) { return $false }
    try {
        & dotnet --version 2>&1 | Out-Null
        return ($LASTEXITCODE -eq 0)
    } catch {
        return $false
    }
}

# Po instalacji przez winget bieżąca sesja nie zna jeszcze zaktualizowanego PATH.
function Update-SessionPath {
    $machine = [Environment]::GetEnvironmentVariable('Path', 'Machine')
    $user = [Environment]::GetEnvironmentVariable('Path', 'User')
    $env:Path = "$machine;$user"
}

function Install-WithWinget([string]$Id, [string]$Label) {
    if (-not (Test-Command 'winget')) {
        $info.Add("Brak winget. Zainstaluj $Label ręcznie (identyfikator pakietu: $Id).")
        return $false
    }
    if (-not (Confirm-Action "Zainstalować $Label przez winget (winget install --id $Id)?")) {
        $info.Add("Pominięto instalację $Label. Komenda: winget install --id $Id -e")
        return $false
    }
    & winget install --id $Id -e --accept-package-agreements --accept-source-agreements | Out-Host
    $exitCode = $LASTEXITCODE
    Update-SessionPath
    return ($exitCode -eq 0)
}

Write-Step 'Git'
if (Test-Command 'git') {
    $ok.Add("Git: $(& git --version)")
} else {
    $missing.Add('Git: nie znaleziono. Zainstaluj: winget install --id Git.Git -e')
}

Write-Step '.NET SDK'
$globalJson = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'global.json') | ConvertFrom-Json
$requiredSdk = $globalJson.sdk.version
if (Test-DotnetSdk) {
    $ok.Add(".NET SDK: $(& dotnet --version) (wymagane z global.json: $requiredSdk lub nowsza poprawka 10.0.x)")
} else {
    $installCmd = 'winget install --id Microsoft.DotNet.SDK.10 -e'
    if ($CheckOnly) {
        $missing.Add(".NET SDK zgodny z global.json ($requiredSdk, rollForward) nie jest zainstalowany")
        $info.Add("Instalacja .NET SDK: $installCmd")
    } else {
        $installed = Install-WithWinget 'Microsoft.DotNet.SDK.10' '.NET SDK 10'
        if ($installed -and (Test-DotnetSdk)) {
            $ok.Add(".NET SDK: $(& dotnet --version) (zainstalowano)")
        } else {
            $missing.Add(".NET SDK zgodny z global.json ($requiredSdk, rollForward) nie jest zainstalowany")
        }
    }
}

Write-Step 'Node.js (opcjonalny)'
if (Test-Command 'node') {
    $ok.Add("Node.js: $(& node --version) (opcjonalny; tylko klient React i dotnet publish)")
} else {
    $info.Add('Node.js nie jest zainstalowany. Jest opcjonalny: potrzebny tylko do klienta React (ClientApp) i dotnet publish; build i testy .NET działają bez niego.')
    if (-not $CheckOnly) {
        if (Install-WithWinget 'OpenJS.NodeJS.LTS' 'Node.js LTS') {
            if (Test-Command 'node') { $ok.Add("Node.js: $(& node --version) (zainstalowano)") }
            else { $info.Add('Node.js: zainstalowano, ale polecenie node będzie dostępne po otwarciu nowego terminala.') }
        }
    }
}

Write-Step 'Docker / SQL Server (opcjonalne)'
if (Test-Command 'docker') {
    $ok.Add("Docker: $(& docker --version) (opcjonalny; np. do uruchomienia SQL Server na potrzeby testów)")
} else {
    $info.Add('Docker nie jest zainstalowany (opcjonalny). Testy SQL Server są pomijane bez zmiennej WEBSTORE_TEST_SQLSERVER; domyślna baza to SQLite.')
}

Write-Step 'Restore, build, testy'
if ($CheckOnly) {
    $info.Add('Tryb -CheckOnly: pominięto dotnet tool restore, dotnet restore, dotnet build i dotnet test.')
} elseif (-not (Test-DotnetSdk)) {
    $info.Add('Pominięto restore, build i testy: brak zgodnego .NET SDK.')
} else {
    # Release, bo Smart App Control na części komputerów z Windows blokuje świeżo zbudowane biblioteki w Debug.
    $steps = @(
        @('tool', 'restore'),
        @('restore'),
        @('build', '-c', 'Release', '--no-restore'),
        @('test', '-c', 'Release', '--no-build')
    )
    $failed = $null
    foreach ($dotnetArgs in $steps) {
        Write-Host "> dotnet $($dotnetArgs -join ' ')"
        & dotnet @dotnetArgs
        if ($LASTEXITCODE -ne 0) { $failed = "dotnet $($dotnetArgs -join ' ')"; break }
    }
    if ($failed) { $missing.Add("Polecenie zakończyło się błędem: $failed") }
    else { $ok.Add('dotnet tool restore, restore, build i test zakończone powodzeniem') }
}

Write-Step 'SQL Server Express (opcjonalny)'
$sqlService = Get-Service -Name 'MSSQL$SQLEXPRESS' -ErrorAction SilentlyContinue
if (-not $sqlService) {
    $info.Add('SQL Server Express (instancja SQLEXPRESS) nie jest zainstalowany. Instalacja: winget install --id Microsoft.SQLServer.2025.Express -e (instalator Basic) lub strona https://www.microsoft.com/sql-server/sql-server-downloads. Bez niego aplikacja działa na SQLite.')
} elseif ($sqlService.Status -ne 'Running') {
    $info.Add('Usługa SQL Server Express (MSSQL$SQLEXPRESS) jest zainstalowana, ale nie działa. Uruchom ją, aby skonfigurować SQL Server; do tego czasu aplikacja działa na SQLite.')
} elseif ($CheckOnly) {
    $ok.Add('SQL Server Express: wykryto działającą instancję SQLEXPRESS (tryb -CheckOnly: konfiguracja pominięta)')
} elseif (-not (Test-DotnetSdk)) {
    $info.Add('Pominięto konfigurację SQL Server: brak zgodnego .NET SDK.')
} else {
    $webProject = Join-Path $PSScriptRoot 'WebStore.Web\WebStore.Web.csproj'
    $providerKey = 'Database:Provider'
    $connectionKey = 'ConnectionStrings:SqlServer'
    $question = 'Wykryto SQL Server Express (SQLEXPRESS). Skonfigurować aplikację do pracy z nim (user-secrets projektu WebStore.Web; nic nie trafia do repozytorium)?'
    if (-not (Confirm-Action $question)) {
        $info.Add('SQL Server nie został skonfigurowany: user-secrets bez zmian, aplikacja działa na SQLite.')
    } else {
        # Wartości sekretów nie są wypisywane; sprawdzane są tylko nazwy kluczy.
        $existing = @(& dotnet user-secrets list --project $webProject)
        if ($LASTEXITCODE -ne 0) {
            $info.Add('SQL Server nie został skonfigurowany: nie udało się odczytać user-secrets.')
        } else {
            $hasKeys = @($existing | Where-Object { $_ -match "^($providerKey|$connectionKey) = " }).Count -gt 0
            $overwrite = $true
            if ($hasKeys) {
                $overwrite = Confirm-Action 'User-secrets zawierają już Database:Provider lub ConnectionStrings:SqlServer. Nadpisać istniejące wartości?'
            }
            if (-not $overwrite) {
                $info.Add('Istniejące user-secrets zostały zachowane; SQL Server nie został skonfigurowany przez skrypt.')
            } else {
                # Ciąg połączenia powstaje tu, z nazwy wykrytej instancji; uwierzytelnianie Windows, bez loginu i hasła.
                # Lokalna instancja ma certyfikat samopodpisany, stąd zaufanie certyfikatowi serwera.
                $instance = $sqlService.Name.Substring('MSSQL$'.Length)
                $builder = New-Object System.Data.Common.DbConnectionStringBuilder
                $builder['Data Source'] = "localhost\$instance"
                $builder['Initial Catalog'] = 'WebStore'
                $builder['Integrated Security'] = 'True'
                $builder['TrustServerCertificate'] = 'True'
                & dotnet user-secrets set $providerKey 'SqlServer' --project $webProject | Out-Null
                $providerExit = $LASTEXITCODE
                & dotnet user-secrets set $connectionKey $builder.ConnectionString --project $webProject | Out-Null
                if ($providerExit -ne 0 -or $LASTEXITCODE -ne 0) {
                    $missing.Add('SQL Server: nie udało się zapisać user-secrets projektu WebStore.Web')
                } else {
                    $ok.Add("SQL Server: user-secrets projektu WebStore.Web ustawione dla instancji $instance (Database:Provider, ConnectionStrings:SqlServer)")
                    $info.Add('Schemat bazy utworzysz poleceniem: dotnet ef database update --context SqlServerDbContext --project WebStore.DAL --startup-project WebStore.Web. Powrót do SQLite: dotnet user-secrets clear --project WebStore.Web.')
                }
            }
        }
    }
}

Write-Host ''
Write-Host '==================== PODSUMOWANIE ===================='
foreach ($item in $ok) { Write-Host "[OK]      $item" -ForegroundColor Green }
foreach ($item in $missing) { Write-Host "[BRAK]    $item" -ForegroundColor Red }
foreach ($item in $info) { Write-Host "[INFO]    $item" -ForegroundColor Yellow }
Write-Host '======================================================'

if ($missing.Count -gt 0) { exit 1 }
exit 0
