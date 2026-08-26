using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;

using ACLibrary.Replays;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.UI;
using Framework.Bindables;

using VirtualSteward.Datasources;
using VirtualSteward.Datasources.ViewModels;
using VirtualSteward.Features.CarSelection.ViewModels;
using VirtualSteward.Features.PlayersData.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersList.ViewModels;

public partial class VMPlayer : UIItem,IComparable<VMPlayer>
{
  [Flags]
  public enum ShowCommand
  {
    Expand = 1,
    Edit = 2,
    Delete = 4,
    All = 0xFF
  }
  
  private readonly int _playerID = 0;

  [ObservableProperty] private object? _header;
  [ObservableProperty] private bool _isEditingMode;

  public int UniqueID { get; init; }
  public int PlayerID => _playerID;

  public bool IsNoLapPlayer => Laps.Count == 0 || (Laps.Count == 1 && Laps[0].LapTime == 0);
  public bool HasCalculatedLaptimes
  {
    get
    {
      return Laps.Any( lap => lap is { Calculated: true,LapTime: > 0 } );
    }
  }

  public CarDatasource Datasource { get; }

  public VMPlayerInfo PlayerInfo { get; }
  public VMPlayerData PlayerData { get; }

  public VMMapImage CarImage { get; }
  public VMMapLineStyle LineStyle { get; }
  public VMMapLabelStyle LabelStyle { get; }

  public VMPlayerLapList Laps
  {
    get => field ??= CreateLapsList( );
  } = null;
  public VMPlayerLapList BestLaps
  {
    get => field ??= CreateBestLapsList( );
  } = null;

  public VMPlayerInfoEditing InfoEditing { get; }

  public FeatureCommandList Commands { get; } = [];

  public VMPlayer( int playerID,VMPlayer copyPlayer,CarDatasource datasource,VMMapLabelStyle labelStyle,VMMapLineStyle lineStyle,VMMapImage carImage,ShowCommand commands = ShowCommand.All )
  {
    _playerID = UniqueID = playerID;

    PlayerData = new VMPlayerData( );
    PlayerInfo = new VMPlayerInfo( copyPlayer.PlayerInfo );
    Datasource = datasource;
    
    CarImage = carImage;
    CarImage.BindIsVisible( this );

    LineStyle = lineStyle;
    LabelStyle = labelStyle;

    InfoEditing = new VMPlayerInfoEditing( this );
    
    CreateCommands( commands );
  }
  public VMPlayer( int idPlayer,ReplayCar replayCar,ReplayTail replayTail,VMCarInfo carInfo,VMCarSkinInfo skinInfo,VMMapLabelStyle labelStyle,VMMapLineStyle lineStyle,VMMapImage carImage )
  {
    _playerID = UniqueID = idPlayer;

    PlayerData = new VMPlayerData( );
    PlayerInfo = new VMPlayerInfo( replayCar,carInfo,skinInfo );
    Datasource = new ReplayFileDatasource( replayCar,replayTail );

    CarImage = carImage;
    CarImage.BindIsVisible( this );
    
    LineStyle = lineStyle;
    LabelStyle = labelStyle;

    Laps = LoadLapsTimes( replayCar );

    InfoEditing = new VMPlayerInfoEditing( this );
    
    CreateCommands( ShowCommand.All );
  }
  public VMPlayer( int playerID,string playerName,string playerNation,string playerTeam,string carID,string skinID,CarDatasource? datasource = null )
  {
    _playerID = UniqueID = playerID;
    
    PlayerData = new VMPlayerData( );
    PlayerInfo = new VMPlayerInfo( playerName,playerNation,playerTeam,carID,skinID );

    //_lineColor = LineColors[((PlayerID < 0) ? 0 : PlayerID) % LineColors.Count];
    
    Datasource = datasource ?? new EmptyDatasource( );
    
    CreateCommands( ShowCommand.All );
  }

  public VMPlayerLap? GetLap( uint frame )
  {
    foreach( var lap in Laps )
    {
      if( lap.StartFrame <= frame && lap.EndFrame >= frame )
        return lap;
    }
    return null;
  }
  
  public PointCollection GetLineSegment(uint start, uint end, int maxLength = 24000)
  {
    PointCollection points = [];
    if (end - start < maxLength)
    {
      for (uint i = start; i < end; i++)
      {
        VMCarPosition? pos = Datasource.GetPositionAndRotation(i);
        if (pos != null)
          points.Add(new Point(pos.Position.X, pos.Position.Y));
      }
      //if( bCloseCap && arPoints.Count > 0 )
      //arPoints.Add( arPoints[0] );
    }

    return points;
  }
  public LineCollection GetBrakingLineSegment(uint start, uint end, int maxLength = 24000)
  {
    LineCollection linesPoints = [];
    if (end - start < maxLength)
    {
      PointCollection? points = null;
      for (uint i = start; i < end; i++)
      {
        VMCarPosition? pos = Datasource.GetPositionAndRotation(i);
        if (pos != null)
        {
          if (pos.BrakePedal > 0)
          {
            points ??= [];
            points.Add(new Point(pos.Position.X, pos.Position.Y));
          }
          else
          {
            if (points != null)
            {
              linesPoints.Add(points);
              points = null;
            }
          }
        }
      }

      if (points != null)
      {
        linesPoints.Add(points);
      }
      //if( bCloseCap && arPoints.Count > 0 )
      //arPoints.Add( arPoints[0] );
    }
    return linesPoints;
  }

