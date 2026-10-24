const layers = [
  ['WebStore.Model', 'encje domenowe (DataModels), niezależne od providera bazy danych.'],
  ['WebStore.DAL', 'kontekst Entity Framework Core, konfiguracja providera i migracje bazy danych.'],
  ['WebStore.Services', 'interfejsy i usługi logiki biznesowej oraz profil mapowania AutoMappera.'],
  ['WebStore.ViewModels', 'modele widoków (VM) wymieniane między usługami a warstwą prezentacji.'],
  ['WebStore.Web', 'aplikacja ASP.NET Core serwująca klienta React.'],
  ['WebStore.Tests', 'testy jednostkowe usług uruchamiane na bazie SQLite w pamięci.']
];

const technologies = [
  ['.NET', 'platforma serwerowa i język C#.'],
  ['Entity Framework Core', 'dostęp do danych i migracje.'],
  ['AutoMapper', 'mapowanie encji na view modele.'],
  ['xUnit', 'testy jednostkowe.'],
  ['React', 'interfejs użytkownika w przeglądarce.']
];

export default function Home() {
  return (
    <>
      <h1>Witaj w WebStore</h1>
      <p>
        WebStore to aplikacja klient–serwer realizowana iteracyjnie, w kolejnych sprintach.
        Ta strona przedstawia strukturę projektu i użyte technologie.
      </p>

      <h2>Warstwy solucji</h2>
      <ul>
        {layers.map(([name, description]) => (
          <li key={name}><strong>{name}</strong> — {description}</li>
        ))}
      </ul>

      <h2>Technologie</h2>
      <ul>
        {technologies.map(([name, description]) => (
          <li key={name}><strong>{name}</strong> — {description}</li>
        ))}
      </ul>
    </>
  );
}
