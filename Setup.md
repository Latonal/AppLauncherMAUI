Will be formated at a later date

- Update variables in Config/AppConfig.cs to customize the application
- Change content in Resources/Raw/apps_data.json to your apps to download/manage
- For localization, add the supported languages in MVVM/Models/LanguagesModel.cs, in the enum in Utilities/Common.cs, in Resources/Raw/apps_data.json and in AppResources.resx (add a new AppResources file for a new language).

# Customize your application quickly

- Update AppLauncherMaui/Config/AppConfig.cs

# Set your own applications to download

- Update AppLauncherMaui/Resources/Raw/apps_data.json accordingly to need (do not reference a json coming from Github yet)
- Application downloaded must have a version.txt at the root if you want to update it later on

# The updater

- Set the link correctly since the updater can't be updated with the current version of the main app