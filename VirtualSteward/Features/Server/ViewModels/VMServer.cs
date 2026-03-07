using Framework.Bindables;

using VirtualSteward.ACNetwork.Shared;
using VirtualSteward.ACNetwork.Weather;
using VirtualSteward.ViewModels;
using VirtualSteward.Features.CarSelection.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Server.Classes;
using VirtualSteward.Features.Server.Configurations;

namespace VirtualSteward.Features.Server.ViewModels;

public enum ServerType
{
  TypeA = 0,
  TypeB = 1,
  TypeC = 2,
}

public class VMServer : BindableBase
{
  private readonly VMPlayerList _players;
  private readonly VMTrackObjects _trackObjects;
  private readonly VMServerDebug _serverDebug;
  private readonly VMCarInfoList _additionalCars;

  private VMReplay _replay = new( );
  private VMTrackInfo _track = new( "","" );

  private ACServerSettings _settings = new( );
  private ServerManager? _serverManager = null;

  private string _serverName = "Virtual Steward";
  private ServerType _serverType = ServerType.TypeA;

  private bool _showTrackGripFull = false;
  private bool _showExtendedWeather = false;

  private uint _mapFrequencyValue = 1,_mapFrequencyMs = 1000;

  public VMReplay CurrentReplay
  {
    get => _replay;
    set
    {
      SetProperty( ref _replay,value ); 

      ServerOptions.ServerFrequency.Value = (int)(1000 / _replay.ReplayFrequency);
    }
  }
  public VMTrackInfo CurrentTrack
  {
    get => _track;
    set => SetProperty( ref _track,value );
  }

  public uint CurrentFrame
  {
    get => _serverManager?.CurrentFrame ?? 0;
  }

  public string ServerName
  {
    get => _serverName;
    set 
    {
      if( SetProperty( ref _serverName, value ) )
        OnPropertyChanged( nameof( ServerTitle ) );
    }
  }
  public string ServerAddress
  {
    get => _settings.ServerAddress;
    set
    {
      _settings.ServerAddress = value;

      OnPropertyChanged( nameof( ServerLink ) );
      OnPropertyChanged( nameof( ServerTitle ) );
    }
  }
  public string ServerTitle
  {
    get => $"{_serverName} - {ServerAddress}:{_settings.HttpPort}";
  }
  public string ServerLink
  {
    get => $"acmanager://race/online/join?query=race/online/join&ip={ServerAddress}&httpPort={HttpPort}";
  }
  public string TimeOfDayText
  {
    get
    {
      int m = (int)(_settings.TimeOfDay * (24*60));
      string t = $"{m/60:00}:{m%60:00}";

      return t;
    }
  }

  public CMServerOptions ServerOptions { get; }

  public int HttpPort
  {
    get => _settings.HttpPort;
    set 
    {
      _settings.HttpPort = value;

      OnPropertyChanged( nameof( HttpPort ) );
      OnPropertyChanged( nameof( ServerLink ) );
      OnPropertyChanged( nameof( ServerTitle ) );
    }
  }
  public int TcpPort
  {
    get => _settings.TcpPort;
    set
    {
      _settings.TcpPort = value;

      OnPropertyChanged( nameof( TcpPort ) );
    }
  }
  public int UdpPort
  {
    get => _settings.UdpPort;
    set
    {
      _settings.UdpPort = value;

      OnPropertyChanged( nameof( UdpPort ) );
    }
  }

  public ServerType ServerType
  {
    get => _serverType;
    set 
    {
      SetProperty( ref _serverType,value );

      OnPropertyChanged( nameof( ServerTypeIndex ) );
      OnPropertyChanged( nameof( ShowServerFrequency ) );
    }
  }
  public int ServerTypeIndex
  {
    get => (int)_serverType;
    set
    {
      SetProperty( ref _serverType,(ServerType)value ); 

      OnPropertyChanged( nameof( ServerType ) );
      OnPropertyChanged( nameof( ShowServerFrequency ) );
    }
  }

  /*
  public int ServerFrequency
  {
    get => _settings.ServerFrequency;
    set
    {
      _settings.ServerFrequency = value;
      //if( _mapFrequency == 1 )
        //_serverManager?.SetMapFrequency( _settings.MapFrequency = _settings.ServerFrequency );

      OnPropertyChanged( nameof( ServerFrequency ) );
      if( _mapFrequencyValue == 1 )
      {
        OnPropertyChanged( nameof( MapFrequencyText ) );
        OnPropertyChanged( nameof( MapFrequencyValue ) );
      }
    }
  }
  */

  public bool ShowServerFrequency
  {
    get => _serverType == ServerType.TypeA;
  }
  public bool ShowExtendedWeather
  {
    get => _settings.EnableRain;
    set 
    {
      SetProperty( ref _settings.EnableRain,value );
    }
  }

