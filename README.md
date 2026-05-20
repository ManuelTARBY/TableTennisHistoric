# TableTennisHistoric
Application web d'archivage de matchs de tennis de table avec tableau de bord et statistiques. L'application permet également un suivi de championnats


# Chaîne de connexion
La chaîne de connexion est stockée sur la machine grâce au secret-utilisateur de .NET
Pour la paramétrer, utiliser les lignes de commandes suivantes :
- dotnet user-secrets init
- dotnet user-secrets set "ConnectionStrings:DefaultConnection" "votre_chaine_de_connexion"
