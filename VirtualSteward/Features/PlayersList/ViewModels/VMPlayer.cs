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
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersList.ViewModels;

public partial class VMPlayer : UIItem,IComparable<VMPlayer>
{
  private readonly int _playerID = 0;

  [ObservableProperty] private bool _isEditingMode;
  
  public CarDatasource Datasource { get; }

  public VMPlayerInfo PlayerInfo { get; }
  public VMMapImage CarImage { get; }
  public VMMapLineStyle LineStyle { get; }

  public VMPlayerLapList Laps
  {
    get => field ??= CreateLapsList( );
  } = null;
  public VMPlayerLapList BestLaps
  {
    get => field ??= CreateBestLapsList( );
  } = null;

  public VMPlayerInfoEditing InfoEditing { get; }

  public VMPlayer( int idPlayer,ReplayCar replayCar,ReplayTail replayTail,VMCarInfo carInfo,VMCarSkinInfo skinInfo,VMMapLineStyle lineStyle,VMMapImage carImage )
  {
    _playerID = idPlayer;

    PlayerInfo = new VMPlayerInfo( replayCar,carInfo,skinInfo );
    Datasource = new ReplayFileDatasource( replayCar,replayTail );

    CarImage = carImage;
    CarImage.BindIsVisible( this );
    
    LineStyle = lineStyle;

    InfoEditing = new VMPlayerInfoEditing( this );
  }

  private VMPlayerLapList CreateLapsList()
  {
    VMPlayerLapList lapList = new(true);

    uint currentLap = 0;
    uint frames = (uint)Datasource.Length;
    for (uint i = 1; i < frames; i++)
    {
      uint lapTime = Datasource.GetLapTime(i);
      uint prevLapTime = Datasource.GetLapTime(i - 1);

      //if( pos != null && last != null )
      {
        if (i == 1 && prevLapTime != 0)
        {
          lapList.Add(new VMPlayerLap(currentLap, 0, frames,LineStyle));
        }

        // Giri e tempi sul giro
        if ((lapTime < prevLapTime) || (lapTime > 0 && prevLapTime == 0))
        {
          if (lapList.Count > 0)
          {
            VMPlayerLap lastLap = lapList[^1];
            lastLap.EndFrame = i - 1;

            //if( pos.LastLapTime > 0 )
            //lastLap.LapTime = pos.LastLapTime;
            //else
            lastLap.LapTime = prevLapTime;
          }
          lapList.Add(new VMPlayerLap(++currentLap, i, frames,LineStyle) );
        }
      }
    }
    return lapList;
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
}

public class VMPlayerList( bool multiSelect = false,bool multiActiveWithCtrl = false ) : MultiList<VMPlayer>( multiSelect,false,multiActiveWithCtrl )
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