  public uint MapFrequency
  {
    get => _mapFrequencyMs;
    set => SetProperty( ref _mapFrequencyMs,value );
  }

  /*
  public uint MapFrequencyValue
  {
    get => _mapFrequencyValue;
    set
    {
      _mapFrequencyValue = value;

      if( _mapFrequencyValue == 19 )
        MapFrequency = (uint)(1000 / _settings.ServerFrequency);
      else if( _mapFrequencyValue > 0 )
        MapFrequency = 1000 / _mapFrequencyValue;
      else
        MapFrequency = 0;

      OnPropertyChanged( nameof( MapFrequencyValue ) );
    }
  }
  */
  public bool BatchedUpdates
  {
    get => _settings.BatchedUpdates;
    set
    {
      _settings.BatchedUpdates = value;

      OnPropertyChanged( nameof( BatchedUpdates ) );
    }
  }

  public float TrackGrip
  {
    get => _settings.TrackGrip * 100f;
    set 
    {
      _settings.TrackGrip = value / 100f;

      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.TrackGrip = value;

      OnPropertyChanged( nameof( TrackGrip ) );
    }
  }
  public float FuelRate
  {
    get => _settings.FuelRate;
    set
    {
      _settings.FuelRate = value;

      OnPropertyChanged( nameof( FuelRate ) );
    }
  }
  public float TiresWear
  {
    get => _settings.TiresWear;
    set
    {
      _settings.TiresWear = value;

      OnPropertyChanged( nameof( TiresWear ) );
    }
  }
  public float TimeOfDay
  {
    get => _settings.TimeOfDay;
    set
    {
      _settings.TimeOfDay = value;

      _serverManager?.ResendWeather( );

      OnPropertyChanged( nameof( TimeOfDay ) );
      OnPropertyChanged( nameof( TimeOfDayText ) );
    }
  }

  public float AmbientTemperature
  {
    get => _settings.Weather.WeatherData.TemperatureAmbient;
    set 
    {
      _settings.Weather.WeatherData.TemperatureAmbient = value;

      _serverManager?.ResendWeather( );

      OnPropertyChanged( nameof( AmbientTemperature ) );
    }
  }
  public float RoadTemperature
  {
    get => _settings.Weather.WeatherData.TemperatureRoad;
    set
    {
      _settings.Weather.WeatherData.TemperatureRoad = value;

      TemperatureRoad = value;

      _serverManager?.ResendWeather( );

      OnPropertyChanged( nameof( RoadTemperature ) );
    }
  }

  public int WindSpeed
  {
    get => _settings.Weather.WindSpeed;
    set
    {
      _settings.Weather.WindSpeed = (short)value;

      _serverManager?.ResendWeather( );

      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.WindSpeed = value;

      OnPropertyChanged( nameof( WindSpeed ) );
    }
  }
  public int WindDirection
  {
    get => _settings.Weather.WindDirection;
    set
    {
      _settings.Weather.WindDirection = (short)value;

      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.WindDirection = value;

      _serverManager?.ResendWeather( );

      OnPropertyChanged( nameof( WindDirection ) );
    }
  }

  #region Weather FX
  public WeatherFxType WeatherType
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.Type;
      return WeatherFxType.None;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.Type = value;

      _serverManager?.ResendWeather( );

