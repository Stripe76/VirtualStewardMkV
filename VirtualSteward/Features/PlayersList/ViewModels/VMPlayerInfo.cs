using CommunityToolkit.Mvvm.ComponentModel;

using ACLibrary.Replays;

using Framework.UI;

using VirtualSteward.Features.CarSelection.ViewModels;

namespace VirtualSteward.Features.PlayersList.ViewModels;

public partial class VMPlayerInfo : UIItem
{
    [ObservableProperty] private string _playerName;
    [ObservableProperty] private string _playerTeam;
    [ObservableProperty] private string _playerNation;

    public VMCarInfo CarInfo { get; }
    public VMCarSkinInfo CarSkinInfo { get; set; }

    public VMPlayerInfo( ReplayCar replayCar,VMCarInfo carInfo,VMCarSkinInfo skinInfo )
    {
        _playerName = replayCar.PlayerName;
        _playerTeam = replayCar.PlayerTeam;
        _playerNation = replayCar.PlayerNation;

        CarInfo = carInfo;
        CarSkinInfo = skinInfo;
    }

    public override string ToString()
    {
        return $"({PlayerNation}) {CarInfo} - {CarSkinInfo}";
    }
}
