using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.Templates;
using Framework.Helpers;
using Framework.UI;
using VirtualSteward.Datasources.ViewModels;
using VirtualSteward.Features.PlayersCars.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersCars;

public class PlayersCars : Feature
{
    private VMMap _map;
    private VMTimeline _timeline;
    private VMPlayerList _players;
    
    public PlayersCars(DataTemplates templates,VMMap map,VMTimeline timeline,VMPlayerList players) : base(templates)
    {
        _map = map;
        _timeline = timeline;
        _players = players;
        
        _map.PropertyChanged += Map_PropertyChanged;
        _timeline.PropertyChanged += Timeline_PropertyChanged;

        map.AddLayerNew(new VMPlayersCarsLayer(_players));
    }

    public override Feature AddDataTemplates(DataTemplates templates)
    {
        templates.Add(new FuncDataTemplate<VMPlayersCarsLayer>((_, _) => new Controls.PlayersCars( )));

        return this;
    }

    private void UpdatePlayersCars()
    {
        foreach (var player in _players)
        {
            VMCarPosition? pos = player.Datasource.GetPositionAndRotation(_timeline.CurrentFrame);
            if (pos != null)
            {
                player.CarImage.Position = _map.TrackToCanvas(pos.Position.X, pos.Position.Y);
                player.CarImage.Scale =  _map.Zoom;
                player.CarImage.Rotation = Mathematics.Degrees( pos.Rotation.X );
            }
        }
    }

    private void Map_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not null and VMMap map && e.PropertyName == nameof(VMMap.Offset))
        {
            UpdatePlayersCars();
        }
    }
    private void Timeline_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not null and VMTimeline timeline && e.PropertyName == nameof(VMTimeline.CurrentFrame))
        {
            UpdatePlayersCars();
        }
    }
}