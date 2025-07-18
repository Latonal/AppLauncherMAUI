﻿using AppLauncherMAUI.Config;
using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.MVVM.Views.Controls;
using AppLauncherMAUI.Utilities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AppLauncherMAUI.MVVM.ViewModels;

internal partial class HomeViewModel : ExtendedBindableObject
{
    public ObservableCollection<AppCardView> AppCardList { get; set; }


    public HomeViewModel()
    {
        AppCardList = [];
        GenerateAppsCards();
    }

    private async void GenerateAppsCards()
    {
        List<MiniAppDataModel> adms = await JsonFileManager.ReadDataAsync<List<MiniAppDataModel>>(AppPaths.AppsDataJsonName);
        if (adms == null) return;
        AppCardList.Clear();
        foreach (MiniAppDataModel adm in adms)
        {
            LanguagesModel? texts = adm.Banners?.MiniDescription;
            string desc = (texts != null) ? Common.GetTranslatedJsonText(texts) : "";

            AppCardList.Add(new AppCardView
            {
                AppCardId = adm.Id,
                AppCardMiniBanner = adm.Banners?.Mini ?? "",
                AppCardMiniDescription = desc
            });
    }
}
}
