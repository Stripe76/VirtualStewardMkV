using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

using Framework.UI;

using VirtualSteward.Features.Server.Classes;

namespace VirtualSteward.Pages.Server.ViewModels;

public partial class VMServerStatus : UIBase
{
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isStarting;
    
    public VMServerStatus(  )
    {
    }

    public void SetServerManager( ServerManager serverManager )
    {
        serverManager.PropertyChanged += ServerManager_PropertyChanged;

        IsRunning = serverManager.IsRunning;
        IsPlaying = serverManager.IsPlaying;
    }

    private void ServerManager_PropertyChanged( object? sender,PropertyChangedEventArgs e )
    {
        if( sender is not null and ServerManager serveManager )
        {
            IsRunning = serveManager.IsRunning;
            IsPlaying = serveManager.IsPlaying;
        }
    }
}