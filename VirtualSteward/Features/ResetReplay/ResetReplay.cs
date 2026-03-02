using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using Framework.UI;
using VirtualSteward.Classes;
using VirtualSteward.Features.ReplayLoading.ViewModels;

namespace VirtualSteward.Features.ResetReplay;

public partial class ResetReplay( State state ) : StateFeature( state )
{
  public override Feature AddCommands( UIItemList commands )
  {
    commands?.Add(
      new FeatureCommand( )
      {
        Icon = "\xf344",
        Text = "Reset",
        Tooltip = "Reset",
        RoutedCommand = ReplayResetCommand
      }
    );
    //routingCtrl?.CommandBindings.Add( new CommandBinding( Command_ResetReplay,ReplayReset,ReplayReset_CanExecute ) );
    return this;
  }

  public override void OnReplayChanged( VMReplay replay )
  {
    ReplayResetCommand.NotifyCanExecuteChanged( );
  }

  [RelayCommand(CanExecute = nameof(CanReplayReset))] public void ReplayReset( )
  {
    //Realtime.Stop( );
    //Server.StopServer( );

    _state.Replay = new VMReplay( );
    _state.Players.Clear( );
    //_state.MergedPlayers.Clear( );
    
    //_state.TracklineFiles.Clear( );
    //_state.TracklineLimits.Clear( );

    //LapsMergeMode = false;
    ReplayResetCommand.NotifyCanExecuteChanged( );
  }
  private bool CanReplayReset( )
  {
    return _state.Replay.IsLoaded;
  }
}
