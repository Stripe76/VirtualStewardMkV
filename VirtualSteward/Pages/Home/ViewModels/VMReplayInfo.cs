using System;
using System.IO;
using ACLibrary.Replays;

using Framework.UI;
using Framework.Bindables;

using VirtualSteward.ViewModels;
using VirtualSteward.Features.CarSelection.ViewModels;

namespace VirtualSteward.Pages.Home.ViewModels;

public class VMReplayInfo : UIItem
{
  private readonly ReplayInfo _replay;
  private readonly VMCarInfo _carInfo;
  private readonly VMTrackInfo _trackInfo;
  private readonly VMCarSkinInfo _carSkinInfo;

  private readonly string _today = DateTime.Now.ToShortDateString( );
  private readonly string _yesterday = DateTime.Now.AddDays(-1).ToShortDateString( );

  private string? _cspSettingsFile = null;

  public string FileFullPath
  {
    get => _replay.FileFullPath;
  }

  public string Title
  {
    get => _replay.FileName;
  }

  public string FileName
  {
    get => _replay.FileName;
    set => SetProperty( ref _replay.FileName, value );
  }
  public string PlayerName
  {
    get => _replay.PlayerName;
  }
  public string TrackName
  {
    get => _trackInfo.TrackName;
  }
  public string CarName
  {
    get => _carInfo.Model;
  }
  public string Weather
  {
    get => _replay.Weather;
    set => SetProperty( ref _replay.Weather,value );
  }
  public string BestLapText
  {
    get 
    {
      return LapTimeToString( _replay.BestLap );
    }
  }
  public string AverageLapText
  {
    get
    {
      return LapTimeToString( _replay.AverageLap );
    }
  }
  public string Details
  {
    get 
    {
      string sDetails = String.Empty;

      sDetails += String.Format( "{0}",_trackInfo.TrackName );
      sDetails += String.Format( " - {0} ({1} hz)",_carInfo.Model,(int)(1000 / Frequency) );
      sDetails += String.Format( "\r\nBest lap: {0}",BestLapText );
      sDetails += String.Format( " - Average lap: {0}",AverageLapText );
      sDetails += String.Format( " - Laps number: {0}",_replay.LapsNumber );
      sDetails += String.Format( "\r\nDate: {0} {1}",_replay.Date.ToShortDateString( ),_replay.Date.ToShortTimeString( ) );

      return sDetails;
    }
  }
  public string MonthGrouping
  {
    get
    {
      if( _replay.Date.ToShortDateString( ).Equals( _today ) )
        return " Today";
      if( _replay.Date.ToShortDateString( ).Equals( _yesterday ) )
        return " Yesterday";

      return _replay.Date.ToString( "yyyy/MM MMMM" ); 
    }
  }

  public int TailDataRecords { get; set; }
  public int TailDataVersion { get; set; }

  public string? CSPSettingsFilePath
  {
    get => _cspSettingsFile;
    set
    {
      if( SetProperty( ref _cspSettingsFile,value ) )
        OnPropertyChanged( nameof( CSPSettingsFileName ) );
    }
  }
  public string? CSPSettingsFileName
  {
    get => _cspSettingsFile != null ? Path.GetFileName( _cspSettingsFile ) : "Not found";
  }

  public double Frequency
  {
    get => _replay.Frequency;
  }

  public DateTime ReplayDate
  {
    get => _replay.Date;
  }

  public VMCarInfo CarInfo 
  {
    get => _carInfo;
  }
  public VMTrackInfo TrackInfo 
  {
    get => _trackInfo;
  }
  public VMCarSkinInfo CarSkinInfo 
  {
    get => _carSkinInfo;
  }

  public VMReplayInfo( ReplayInfo replayInfo,VMTrackInfo trackInfo,VMCarInfo carInfo,VMCarSkinInfo carSkinInfo )
  {
    //_isLoaded = true;

    _replay = replayInfo;
    _carInfo = carInfo;
    _trackInfo = trackInfo;
    _carSkinInfo = carSkinInfo;
  }

  public static string LapTimeToString( int nTime )
  {
    return String.Format( "{0:00}:{1:00}:{2:000}",nTime / 60000,nTime / 1000 % 60,nTime % 1000 );
  }
}

public class VMReplayInfoList : ObservableCollectionEx<VMReplayInfo>
{
}