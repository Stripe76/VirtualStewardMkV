using System.Linq;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.UI;
using Framework.UI.Configurations;
using Framework.UI.Values;
using VirtualSteward.Classes;
using VirtualSteward.Features.ReplayLoading.ViewModels;

namespace VirtualSteward.Features.PlayersMessage;

public partial class PlayersMessage : StateFeature
{
    [ObservableProperty] private bool _isVisible;

    public PlayersMessageOptions Options { get; }

    public PlayersMessage( State state,DataTemplates templates ) : base( state,templates )
    {
        AddConfiguration( Options = new PlayersMessageOptions( this ) );
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<PlayersMessage>( (_,_) => new Pages.PlayersMessage(  ) ) );

        return this;
    }

    public override void OnReplayChanged( VMReplay replay )
    {
        if( Options.ShowCalculatedLaptimes )
        {
            IsVisible = _state.Players.Any( player => player.HasCalculatedLaptimes );
        }
        else
        {
            IsVisible = false;
        }
    }
}

public class PlayersMessageOptions( PlayersMessage playersMessage ) : Configuration( "PLAYERS_MESSAGE" )
{
    public BaseBool ShowCalculatedLaptimes = new BaseBool( "CalculatedLaptimes","Calculated laptimes" )
    {
        Value = true,
        ValueChanged = ( value ) => playersMessage.IsVisible = value
    };
}