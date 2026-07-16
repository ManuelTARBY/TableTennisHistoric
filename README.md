# 🏓 TableTennisHistoric

Application web personnelle de suivi de résultats de tennis de table, développée en ASP.NET Core Razor Pages.

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
│   ├── ChampionshipDTO.cs  # Inclut ChampionshipMatchData, ChampionshipTeamResult, ChampionshipPageData
│   ├── ClubDTO.cs
│   ├── ClubsPageDataDTO.cs
│   ├── CoefficientCompetitionEditDTO.cs
│   ├── CompetitionDTO.cs
│   ├── CompetitionsPageDataDTO.cs
│   ├── CompetitionSupplementDTO.cs
│   ├── CreateMatchDTO.cs
│   ├── IndexDataDTO.cs
│   ├── MatchDTO.cs
│   ├── MatchesPageDataDTO.cs
│   ├── MatchesFilterDTO.cs
│   ├── PlayerDTO.cs
|	├── PlayerEditDTO.cs
│   ├── PlayerSeasonDTO.cs
│   ├── PlayerWithClubDTO.cs
│   ├── RankingHistoryDTO.cs
│   ├── SeasonCompetitionDTO.cs
│   ├── SeasonDTO.cs
│   ├── SeasonEditDTO.cs
│   ├── SetDTO.cs
│   ├── TableTennisMatchDTO.cs
│   └── TeamDTO.cs
├── Migrations/             # Migrations Entity Framework Core
├── Models/                 # Entités de la base de données
├── Pages/                  # Pages Razor (UI + logique allégée)
│   ├── Championship/
│   │   ├── Create.cshtml   # Création et gestion des championnats
│   │   └── Read.cshtml     # Consultation et saisie des scores
│   ├── Matches/
│   │   ├── CreateMatches.cshtml
│   │   ├── FilteredMatches.cshtml
│   │   ├── Matches.cshtml
│   │   └── UpdateMatch.cshtml
│   ├── Players/
│   │   ├── Affiliate.cshtml        # Création affiliation
│   │   ├── Create.cshtml           # Création joueur avec son affiliation dans la foulée
│   │   ├── ManagePlayerSeason.cshtml  # Modification/suppression des affiliations
│   │   └── RankingHistory.cshtml
│   ├── Calculator.cshtml
│   ├── Clubs.cshtml
│   ├── Competitions.cshtml
│   ├── Error.cshtml
│   ├── Index.cshtml
│   ├── OpponentsHistoric.cshtml
│   ├── Privacy.cshtml
│   └── Seasons.cshtml
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

### Contraintes notables

- La paire `PlayerId/SeasonId` dans `PlayerSeason` est unique — un joueur ne peut avoir qu'une seule affiliation par saison.

## 🚀 Déploiement local

L'application se lance automatiquement sur `http://localhost:44304` et ouvre le navigateur par défaut hors mode Debug.

La saison sélectionnée sur la page d'accueil est mémorisée en session et restaurée automatiquement à la prochaine visite.

## 📝 Notes personnelles

- Penser à sauvegarder la base de données régulièrement
- Les user-secrets sont stockés dans `%APPDATA%\Microsoft\UserSecrets\` sous Windows
- La session est configurée avec un timeout de 8h (`AddSession` dans `Program.cs`)
- Les tables de gains (points par match) sont centralisées dans `MatchService` — ne pas les dupliquer ailleurs
- La page `ManagePlayerSeason` est accessible depuis le bouton à droite du sélecteur d'adversaire dans `OpponentsHistoric` et dans `Players\Affiliate` (lorsqu'un adversaire est sélectionné)
