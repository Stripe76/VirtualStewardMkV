using CommunityToolkit.Mvvm.ComponentModel;
using Framework.UI;

namespace VirtualSteward.Features.PlayersData.ViewModels;

public partial class VMPlayerData : UIItem
{
    private float _gasPedal;
    private float _brakePedal;

    [ObservableProperty] private float _rpm;
    [ObservableProperty] private string _gear = "";
    [ObservableProperty] private float _steering;

    public float GasPedal
    {
        get => _gasPedal;
        set
        {
            if( SetProperty( ref _gasPedal,value ) )
                OnPropertyChanged( nameof( GasBarHeight ) );
        }
    }
    public float BrakePedal
    {
        get => _brakePedal;
        set
        {
            if( SetProperty( ref _brakePedal,value ) )
                OnPropertyChanged( nameof( BrakeBarHeight ) );
        }
    }

    public float GasBarHeight
    {
        get
        {
            return _gasPedal * 50.0f;
            //rClutchPedal.Height = ((float)0 / 255f) * 60;
            //rBrakePedal.Height = ((float)pos.nBrakePedal / 255f) * 60;
            //rGasPedal.Height = ((float)pos.nGasPedal / 255f) * 60;            
        }
    }
    public float BrakeBarHeight
    {
        get
        {
            return _brakePedal * 50.0f;
        }
    }
}