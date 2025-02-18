using AppLauncherMAUI.Config;
using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.Utilities;
using System.Diagnostics;

namespace AppLauncherMAUI.MVVM.ViewModels;

internal partial class SingleAppViewModel : ExtendedBindableObject
{
    public int _appId = 0;
    public int AppId {
        get { return _appId; }
        set { 
            _appId = value; SetData(value);
        }
    }

    public SingleAppViewModel(int appId)
	{
        AppId = appId;
	}

    private async void SetData(int id)
    {
        AppDataModel data = await GetData(id);
        //Debug.WriteLine("should set Data");
        //Debug.WriteLine(data.ToString());
        //Debug.WriteLine(data.Id);
        //Debug.WriteLine(data.Name);
        //Debug.WriteLine(data.Text);
        //Debug.WriteLine(data.SomeRandomNumber);

    }

    private static async Task<AppDataModel> GetData(int id)
    {
        return await JsonFileManager.ReadSingleDataAsync<AppDataModel>(AppPaths.AppsDataJsonName, "Id", id);
    }
}