  public int CompareTo(VMPlayer? obj)
  {
    if (obj == null)
      return 0;
    return _playerID.CompareTo(obj._playerID);
  }

  private void CreateCommands( ShowCommand commands )
  {
    Commands.Clear(  );

    if( (commands & ShowCommand.Expand) == ShowCommand.Expand )
    {
      Commands.Add( new ToggleCommand( )
      {
        Icon = "\xf173",
        Object = this,
        Property = "IsExpanded",
      } );
    }
    if( (commands & ShowCommand.Edit) == ShowCommand.Edit )
    {
      Commands.Add( new ToggleCommand( )
      {
        Icon = "\xf28e",
        Object = this,
        Property = "IsEditingMode",
      } );
    }
    if( (commands & ShowCommand.Delete) == ShowCommand.Delete )
    {
      Commands.Add( new ToggleCommand( )
      {
        Icon = "\xf317",
        Object = this,
        Property = "DeleteItem",
      } );
    }
  }

  private VMPlayerLapList? LoadLapsTimes( ReplayCar replayCar )
  {
    VMPlayerLapList lapsList = new( true );

    uint currentLap = (replayCar.Laps.Length == 0 || replayCar.Laps[0].LapTime == 0) ? (uint)1 : 0;
    uint frames = (uint)Datasource.Length;
    for( uint i = 1; i < frames; i++ )
    {
      uint lapTime = Datasource.GetLapTime( i );
      uint prevLapTime = Datasource.GetLapTime( i - 1 );

      if( i == 1 && prevLapTime != 0 )
      {
        lapsList.Add( new VMPlayerLap( currentLap,0,frames,LineStyle ) );
      }
      // Giri e tempi sul giro
      if( lapTime < prevLapTime || (lapTime > 0 && prevLapTime == 0) )
      {
        if( lapsList.Count > 0 )
        {
          VMPlayerLap lastLap = lapsList[^1];
          lastLap.EndFrame = i - 1;

          if( replayCar.Laps.Length >= lapsList.Count && replayCar.Laps[lapsList.Count - 1].LapTime > 0 )
          {
            lastLap.Calculated = false;
            lastLap.LapTime = replayCar.Laps[lapsList.Count - 1].LapTime;
          }
          else
          {
            lastLap.LapTime = prevLapTime;
          }
        }
        lapsList.Add( new VMPlayerLap( ++currentLap,i,frames,LineStyle ) );
      }
    }
    return lapsList;
  }

  private VMPlayerLapList CreateLapsList()
  {
    VMPlayerLapList lapsList = new( true );

    uint currentLap = 0;
    uint frames = (uint)Datasource.Length;
    for( uint i = 1; i < frames; i++ )
    {
      uint lapTime = Datasource.GetLapTime( i );
      uint prevLapTime = Datasource.GetLapTime( i - 1 );

      //if( pos != null && last != null )
      {
        if( i == 1 && prevLapTime != 0 )
        {
          lapsList.Add( new VMPlayerLap( currentLap,0,frames,LineStyle ) );
        }
        // Giri e tempi sul giro
        if( (lapTime < prevLapTime) || (lapTime > 0 && prevLapTime == 0) )
        {
          if( lapsList.Count > 0 )
          {
            VMPlayerLap lastLap = lapsList[^1];
            lastLap.EndFrame = i - 1;

            //if( pos.LastLapTime > 0 )
            //lastLap.LapTime = pos.LastLapTime;
            //else
            lastLap.LapTime = prevLapTime;
          }
          lapsList.Add( new VMPlayerLap( ++currentLap,i,frames,LineStyle ) );
        }
      }
    }
    return lapsList;
  }
  private VMPlayerLapList CreateBestLapsList()
  {
    List<VMPlayerLap> allLaps = Laps.Where(x => x.LapTime > 0).ToList();

    allLaps.Sort((x, y) => (int)x.LapTime - (int)y.LapTime);

    if (allLaps.Count > 0)
    {
      allLaps[0].IsActive = true;
      allLaps[0].IsSelected = true;
    }
    VMPlayerLapList bestLaps = new(true);
    bestLaps.AddRange(allLaps);
    return bestLaps;
  }
}

public class VMPlayerList( bool multiSelect = false,bool multiActive = false ) : MultiList<VMPlayer>( multiSelect,multiActive )
{
  public uint MaxFrames
  {
    get
    {
      uint max = 0;
      foreach( var player in Items )
      {
        if( player.Datasource.Length > max )
          max = (uint)player.Datasource.Length;
      }
      return max;
    }
  }
}
