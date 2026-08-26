using System.Collections.ObjectModel;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.UI;
using VirtualSteward.Classes;
using VirtualSteward.Datasources.ViewModels;
using VirtualSteward.Features.PlayersData.Configurations;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;

namespace VirtualSteward.Features.PlayersData;

public partial class PlayersData : StateFeature
{
    private readonly VMTimeline _timeline;

    [ObservableProperty] private bool _lapAutoSelect = false;

    public ObservableCollection<VMPlayer> Players { get; }

    public PlayersDataOptions Options { get; }

    public PlayersData( State state,DataTemplates? templates,VMTimeline timeline,VMPlayerList players ) : base( state,templates,null,timeline )
    {
        _timeline = timeline;
            
        Players = players.SelectedItems;

        players.SelectedItemChanged += ( sender,player ) => UpdateVisibility( );

        Options = new PlayersDataOptions( this );
        Options.DataVisible.Value = true;

        UpdateVisibility( );
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<PlayersData>( (_,_) => new Pages.PlayersData(  ) ) );
        templates.Add( new FuncDataTemplate<PlayersDataOptions>( ( _,_ ) => new Framework.UI.Panels.BaseConfiguration( ) ) );

        return this;
    }
    public override Feature AddFooter( UIBaseList pages,string? headerTitle = null )
    {
        pages.Add( Options );
        
        return this;
    }
    
    public override void OnTimelineChange( VMTimeline timeline,StateFeature.TimelineChangeType type )
    {
        if( type == TimelineChangeType.CurrentFrame )
            UpdatePlayersData( timeline );
        else if( type == TimelineChangeType.IsActive )
            UpdateVisibility( );
    }

    public void UpdateVisibility( )
    {
        IsVisible = Players.Count > 0 && _timeline.IsActive && Options.DataVisible.Value;
    }
    
    private void UpdatePlayersData( VMTimeline timeline )
    {
        foreach( var player in Players )
        {
            VMCarData? pos = player.Datasource.GetCarData( timeline.CurrentFrame );
            if( pos != null )
            {
                //player.CarImage ??= new VMMapImage( _filesManager.GetCarImage( player.PlayerInfo.CarInfo.CarID,player.PlayerInfo.CarInfo.SkinID,player.LineStyle.Color ) );

                player.PlayerData.Rpm = pos.RPMs;
                player.PlayerData.Steering = pos.SteeringWheel;
                player.PlayerData.GasPedal = pos.GasPedal;
                player.PlayerData.BrakePedal = pos.BrakePedal;
                
                int gear = pos.Gear-1;
                if( gear < 0 )
                    player.PlayerData.Gear = "R";
                player.PlayerData.Gear = gear == 0 ? "N" : gear.ToString( );
            }
            if( LapAutoSelect && player.IsSelected )
            {
                VMPlayerLap? lap = player.GetLap( timeline.CurrentFrame );
                if( lap != null )
                {
                    player.Laps.MultiSelectedEnabled = false;
                    player.Laps.SelectedItem = lap;
                    player.Laps.MultiSelectedEnabled = true;
                }
            }
        }
    }
}
