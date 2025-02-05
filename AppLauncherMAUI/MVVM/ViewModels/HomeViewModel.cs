using AppLauncherMAUI.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppLauncherMAUI.MVVM.ViewModels;

internal partial class HomeViewModel : ExtendedBindableObject
{
    private int _clicksReceived = 0;
    public int ClicksReceived { get {  return _clicksReceived; } set { _clicksReceived = value; RaisePropertyChanged(() => ClicksReceived); } }
    public ICommand ActionClickedCommand { get; set; }


    public HomeViewModel()
    {
        //Message = "Test";
        //ChangeMessageCommand = new Command(ChangeMessage);
        ActionClickedCommand = new Command(OnActionClicked);
    }

    private void OnActionClicked(object obj)
    {
        ClicksReceived++;
    }

    //private void ChangeMessage()
    //{
    //    Message = "eeee";
    //}

    //public ICommand ChangeMessageCommand { get; }

}
