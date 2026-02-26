using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Avalonia.Controls.Templates;

using Framework.UI;
using Framework.UI.Inputs;

using ACLibrary.Tracklines;
using Avalonia;
using VirtualSteward.Classes;
using VirtualSteward.Features.Tracklines.Values;
using VirtualSteward.Features.Tracklines.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;
using VirtualSteward.ViewModels;

namespace VirtualSteward.Features.Tracklines;

public class Tracklines : StateFeature
{
  private readonly VMMap? _map;
  private readonly FilesManager _filesManager;

  private readonly VMTracklineFileList _tracklineFiles = [];
  private readonly VMTracklineList _tracklineLimits = new VMTracklineList(true);

  private readonly VMMapLayer? _limitsLayer = null,_tracklinesLayer = null;

  public TracklineFileValue TracklineFile { get; }
  
  /*
  public ObservableCollection<VMMapLayerSelector> AdditionalLayers
  {
    get => _additionalLayers;
  }
  */
  
  public VMTracklineList TracklineLimits
  {
    get => _tracklineLimits;
  }
  public VMTracklineFileList TracklineFiles
  {
    get => _tracklineFiles;
  }

  public Tracklines( State state,DataTemplates templates,VMMap? map,FilesManager filesManager) : base( state,templates )
  {
    _map = map;
    _filesManager = filesManager;

    TracklineFile = new TracklineFileValue( _tracklineFiles );
    TracklineFile.ValueChanged += OnSelectedTracklineFileChanged;

    _map?.AddLayer( _limitsLayer = new VMLayerTrackline( _tracklineLimits.SelectedItems ),true );
    _map?.AddLayer( _tracklinesLayer = new VMLayerTracklineFile( _tracklineFiles.SelectedItems ) { IsVisible = false },true );
  }

  public override void AddDataTemplates( DataTemplates templates )
  {
    templates.Add(new FuncDataTemplate<TracklineFileValue>( (_,_) => new MultiListInput()));
  }

  public override Feature AddFooter(UIBaseList pages, string? headerTitle = null)
  {
    pages.Add(TracklineFile);

    return this;
  }

  public override void OnTrackChanged( VMTrackInfo trackInfo )
  {
    LoadTracklinesFiles( trackInfo,null );
  }

  private void OnSelectedTracklineFileChanged(VMTracklineFile? file)
  {
    _tracklineLimits.Clear( );

    if (file != null)
    {
      (VMTrackline? leftSide, VMTrackline? rightSide) = CreateTrackLimits(file);
      if (leftSide != null)
        _tracklineLimits.Add(leftSide, true);
      if (rightSide != null)
      {
        _tracklineLimits.Add(rightSide, true);

        if (_map != null)
        {
          if (((double)_map.Display.Height) / rightSide.Height * 0.9f <
              ((double)_map.Display.Width) / rightSide.Width * 0.9f)
            _map.Zoom = ((double)_map.Display.Height) / rightSide.Height * 0.9f;
          else
            _map.Zoom = ((double)_map.Display.Width) / rightSide.Width * 0.9f;
          _map.CenterOn = new Point((rightSide.Left + rightSide.Right) / 2.0f, (rightSide.Top + rightSide.Bottom) / 2.0f);
        }
      }
    }
  }

  private async void LoadTracklinesFiles( VMTrackInfo trackInfo,IProgress<float>? progress )
  {
    _tracklineFiles.Clear( );

    progress?.Report( 0 );
    {
      var files = await Task.Run( () => Trackline.GetTracklinesFiles( _filesManager.ACTracksFolder,trackInfo.TrackID,trackInfo.VariantID ) );
      
      foreach( var file in files )
      {
        VMTracklineFile newTrackline = new ( file );
        //if( newTrackline.FileName.Equals( "fast_lane.ai" ) )
        //_ = newTrackline.Lines;
        _tracklineFiles.Add( newTrackline );
      }
    }
    progress?.Report( -1 );

    foreach( var file in _tracklineFiles )
    {
      if( file.FileName.Equals( "fast_lane.ai" ) )
      {
        file.IsSelected = true;
        file.LineColor = VMTracklineFile.LineColors[8];
      }
      else if( file.FileName.StartsWith( "pit_lane" ) )
      {
        file.LineColor = VMTracklineFile.LineColors[1];
      }
    }
    //_state.TracklinesLoaded = true;
  }

  public static async Task LoadTracklineAsync( string filename,IList<VMTrackline>? trackLines,bool selectedLines,IProgress<float> progress )
  {
    try
    {
      using IsWorking loading = new( IsWorking.Tasks.TracklinesLoading );

      progress.Report( 0 );
      List<Trackline> toAdds = [];
      List<Trackline> lines = await Task.Run( ( ) => Trackline.LoadTracklines( filename,progress ) );

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
      if( trackLines != null )
      {
        foreach( Trackline trackline in toAdds )
        {
          VMTrackline newTrackline = new ( trackline.Filename,trackline );

          trackLines.Add( newTrackline );

          newTrackline.IsSelected = selectedLines;
        }
      }
      progress.Report( -1 );
    }
    catch( TaskAlreadyRunning  )
    {
      //logger?.Error( "Task already running: {message}",tx.Task );
    }
    catch( Exception )
    {
      //logger?.Error( "Error in LoadTracklineAsync: {Message}",ex.Message );

      progress.Report( -2 );
    }
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
