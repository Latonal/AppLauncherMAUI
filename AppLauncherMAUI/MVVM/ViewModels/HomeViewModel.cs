using AppLauncherMAUI.Config;
using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.MVVM.Views.Controls;
using AppLauncherMAUI.Utilities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AppLauncherMAUI.MVVM.ViewModels;

internal partial class HomeViewModel : ExtendedBindableObject
{
    private int _clicksReceived = 0;
    public int ClicksReceived { get { return _clicksReceived; } set { _clicksReceived = value; RaisePropertyChanged(() => ClicksReceived); } }
    public ICommand ActionClickedCommand { get; set; }
    public ObservableCollection<AppCard> AppCardList { get; set; }


    public HomeViewModel()
    {
        ActionClickedCommand ??= new Command(OnActionClicked);
        AppCardList = [];
        GenerateAppsCards();
    }

    private void OnActionClicked(object obj)
    {
        ClicksReceived++;
    }

    private async void GenerateAppsCards()
    {
        List<AppDataModel> adms = await JsonFileManager.ReadDataAsync<List<AppDataModel>>(AppPaths.AppsDataJsonName);
        if (adms == null) return;
        AppCardList.Clear();
        foreach (AppDataModel adm in adms)
        {
            AppCardList.Add(new AppCard
            {
                AppCardId = adm.Id,
                AppCardText = adm.Name ?? "",
            });
        }
    }
}
