using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Framework.UI;
using VirtualSteward.Features.CarSelection.ViewModels;

namespace VirtualSteward.Features.PlayersList.ViewModels;

public partial class VMPlayerInfoEditing( VMPlayer player ) : UIBase
{
    [ObservableProperty] private string _playerName = player.PlayerInfo.PlayerName;
    [ObservableProperty] private VMCarSkinInfo _selectedSkin = player.PlayerInfo.CarSkinInfo; 

    public VMCarSkinInfoList Skins => player.PlayerInfo.CarInfo.CarSkinsList;
    
    [RelayCommand] protected void Accept( )
    {
        player.PlayerInfo.PlayerName = PlayerName;
        player.PlayerInfo.CarSkinInfo = SelectedSkin;
        
        player.IsEditingMode = false;
    }
    [RelayCommand] protected void Cancel( )
    {
        PlayerName = player.PlayerInfo.PlayerName;
        SelectedSkin = player.PlayerInfo.CarSkinInfo;
        
        player.IsEditingMode = false;
    }
}