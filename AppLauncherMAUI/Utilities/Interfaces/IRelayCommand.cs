using System.Windows.Input;

namespace AppLauncherMAUI.Utilities.Interfaces;

// From: https://github.com/CommunityToolkit/dotnet/blob/main/src/CommunityToolkit.Mvvm/Input/Interfaces/IRelayCommand.cs

public interface IRelayCommand : ICommand
{
    void NotifyCanExecuteChanged();
}
