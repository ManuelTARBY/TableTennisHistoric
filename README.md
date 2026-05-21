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
   git clone https://https://github.com/ManuelTARBY/TableTennisHistoric.git
   cd TableTennisHistoric
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
├── Datas/          # DbContext et configuration EF Core
├── DTO/            # Objets de transfert de données
├── Models/         # Entités de la base de données
├── Pages/          # Pages Razor (UI + logique)
├── Services/       # Services métiers
└── wwwroot/        # Fichiers statiques (CSS, JS)
```

## 🗄️ Base de données

Les migrations sont gérées via Entity Framework Core. En cas de changement de machine :

1. Restaurer la base de données depuis une sauvegarde, ou
2. Relancer `dotnet ef database update` pour recréer la structure

> ⚠️ La chaîne de connexion ne doit jamais être commitée. Elle est gérée via les user-secrets .NET en développement.

## 🚀 Déploiement local

L'application se lance automatiquement sur `http://localhost:44304` et ouvre le navigateur par défaut hors mode Debug.

## 📝 Notes personnelles

- Penser à sauvegarder la base de données régulièrement
- Les user-secrets sont stockés dans `%APPDATA%\Microsoft\UserSecrets\` sous Windows