      OnPropertyChanged( nameof( WeatherType ) );
    }
  }
  public WeatherFxType WeatherUpcomingType
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.UpcomingType;
      return WeatherFxType.None;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.UpcomingType = value;
      OnPropertyChanged( nameof( WeatherUpcomingType ) );
    }
  }
  public ushort TransitionValue
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.TransitionValue;
      return 0;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.TransitionValue = value;
      OnPropertyChanged( nameof( TransitionValue ) );
    }
  }
  public double TransitionValueInternal
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.TransitionValueInternal;
      return 0;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.TransitionValueInternal = value;
      OnPropertyChanged( nameof( TransitionValueInternal ) );
    }
  }
  public double TransitionDuration
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.TransitionDuration;
      return 0;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.TransitionDuration = value;
      OnPropertyChanged( nameof( TransitionDuration ) );
    }
  }
  public float TemperatureAmbient
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.TemperatureAmbient;
      return 0;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.TemperatureAmbient = value;

      _serverManager?.ResendWeather( );

      OnPropertyChanged( nameof( TemperatureAmbient ) );
    }
  }
  public float TemperatureRoad
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.TemperatureRoad;
      return 0;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.TemperatureRoad = value;

      _serverManager?.ResendWeather( );

      OnPropertyChanged( nameof( TemperatureRoad ) );
    }
  }
  public int Pressure
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.Pressure;
      return 0;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.Pressure = value;
      OnPropertyChanged( nameof( Pressure ) );
    }
  }
  public float Humidity
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.Humidity;
      return 0;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.Humidity = value;
      OnPropertyChanged( nameof( Humidity ) );
    }
  }
  public float RainIntensity
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.RainIntensity;
      return 0;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.RainIntensity = value;
      _serverManager?.ResendWeather( );

      OnPropertyChanged( nameof( RainIntensity ) );
    }
  }
  public float RainWetness
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.RainWetness;
      return 0;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.RainWetness = value;
      _serverManager?.ResendWeather( );

      OnPropertyChanged( nameof( RainWetness ) );
    }
  }
  public float RainWater
  {
    get
    {
      if( _settings.Weather.WeatherData != null )
        return _settings.Weather.WeatherData.RainWater;
      return 0;
    }
    set
    {
      if( _settings.Weather.WeatherData != null )
        _settings.Weather.WeatherData.RainWater = value;
      _serverManager?.ResendWeather( );

      OnPropertyChanged( nameof( RainWater ) );
    }
  }
  #endregion

  public bool TiresBlankets
  {
    get => _settings.TiresBlanket;
    set
    {
      _settings.TiresBlanket = value;

      OnPropertyChanged( nameof( TiresBlankets ) );
    }
  }
  public bool ExtendedCarPhysic
  {
    get => _settings.ExtendedCarPhysic;
    set
    {
      _settings.ExtendedCarPhysic = value;

      OnPropertyChanged( nameof( ExtendedCarPhysic ) );
    }
  }
  public bool ExtendedTrackPhysic
  {
    get => _settings.ExtendedTrackPhysic;
    set
    {
      _settings.ExtendedTrackPhysic = value;

      OnPropertyChanged( nameof( ExtendedTrackPhysic ) );
    }
  }
  public bool RecalcVelocities
  {
    get => _settings.RecalcVelocities;
    set
    {
      _settings.RecalcVelocities = value;

      OnPropertyChanged( nameof( RecalcVelocities ) );
    }
  }
  public bool DisableWrongWay
  {
    get => _settings.AllowWrongWay;
    set
    {
      _settings.AllowWrongWay = value;

      OnPropertyChanged( nameof( DisableWrongWay ) );
    }
  }
  public bool DisableCollisions
  {
    get => _settings.DisableCollisions;
    set
    {
      _settings.DisableCollisions = value;

      OnPropertyChanged( nameof( DisableCollisions ) );
    }
  }
  public bool ShowTrackGripFull
  {
    get => _showTrackGripFull;
    set => SetProperty( ref _showTrackGripFull, value );
  }

  public bool? HeadlightsOnOff
  {
    get => _settings.HeadlightsOnOff;
    set
    {
      _settings.HeadlightsOnOff = value;

      OnPropertyChanged( nameof( HeadlightsOnOff ) );
    }
  }

  /*
  public CarSelection.CarSelection CarSelectionPage
  {
    get;
    set;
  }
  */

  public ACServerSettings Settings
  {
    get => _settings;
  }

  public bool IsRunning
  {
    get => _serverManager?.IsRunning??false;
  }
  public bool IsPlaying
  {
    get => _serverManager?.IsPlaying ?? false;
  }

  public VMCarInfoList AdditionalCars
  {
    get => _additionalCars; 
  }

  public VMServer( VMPlayerList players,VMTrackObjects trackObjects,VMCarInfoList additionalCars,VMServerDebug debug )
  {
    _players = players;
    _trackObjects = trackObjects;
    _additionalCars = additionalCars;
    _serverDebug = debug;

    ServerOptions = new( _settings );
  }

  public bool CreateManager( )
  {
    _serverManager ??= new ServerManager( _settings,_players,_trackObjects,_serverDebug );
    if( _serverManager != null )
      _serverManager.PropertyChanged += ServerManager_PropertyChanged;

    return _serverManager != null;
  }

  public bool StartServer( uint startingFrame,Serilog.ILogger? logger )
  {
    //return _serverManager?.StartServer( _additionalCars.SelectedItems,_replay.ReplayFrequency,startingFrame,logger ) ?? false;
    return false;
  }
  public void StopServer( )
  {
    _serverManager?.StopServer( );
    _serverManager = null;
  }

  public void Play( uint loopStart,uint loopEnd )
  {
    _serverManager?.Play( loopStart,loopEnd );
  }
  public void Stop( )
  {
    _serverManager?.Stop( );
  }

  public void SetLoopFrames( uint loopStart,uint loopEnd )
  {
    _serverManager?.SetLoopFrames( loopStart,loopEnd );
  }

  public void SetStartingFrame( uint frame ) 
  {
    _serverManager?.SetStartingFrame( frame );
  }

  private void ServerManager_PropertyChanged( object? sender,System.ComponentModel.PropertyChangedEventArgs e )
  {
    if( e.PropertyName != null && e.PropertyName.Equals( nameof( ServerManager.IsRunning ) ) )
      OnPropertyChanged( nameof( IsRunning ) );
  }
}
