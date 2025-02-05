using AppLauncherMAUI.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppLauncherMAUI.Utilities;

// From: https://github.com/CommunityToolkit/dotnet/blob/main/src/CommunityToolkit.Mvvm/Input/RelayCommand.cs

public sealed partial class RelayCommand : IRelayCommand
{
    private readonly Action execute;
    private readonly Func<bool>? canExecute;
    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action execute)
    {
        ArgumentNullException.ThrowIfNull(execute);

        this.execute = execute;
    }

    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);

        this.execute = execute;
        this.canExecute = canExecute;
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanExecute(object? parameter)
    {
        return canExecute?.Invoke() != false;
    }

    public void Execute(object? parameter)
    {
        execute();
    }
}