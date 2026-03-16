using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Media;
using Avalonia.Controls.Templates;

using Framework.UI;
using Framework.UI.Inputs;

using ACLibrary.Tracklines;
using VirtualSteward.Classes;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Tracklines.Values;
using VirtualSteward.Features.Tracklines.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;
using VirtualSteward.ViewModels;

namespace VirtualSteward.Features.Tracklines;

public class Tracklines : StateFeature
{
  private readonly VMMap _map;
  private readonly FilesManager _filesManager;

  private readonly VMTracklineFileList _tracklineFiles = [];
  private readonly VMMapLineNewList _trackLimitsLines  = [];
  
  public VMMapLineStyle LineStyle { get; }
  public TracklineFileValue TracklineFile { get; }
 
  public Tracklines( State state,DataTemplates templates,VMMap map,FilesManager filesManager) : base( state,templates )
  {
    _map = map;
    _filesManager = filesManager;

    TracklineFile = new TracklineFileValue( _tracklineFiles );
    TracklineFile.ValueChanged += OnSelectedTracklineFileChanged;

    map.AddLayer( new VMMapLinesLayer( _trackLimitsLines ) );

    LineStyle = new VMMapLineStyle( 1,Brushes.Gray );
  }

  public override Feature AddDataTemplates( DataTemplates templates )
  {
    templates.Add(new FuncDataTemplate<TracklineFileValue>( (_,_) => new MultiListInput()));

    return this;
  }

  public override Feature AddFooter(UIBaseList pages, string? headerTitle = null)
  {
    pages.Add( TracklineFile );

    return this;
  }

  public override void OnReplayChanged( VMReplay replay )
  {
    _trackLimitsLines.Clear(  );
  }
  public override void OnTrackChanged( VMTrackInfo trackInfo )
  {
     LoadTracklinesFiles( trackInfo );
  }

  private void LoadTracklinesFiles( VMTrackInfo trackInfo )
  {
    _tracklineFiles.Clear( );

    var files = Trackline.GetTracklinesFiles( _filesManager.ACTracksFolder,trackInfo.TrackID,trackInfo.VariantID );
    
    foreach( var file in files )
    {
      _tracklineFiles.Add( new VMTracklineFile( file ) );
    }
    var fastLane = _tracklineFiles.FindFile( "fast_lane.ai" );
    if( fastLane != null )
    {
      fastLane.IsSelected = true;
    }
    else if( _tracklineFiles.Count > 0 )
    {
      _tracklineFiles[0].IsSelected = true;
    }
    //_state.TracklinesLoaded = true;
  }

  private async void OnSelectedTracklineFileChanged(VMTracklineFile? file)
  {
    _trackLimitsLines.Clear( );

    if( file != null )
    {
      file.Lines ??= await LoadTracklineAsync( file.FileFullPath,true,null );

      if( file.Lines != null )
      {
        (VMTrackline? leftSide,VMTrackline? rightSide) = CreateTrackLimits( file );
        if( leftSide != null )
        {
          _trackLimitsLines.Add( new VMMapLineNew( leftSide.GetLinePoints( 0,0 ),LineStyle ) );
        }
        if( rightSide != null )
        {
          _trackLimitsLines.Add( new VMMapLineNew( rightSide.GetLinePoints( 0,0 ),LineStyle ) );

          if( ((double)_map.Display.Height) / rightSide.Height * 0.9f < ((double)_map.Display.Width) / rightSide.Width * 0.9f )
            _map.Zoom = ((double)_map.Display.Height) / rightSide.Height * 0.9f;
          else
            _map.Zoom = ((double)_map.Display.Width) / rightSide.Width * 0.9f;
          _map.CenterOn = new Point( (rightSide.Left + rightSide.Right) / 2.0f,(rightSide.Top + rightSide.Bottom) / 2.0f );

          _map.UpdateLayers( );
        }
      }
    }
  }

