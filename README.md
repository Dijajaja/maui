# AppTodoPro (MAUI)

Application de gestion de tâches (todo) multiplateforme construite avec .NET MAUI.

## Prérequis

- .NET SDK installé
- Workload MAUI installé :
  - `dotnet workload install maui`
- Pour Android/iOS : SDKs et émulateurs correspondants (Android Studio/Xcode)

## Démarrage rapide

Depuis la racine du dépôt :

- Restaurer les dépendances :
  - `dotnet restore AppTodoPro/AppTodoPro.csproj`
- Lancer sur une plateforme :
  - Android : `dotnet build AppTodoPro/AppTodoPro.csproj -t:Run -f net10.0-android`
  - Windows : `dotnet build AppTodoPro/AppTodoPro.csproj -t:Run -f net10.0-windows10.0.19041.0`
  - MacCatalyst : `dotnet build AppTodoPro/AppTodoPro.csproj -t:Run -f net10.0-maccatalyst`
  - iOS (macOS requis) : `dotnet build AppTodoPro/AppTodoPro.csproj -t:Run -f net10.0-ios`

## Structure du projet

- `AppTodoPro/` : projet principal MAUI
- `AppTodoPro/Models/` : modèles de données
- `AppTodoPro/ViewModels/` : logique de présentation
- `AppTodoPro/Views/` + `*.xaml` : vues UI
- `AppTodoPro/Services/` : services (auth, todo, etc.)

## Remarques

- Les fichiers générés (`bin/`, `obj/`, `.vs/`) sont ignorés via `.gitignore`.

