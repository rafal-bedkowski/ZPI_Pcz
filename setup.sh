#!/usr/bin/env bash
# Konfiguracja środowiska projektu WebStore na macOS.
# Użycie: ./setup.sh [--check-only]
#   --check-only  tylko sprawdza narzędzia; niczego nie instaluje, o nic nie pyta
#                 i nie uruchamia restore/build/test.
# Brakujące .NET SDK i Node.js są instalowane wyłącznie po zgodzie użytkownika.

set -u

CHECK_ONLY=0
for arg in "$@"; do
  case "$arg" in
    --check-only) CHECK_ONLY=1 ;;
    -h|--help)
      sed -n '2,6p' "$0" | sed 's/^# \{0,1\}//'
      exit 0
      ;;
    *)
      echo "Nieznany parametr: $arg (dostępny: --check-only)" >&2
      exit 2
      ;;
  esac
done

cd "$(dirname "$0")" || exit 1

OK_LIST=()
MISSING_LIST=()
INFO_LIST=()

ok()      { OK_LIST+=("$1"); }
missing() { MISSING_LIST+=("$1"); }
info()    { INFO_LIST+=("$1"); }
step()    { printf '\n== %s ==\n' "$1"; }
has()     { command -v "$1" >/dev/null 2>&1; }

# Pyta [t/N]; bez terminala interaktywnego odpowiedź to zawsze "nie".
confirm() {
  [ -t 0 ] || return 1
  local answer
  read -r -p "$1 [t/N] " answer
  case "$answer" in
    t|T|tak|Tak|TAK) return 0 ;;
    *) return 1 ;;
  esac
}

# dotnet uruchomiony w katalogu repozytorium sam stosuje global.json (wersja + rollForward);
# brak zgodnego SDK kończy się błędem.
dotnet_sdk_ok() {
  has dotnet && dotnet --version >/dev/null 2>&1
}

step "Git"
if has git; then
  ok "Git: $(git --version)"
else
  missing "Git: nie znaleziono. Zainstaluj Xcode Command Line Tools (xcode-select --install) lub: brew install git"
fi

step ".NET SDK"
required_sdk="$(sed -n 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' global.json | head -n 1)"
if dotnet_sdk_ok; then
  ok ".NET SDK: $(dotnet --version) (wymagane z global.json: $required_sdk lub nowsza poprawka 10.0.x)"
else
  missing ".NET SDK zgodny z global.json ($required_sdk, rollForward) nie jest zainstalowany"
  install_cmd="brew install --cask dotnet-sdk"
  if [ "$CHECK_ONLY" -eq 1 ]; then
    info "Instalacja .NET SDK: $install_cmd"
  elif ! has brew; then
    info "Brak Homebrew (https://brew.sh). Po jego instalacji uruchom: $install_cmd"
  elif ! has sudo || [ ! -t 0 ]; then
    info "Brak sudo lub terminala interaktywnego. Uruchom ręcznie (wymaga hasła administratora): $install_cmd"
  elif confirm "Zainstalować .NET SDK przez Homebrew ($install_cmd; zapyta o hasło administratora)?"; then
    brew install --cask dotnet-sdk
    hash -r
    if dotnet_sdk_ok; then
      MISSING_LIST=("${MISSING_LIST[@]/.NET SDK zgodny*/}")
      ok ".NET SDK: $(dotnet --version) (zainstalowano)"
    else
      missing ".NET SDK: instalacja nie dała zgodnej wersji; sprawdź komunikat Homebrew i spróbuj ponownie w nowym terminalu"
    fi
  else
    info "Pominięto instalację .NET SDK. Komenda: $install_cmd"
  fi
fi

step "Node.js (opcjonalny)"
if has node; then
  ok "Node.js: $(node --version) (opcjonalny; tylko klient React i dotnet publish)"
else
  info "Node.js nie jest zainstalowany. Jest opcjonalny: potrzebny tylko do klienta React (ClientApp) i dotnet publish; build i testy .NET działają bez niego."
  if [ "$CHECK_ONLY" -eq 0 ]; then
    if ! has brew; then
      info "Brak Homebrew (https://brew.sh); instalacja Node.js: brew install node"
    elif confirm "Zainstalować Node.js (LTS) przez Homebrew (brew install node)?"; then
      brew install node
      hash -r
      if has node; then
        ok "Node.js: $(node --version) (zainstalowano)"
      else
        info "Node.js: instalacja nie powiodła się; sprawdź komunikat Homebrew"
      fi
    else
      info "Pominięto instalację Node.js."
    fi
  fi
fi

step "Docker / SQL Server (opcjonalne)"
if has docker; then
  ok "Docker: $(docker --version) (opcjonalny; np. do uruchomienia SQL Server na potrzeby testów)"
else
  info "Docker nie jest zainstalowany (opcjonalny). Testy SQL Server są pomijane bez zmiennej WEBSTORE_TEST_SQLSERVER; domyślna baza to SQLite."
fi

step "Restore, build, testy"
if [ "$CHECK_ONLY" -eq 1 ]; then
  info "Tryb --check-only: pominięto dotnet tool restore, dotnet restore, dotnet build i dotnet test."
elif ! dotnet_sdk_ok; then
  info "Pominięto restore, build i testy: brak zgodnego .NET SDK."
else
  failed=""
  for cmd in "dotnet tool restore" "dotnet restore" "dotnet build --no-restore" "dotnet test --no-build"; do
    echo "> $cmd"
    if ! $cmd; then
      failed="$cmd"
      break
    fi
  done
  if [ -z "$failed" ]; then
    ok "dotnet tool restore, restore, build i test zakończone powodzeniem"
  else
    missing "Polecenie zakończyło się błędem: $failed"
  fi
fi

printf '\n==================== PODSUMOWANIE ====================\n'
for item in "${OK_LIST[@]+"${OK_LIST[@]}"}"; do printf '[OK]      %s\n' "$item"; done
for item in "${MISSING_LIST[@]+"${MISSING_LIST[@]}"}"; do
  [ -n "$item" ] && printf '[BRAK]    %s\n' "$item"
done
for item in "${INFO_LIST[@]+"${INFO_LIST[@]}"}"; do printf '[INFO]    %s\n' "$item"; done
printf '======================================================\n'

for item in "${MISSING_LIST[@]+"${MISSING_LIST[@]}"}"; do
  [ -n "$item" ] && exit 1
done
exit 0
