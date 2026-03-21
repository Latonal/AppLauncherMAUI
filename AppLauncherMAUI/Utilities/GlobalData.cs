using AppLauncherMAUI.MVVM.Models;

namespace AppLauncherMAUI.Utilities
{
    public static class GlobalData
    {
        public static DomainDownloadModelList DDMList { get; set; } = new();
        public static AppUpdateInfosModelList AUIMList { get; set; } = new();
    }
}
