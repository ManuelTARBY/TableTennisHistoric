# 🏓 TableTennisHistoric

Application web personnelle de suivi de résultats de tennis de table (compétition individuelles et championnats), développée en ASP.NET Core Razor Pages.

## 🛠️ Stack technique

- **Framework** : ASP.NET Core (Razor Pages)
- **Base de données** : MySQL 8.0
- **ORM** : Entity Framework Core
- **UI** : Bootstrap 5, Chart.js
- **Langage** : C#

## ⚙️ Configuration locale

### Prérequis

- .NET SDK (version utilisée dans le projet)
- MySQL 8.0+
- Visual Studio ou VS Code

### Installation

1. Cloner le dépôt :
   ```bash
   git clone https://github.com/votre-compte/votre-repo.git
   cd votre-repo
   ```

2. Configurer la chaîne de connexion via les user-secrets .NET :
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=...;database=...;user=...;password=..."
   dotnet user-secrets set "AppUrl" "http://localhost:44304"
   ```

3. Appliquer les migrations :
   ```bash
   dotnet ef database update
   ```

4. Lancer l'application :
   ```bash
   dotnet run
   ```

## 📁 Structure du projet

```
TableTennisHistoric/
├── Datas/                  # DbContext et configuration EF Core
├── DTO/                    # Objets de transfert de données
│   ├── MatchDTO.cs
│   ├── PlayerDTO.cs
│   ├── SeasonDTO.cs
│   ├── CompetitionDTO.cs
│   ├── ChampionshipDTO.cs  # Inclut ChampionshipMatchData, ChampionshipTeamResult, ChampionshipPageData
│   ├── ClubDTO.cs
│   ├── ClubsPageDataDTO.cs
│   ├── CompetitionsPageDataDTO.cs
│   ├── CreateMatchDTO.cs
│   ├── IndexDataDTO.cs
│   ├── MatchesPageDataDTO.cs
│   ├── PlayerWithClubDTO.cs
│   ├── RankingHistoryDTO.cs
│   └── SetDTO.cs
├── Migrations/             # Migrations Entity Framework Core
├── Models/                 # Entités de la base de données
├── Pages/                  # Pages Razor (UI + logique allégée)
│   ├── Championship/
│   ├── Matches/
│   ├── Players/
│   └── ...
├── Services/               # Services métiers
│   ├── Interfaces/         # Interfaces des services
│   │   ├── IChampionshipService.cs
│   │   ├── IClubService.cs
│   │   ├── ICompetitionService.cs
│   │   ├── IIndexService.cs
│   │   ├── IMatchService.cs
│   │   ├── IMatchSetService.cs
│   │   ├── IPlayerService.cs
│   │   └── ISeasonService.cs
│   ├── ChampionshipService.cs
│   ├── ClubService.cs
│   ├── CompetitionService.cs
│   ├── IndexService.cs
│   ├── MatchService.cs
│   ├── MatchSetService.cs
│   ├── PlayerService.cs
│   └── SeasonService.cs
└── wwwroot/                # Fichiers statiques (CSS, JS)
```

## 🏗️ Architecture

L'application suit une architecture en couches :

- **Pages** : orchestration uniquement — pas de logique métier ni d'accès direct à la base de données
- **Services** : toute la logique métier, injectés via leurs interfaces
- **DTO** : objets de transfert entre les couches Pages et Services
- **Models** : entités Entity Framework mappées sur la base de données

Chaque service est enregistré via son interface dans `Program.cs` :
```csharp
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
// etc.
```

## 🗄️ Base de données

Les migrations sont gérées via Entity Framework Core. En cas de changement de machine :

1. Restaurer la base de données depuis une sauvegarde, ou
2. Relancer `dotnet ef database update` pour recréer la structure

> ⚠️ La chaîne de connexion ne doit jamais être commitée. Elle est gérée via les user-secrets .NET en développement.

## 🚀 Déploiement local

L'application se lance automatiquement sur `http://localhost:44304` et ouvre le navigateur par défaut hors mode Debug.

La saison sélectionnée sur la page d'accueil est mémorisée en session et restaurée automatiquement à la prochaine visite.

## 📝 Notes personnelles

- Penser à sauvegarder la base de données régulièrement
- Les user-secrets sont stockés dans `%APPDATA%\Microsoft\UserSecrets\` sous Windows
- La session est configurée avec un timeout de 8h (`AddSession` dans `Program.cs`)
- Les tables de gains (points par match) sont centralisées dans `MatchService` — ne pas les dupliquer ailleurs