  private static async Task<VMTracklineList?> LoadTracklineAsync( string filename,bool selectedLines,IProgress<float>? progress )
  {
    try
    {
      using IsWorking loading = new( IsWorking.Tasks.TracklinesLoading );

      progress?.Report( 0 );
      List<Trackline> toAdds = [];
      List<Trackline> lines = await Task.Run( ( ) => Trackline.LoadTracklines( filename,progress ) );
      //List<Trackline> lines = Trackline.LoadTracklines( filename,progress );

      foreach( Trackline trackline in lines )
      {
        if( trackline.Filename == "" )
          continue;

        if( trackline.ConnectToEnd != string.Empty )
        {
          string identifier = trackline.ConnectToEnd;
          int separator = identifier.IndexOf( '@' );
          string splineName = identifier.Substring( 0,separator);
          int id = int.Parse(identifier.Substring( separator + 1 ) );

          if( id == 0 )
          {
            foreach( var toJoin in lines )
            {
              if( toJoin.Filename.StartsWith( splineName ) )
              {
                trackline.Filename += " + " + toJoin.Filename.Replace( "fast_lane","" ).Replace( "fastlane","" );
                trackline.Datas = [.. trackline.Datas,.. toJoin.Datas];

                toJoin.Filename = "";

                toAdds.Remove( toJoin );

                break;
              }
            }
          }
        }
        toAdds.Add( trackline );
      }
      var trackLines = new VMTracklineList( );
      foreach( Trackline trackline in toAdds )
      {
        VMTrackline newTrackline = new ( trackline.Filename,trackline );

        trackLines.Add( newTrackline );

        newTrackline.IsSelected = selectedLines;
      }
      progress?.Report( -1 );

      return trackLines;
    }
    catch( TaskAlreadyRunning  )
    {
      //logger?.Error( "Task already running: {message}",tx.Task );
    }
    catch( Exception )
    {
      //logger?.Error( "Error in LoadTracklineAsync: {Message}",ex.Message );

      progress?.Report( -2 );
    }
    return null;
  }

  public static (VMTrackline? left, VMTrackline? right) CreateTrackLimits( VMTrackline trackline )
  {
    VMTracklineDataList left = [];
    VMTracklineDataList right = [];

    for( int i = 0; i < trackline.Data.Count - 1; i++ )
    {
      VMTracklineData pos = trackline.Data[i];

      float x = pos.Position.X;
      float y = pos.Position.Y;

      float direction = (float)(-Math.Atan2( trackline.Data[i+1].Position.Y - y,
        trackline.Data[i+1].Position.X - x ));
      if( pos.SideLeft > 0 )
      {
        float leftX = (float)(x - Math.Cos( -direction + (Math.PI / 2) ) * pos.SideLeft);
        float leftY = (float)(y - Math.Sin( -direction + (Math.PI / 2) ) * pos.SideLeft);

        left.Add( new VMTracklineData( new System.Numerics.Vector3( leftX,leftY,0 ) ) );
      }
      if( pos.SideRight > 0 )
      {
        float rightX = (float)(x - Math.Cos( -direction - (Math.PI / 2) ) * pos.SideRight);
        float rightY = (float)(y - Math.Sin( -direction - (Math.PI / 2) ) * pos.SideRight);

        right.Add( new VMTracklineData( new System.Numerics.Vector3( rightX,rightY,0 ) ) );
      }
    }
    if( left.Count > 0 )
    {
      left.Add( left[0] );
      if( right.Count > 0 )
      {
        left.Insert( 0,right[0] );

        right.Add( right[0] );
        right.Insert( 0,left[1] );
      }
    }
    else if( right.Count > 0 )
    {
      right.Add( right[0] );
    }
    return (left.Count > 0 ? new VMTrackline( "Left side",left,true ) : null, right.Count > 0 ? new VMTrackline( "Right side",right,true ) : null);
  }
  public static (VMTrackline? left, VMTrackline? right) CreateTrackLimits( VMTracklineFile tracklineFile )
  {
    VMTracklineDataList left = [];
    VMTracklineDataList right = [];

    foreach( var trackline in tracklineFile.Lines )
    {
      (VMTrackline? leftSide, VMTrackline? rightSide) = CreateTrackLimits( trackline );

      if( leftSide != null )
        left.AddRange( leftSide.Data );
      if( rightSide != null )
        right.AddRange( rightSide.Data );
    }
    return (left.Count > 0 ? new VMTrackline( "Left side",left,true ) : null, right.Count > 0 ? new VMTrackline( "Right side",right,true ) : null);
  }
}
