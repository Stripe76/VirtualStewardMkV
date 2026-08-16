using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.Input;
using Framework.Helpers;
using Framework.UI;
using VirtualSteward.Classes;
using VirtualSteward.Datasources.ViewModels;
using VirtualSteward.Features.PlayersCars.EditingTools;
using VirtualSteward.Features.PlayersCars.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersCars;

public partial class PlayersCars : StateFeature
{
    private readonly VMMap _map;
    private readonly VMTimeline _timeline;
    private readonly VMPlayerList _players;
    private readonly VMPlayersCarsLayer _carsLayer;

    public PlayersCars( State state,DataTemplates? templates,VMMap map,VMTimeline timeline,VMPlayerList players ) : base( state,templates,map,timeline )
    {
        _map = map;
        _players = players;
        _timeline = timeline;

        map.AddLayer( _carsLayer = new VMPlayersCarsLayer( _players ) );
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<VMPlayersCarsLayer>( ( _,_ ) => new Controls.PlayersCars( ) ) );

        return this;
    }

    public override void OnMapChange( VMMap map )
    {
        UpdatePlayersCars( );
    }
    public override void OnTimelineChange( VMTimeline timeline,StateFeature.TimelineChangeType type )
    {
        if( type == TimelineChangeType.CurrentFrame )
            UpdatePlayersCars( );
        else if( type == TimelineChangeType.IsActive )
            _carsLayer.IsVisible = _timeline.IsActive;
    }

    private void UpdatePlayersCars( )
    {
        foreach( var player in _players )
        {
            VMCarPosition? pos = player.Datasource.GetPositionAndRotation( _timeline.CurrentFrame );
            if( pos != null )
            {
                //player.CarImage ??= new VMMapImage( _filesManager.GetCarImage( player.PlayerInfo.CarInfo.CarID,player.PlayerInfo.CarInfo.SkinID,player.LineStyle.Color ) );
                player.CarImage.PointerPressed ??= PlayerSelectedCommand;

                player.CarImage.Position = _map.TrackToCanvas( pos.Position.X,pos.Position.Y );
                player.CarImage.Scale = _map.Zoom;
                player.CarImage.Rotation = Mathematics.Degrees( pos.Rotation.X );
            }
        }
    }

    [RelayCommand]
    private void PlayerSelected( VMPlayer player )
    {
        _map.EditingTool = new PlayerCarEdit( player,_timeline );
    }
}