using System;
using System.Net;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using ACConnection.Model;
using ACConnection.Utils;
using ACConnection.Network.Tcp;
using ACConnection.Network.Packets;
using ACConnection.Network.Packets.Protocol;
using ACConnection.Network.Packets.Handshake;
using ACConnection.Network.Udp;
using ACConnection.Network.Http.Responses;

using VirtualSteward.ACNetwork.Shared;
using VirtualSteward.ACNetwork.Weather;
using VirtualSteward.Datasources.ViewModels;
using VirtualSteward.Features.CarSelection.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.Server.ViewModels;

namespace VirtualSteward.ACNetwork;

public class ACServer
{
  private TcpClient? _tcpClient;

  private VSHttpServer? httpServer;
  private TcpListener? tcpListener;

  private VSTcpServer? tcpServer;
  private VSUdpServer? udpServer;

  private byte _sessionId;

  private string _trackID = "";
  private VMPlayerList _replayCars;
  private ObservableCollection<VMCarInfo>? _additionalCars;

  private bool _loopsRunning,_carConnected;

  private Stopwatch _timeSource ;
  private DefaultWeatherTypeProvider _weatherTypeProvider = new DefaultWeatherTypeProvider( );

  private CancellationToken listenerStoppingToken;
  private CancellationTokenSource? listenerCancellationTokenSource;

  private CancellationToken tcpStoppingToken;
  private CancellationTokenSource? tcpCancellationTokenSource;

  private CancellationToken udpStoppingToken;
  private CancellationTokenSource? udpCancellationTokenSource;

  private readonly List<VMPlayer> _connectedPlayers = [];

  private readonly VMServerDebug? _serverDebug = null;

  public delegate void PacketReceived( INetworkPacket packet );
  public event PacketReceived PacketReceivedHandler;

  public string ServerName { get; set; } = "";
  public string ServerAddress { get; set; } = "";

  public int HttpPort { get; set; }
  public int TcpPort { get; set; }
  public int UdpPort { get; set; }

  public List<ACServerCar> Cars { get; set; } = [];
  public SortedList<byte,ACServerSlot> Slots { get; set; } = [];

  public bool IsClientConnected
  {
    get => _carConnected;
  }

  public ACServer( Stopwatch timeSource,PacketReceived handler,VMServerDebug? debug )
  {
    _timeSource = timeSource;
    _serverDebug = debug;

    PacketReceivedHandler = handler;
  }

  public void StartServer( string trackID,VMPlayerList replayCars,ObservableCollection<VMCarInfo>? additionalCars,Serilog.ILogger? logger = null )
  {
    _trackID = trackID;
    _replayCars = replayCars;
    _additionalCars = additionalCars;
    if( _additionalCars != null )
      _additionalCars.CollectionChanged += VisiblePlayers_ListChanged;

    replayCars.VisibleItems.CollectionChanged += VisiblePlayers_ListChanged;

    httpServer = new VSHttpServer( ServerAddress,HttpPort );
    httpServer.SetInfoResponse( GetInfoResponse( trackID,replayCars,additionalCars ) );
    httpServer.SetEntryListResponse( GetEntryListResponse( replayCars,additionalCars ) );

    httpServer.Start( logger );

    udpServer = new VSUdpServer( (ushort)UdpPort,_timeSource );

    udpCancellationTokenSource = new CancellationTokenSource( );
    udpStoppingToken = udpCancellationTokenSource.Token;

    _ = Task.Run( CheckUDPPacketsLoop );
    _ = udpServer.StartAsync( udpStoppingToken );

    listenerCancellationTokenSource = new CancellationTokenSource( );
    listenerStoppingToken = listenerCancellationTokenSource.Token;

    tcpListener = new TcpListener( IPAddress.Parse( ServerAddress ),TcpPort );

    Task.Run( Listen,listenerStoppingToken );
  }

  private void VisiblePlayers_ListChanged( object? sender,NotifyCollectionChangedEventArgs e )
  {
    if( httpServer != null && e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove )
    {
      httpServer.SetInfoResponse( GetInfoResponse( _trackID,_replayCars,_additionalCars ) );
      httpServer.SetEntryListResponse( GetEntryListResponse( _replayCars,_additionalCars ) );
    }
  }

  public Task StopServer( )
  {
    try
    {
      udpCancellationTokenSource?.Cancel( );
      tcpCancellationTokenSource?.Cancel( );
      listenerCancellationTokenSource?.Cancel( );

      httpServer?.Stop( );
      tcpListener?.Stop( );
      tcpServer?.StopAsync( ).Wait( );
    }
    catch( Exception )
    {
    }
    _loopsRunning = false;

    return Task.CompletedTask;
  }

  public void Close( )
  {
    _tcpClient?.Close( );
  }

  public Task Listen( )
  {
    if( tcpListener != null )
    {
      try
      {
        // Start listening for client requests.
        tcpListener.Start( );

        // Enter the listening loop.
        while( !listenerStoppingToken.IsCancellationRequested )
        {
          // Perform a blocking call to accept requests.
          // You could also use server.AcceptSocket() here.
          TcpClient newClient = tcpListener.AcceptTcpClient( );

          if( _tcpClient != null )
          {
            tcpCancellationTokenSource?.Cancel( );
            tcpCancellationTokenSource = null;

            _tcpClient?.Close( );
            _tcpClient = null;

            if( tcpServer != null )
            {
              tcpServer.StopAsync( );
              tcpServer = null;
            }
            _loopsRunning = false;
          }
          if( _tcpClient == null )
          {
            _tcpClient = newClient;

            tcpServer = new VSTcpServer( _tcpClient );

            StartLoops( );
          }
        }
      }
      catch( SocketException e )
      {
        Console.WriteLine( "SocketException: {0}",e );
      }
      finally
      {
        tcpListener.Stop( );
      }
    }
    return Task.CompletedTask;
  }

  public void SendHandshake( HandshakeRequest handshake,IList<VMPlayer> replayCars,IList<VMCarInfo>? additionalCars,ACServerSettings settings )
  {
    if( tcpServer != null && udpServer != null )
    {
      string selectedSkin = SelectSkin( replayCars,additionalCars,handshake.RequestedCar );

      _sessionId = (byte)SelectSessionId( replayCars );

      VMPlayer newPlayer = new ( _sessionId,handshake.Name,handshake.Team,handshake.Nation,handshake.RequestedCar,selectedSkin );

      _connectedPlayers.Add( newPlayer );

      string trackID = settings.TrackID;
      if( settings.ExtendedCarPhysic || settings.ExtendedTrackPhysic )
      {
        CSPTrackOptions cspTrackOptions = new ( )
        {
          Track = settings.TrackID,
          Flags = (settings.ExtendedCarPhysic?TrackOptionsFlags.CustomCarPhysics:0)|(settings.ExtendedTrackPhysic?TrackOptionsFlags.CustomTrackPhysics:0),
          MinimumCSPVersion = 1937
        };
        trackID = cspTrackOptions.ToString( );
      }
      List<Session> sessions = GetSessions( );

      HandshakeResponse response = new ( )
      {
        ABSAllowed = 1,
        TractionControlAllowed = 1,
        AllowedTyresOutCount = 2,
        AllowTyreBlankets = settings.TiresBlanket,
        AutoClutchAllowed = true,
        CarModel = newPlayer.PlayerInfo.CarInfo.CarID,
        CarSkin = newPlayer.PlayerInfo.CarSkinInfo.SkinID,
        FuelConsumptionRate = settings.FuelRate,
        HasExtraLap = false,
        InvertedGridPositions = 0,
        IsGasPenaltyDisabled = true,
        IsVirtualMirrorForced = false,
        JumpStartPenaltyMode = 0,
        MechanicalDamageRate = 0,
        PitWindowEnd = 0,
        PitWindowStart = 0,
        StabilityAllowed = true,
        RaceOverTime = 0,
        RefreshRateHz = (byte)settings.ServerFrequency,
        ResultScreenTime = 60,
        ServerName = "Virtual Steward",
        SessionId = _sessionId,
        SunAngle = 20,
        TrackName = trackID,
        TrackConfig = settings.VariantID,
        TyreConsumptionRate = settings.TiresWear,
        UdpPort = (ushort)UdpPort,
        CurrentSession = sessions[0],
        SessionTime = 60*1000,//_timeSource.ElapsedMilliseconds + vsUDPServer.nServerOffset,
        ChecksumCount = 0,
        ChecksumPaths = [],
        CurrentTime = 0, // Ignored by AC
        LegalTyres = "",
        RandomSeed = 532456236,
        SessionCount = (byte)sessions.Count,
        Sessions = sessions,
        SpawnPosition = _sessionId,
        TrackGrip = settings.TrackGrip,
        MaxContactsPerKm = 20,
      };
      SendTCPPacket( response );
    }
    else
    {
      throw new Exception( "TCP or UDP server is null" );
    }
  }
  public void SendCarListResponse( IList<VMPlayer> replayCars,int pageIndex )
  {
    if( tcpServer != null )
    {
      List<VMPlayer> allCars = [.. replayCars, .._connectedPlayers ];

      allCars.Sort( );

      List<VMPlayer> pageCars = allCars.Skip( pageIndex ).Take( 10 ).ToList( );

      List<EntryCar> arCars = new ( 10 );
      foreach( VMPlayer car in pageCars )
      {
        arCars.Add( new EntryCar( )
        {
          SessionId = (byte)car.PlayerID,
          Model = car.PlayerInfo.CarInfo.CarID,
          Skin = (car.PlayerInfo.CarSkinInfo.SkinID + "/ACA3").Replace( "/ACA3/ACA3","/ACA3" ),
          Name = car.PlayerInfo.PlayerName,
          Team = car.PlayerInfo.PlayerTeam,
          NationCode = car.PlayerInfo.PlayerNation,
          IsSpectator = false,
          Damage = new DamageZoneLevel( ),
        }
        );
      }
      CarListResponse response = new ()
      {
        PageIndex = pageIndex,
        EntryCarsCount = arCars.Count,
        EntryCars = arCars,
      };
      SendTCPPacket( response );
    }
  }
  public void SendCarConnect( )
  {
    if( udpServer != null && !_carConnected )
    {
      udpServer.SendProtocol( udpServer.Address,ACServerProtocol.CarConnect );
    }
  }
  public void SendSessionUpdate( )
  {
    if( udpServer != null )
    {
      Session s = new ()
      {
        Id = 1,
        Type =  SessionType.Practice,
        Name = "Practice",
        Time = 12*60*60,
      };
      List<byte> grid = new ( );
      foreach( var player in _replayCars.VisibleItems )
        grid.Add( (byte)player.PlayerID );

      SendUDPPacket( new CurrentSessionUpdate( )
      {
        CurrentSession = s,
        Grid = grid,
        TrackGrip = 1,
        StartTime = _timeSource.ElapsedMilliseconds - udpServer.nClientOffset,
      } );
    }
  }
  public void SendWelcomeMessage( ACServerSettings settings )
  {
    CSPServerExtraOptions serverExtraOptions = new ( )
    {
      WelcomeMessage = settings.WelcomeMessage,
      CSPExtraOptions = LoadCspExtraOptions( settings.CSPSettingsFile,settings )
    };
    SendTCPPacket( new WelcomeMessage { Message = serverExtraOptions.GenerateWelcomeMessage( ) } );
  }
  public void SendSunAngle( float sunAngle )
  {
    if( tcpServer != null )
    {
      SunAngleUpdate sa = new( )
      {
        SunAngle = sunAngle,
      };
      SendTCPPacket( sa );
    }
  }
  public void SendWeather( WeatherData weather,DateTimeOffset dateTime )
  {
    var wfxParams = new WeatherFxParams
    {
      Type = weather.Type,
      StartDate = dateTime.ToUnixTimeSeconds( )
    };

    var weatherType = _weatherTypeProvider.GetWeatherType(wfxParams.Type) with
    {
      Graphics = wfxParams.ToString(),
    };

    var weatherUpdate = new WeatherUpdate
    {
      Graphics = weatherType.Graphics,
      Ambient = (byte) weather.TemperatureAmbient,
      Road = (byte) weather.TemperatureRoad,
      WindDirection = (short) weather.WindDirection,
      WindSpeed = (short) weather.WindSpeed,
    };
    
    SendTCPPacket( weatherUpdate );
  }
  public void SendWeatherFx( WeatherData weather,DateTimeOffset dateTime )
  {
    var newWeather = new CSPWeatherUpdate
    {
      //UnixTimestamp = (ulong) dateTime.Date.AtStartOfDayInZone(DateTimeZone.Utc).ToInstant().ToUnixTimeSeconds(),
      UnixTimestamp = (ulong) dateTime.ToUnixTimeSeconds( ),
      WeatherType = (byte) weather.Type,
      UpcomingWeatherType = (byte) weather.UpcomingType,
      TransitionValue = weather.TransitionValue,
      TemperatureAmbient = (Half) weather.TemperatureAmbient,
      TemperatureRoad = (Half) weather.TemperatureRoad,
      TrackGrip = (Half) weather.TrackGrip,
      WindDirectionDeg = (Half) weather.WindDirection,
      WindSpeed = (Half) weather.WindSpeed,
      Humidity = (Half) weather.Humidity,
      Pressure = (Half) weather.Pressure,
      RainIntensity = (Half) weather.RainIntensity,
      RainWetness = (Half) weather.RainWetness,
      RainWater = (Half) weather.RainWater
    };
    SendUDPPacket( newWeather );
  }
  public void SendChat( string message )
  {
    if( tcpServer != null )
    {
      ChatMessage chatMessage = new( )
      {
        SessionId = _sessionId,
        Message = message
      };
      SendTCPPacket( chatMessage );
    }
  }

  public void SendPositionUpdateServer( PositionUpdateOut pu )
  {
    if( udpServer != null && _carConnected )
    {
      SendUDPPacket( pu );
    }
  }

  public void SendPing( )
  {
    if( udpServer != null && _carConnected )
    {
      PingUpdate pu = new ()
      {
        Time = (uint)_timeSource.ElapsedMilliseconds,
        CurrentPing = (ushort)udpServer.nPing,
      };
      SendUDPPacket( pu );
    }
  }

  public PositionUpdateOut GetPositionUpdateServer( int sessionId,byte pakSequenceId,uint timeStamp,VMServerData serverData )
  {
    int  nVelocityMultiplier = 1;
    int nSteerAngle = (int)(serverData.SteeringWheel / 270.0F * 254);

    PositionUpdateOut pu = new( )
    {
      SessionId = (byte)sessionId,
      PakSequenceId = pakSequenceId,

      Timestamp = (uint)(timeStamp - udpServer?.nClientOffset ?? 0),
      Ping = (ushort)(udpServer?.nPing ?? 0),

      Position = serverData.Position,
      Rotation = serverData.Rotation,
      Velocity = serverData.Velocity,

      Gas = serverData.GasPedal,
      Gear = serverData.Gear,
      EngineRpm = serverData.RPMs,

      SteerAngle = (byte)(127 + Math.Clamp( nSteerAngle,-127,127 ) ),
      WheelAngle = (byte)(127 - serverData.WheelsAngle / 130.0F * 254),

      StatusFlag = (CarStatusFlags)serverData.Flags,
      PerformanceDelta = 0,

      TyreAngularSpeedFL = (byte)(Math.Clamp( MathF.Round( MathF.Log10( serverData.FLAngular * nVelocityMultiplier + 1.0f ) * 20.0f ) * Math.Sign( serverData.FLAngular ),-100.0f,154.0f ) + 100.0f),
      TyreAngularSpeedFR = (byte)(Math.Clamp( MathF.Round( MathF.Log10( serverData.FRAngular * nVelocityMultiplier + 1.0f ) * 20.0f ) * Math.Sign( serverData.FRAngular ),-100.0f,154.0f ) + 100.0f),
      TyreAngularSpeedRL = (byte)(Math.Clamp( MathF.Round( MathF.Log10( serverData.RLAngular * nVelocityMultiplier + 1.0f ) * 20.0f ) * Math.Sign( serverData.RLAngular ),-100.0f,154.0f ) + 100.0f),
      TyreAngularSpeedRR = (byte)(Math.Clamp( MathF.Round( MathF.Log10( serverData.RRAngular * nVelocityMultiplier + 1.0f ) * 20.0f ) * Math.Sign( serverData.RRAngular ),-100.0f,154.0f ) + 100.0f),
    };
    return pu;
  }

  private Task StartLoops( )
  {
    if( tcpServer != null && !_loopsRunning )
    {
      tcpCancellationTokenSource = new CancellationTokenSource( );
      tcpStoppingToken = tcpCancellationTokenSource.Token;

      _ = Task.Run( CheckTCPPacketsLoop );
      _ = tcpServer.StartAsync( );

      _loopsRunning = true;
    }
    return Task.CompletedTask;
  }

  private string GetInfoResponse( string trackID,IList<VMPlayer> replayCars,IList<VMCarInfo>? additionalCars )
  {
    SortedList<string,string> arCars = [];
    foreach( VMPlayer player in replayCars )
    {
      if( !arCars.ContainsKey( player.PlayerInfo.CarInfo.CarID ) )
        arCars.Add( player.PlayerInfo.CarInfo.CarID,player.PlayerInfo.CarInfo.CarID );
    }
    if( additionalCars != null )
    {
      foreach( VMCarInfo info in additionalCars )
      {
        if( !arCars.ContainsKey( info.CarID ) )
          arCars.Add( info.CarID,info.CarID );
      }
    }
    InfoResponse ir = new ( )
    {
      Cars = arCars.Values,
      Clients = replayCars.Count,
      Country = ["Italy","IT"],
      CPort = HttpPort,
      Durations = [12 * 600],
      Extra = false,
      Inverted = 0,
      Ip = ServerAddress,
      MaxClients = replayCars.Count+1,
      Name = "Virtual Steward"+$" i{HttpPort}",
      Pass = false,
      Pickup = true,
      Pit = false,
      Session = 1,
      Port = (ushort)UdpPort,
      SessionTypes = [1],
      Timed = false,
      TimeLeft = 12 * 60 * 60 - (int)(_timeSource.ElapsedMilliseconds/1000),
      TimeOfDay = 16,
      Timestamp = 0,
      TPort = (ushort)TcpPort,
      Track = trackID,
      PoweredBy = "Virtual Steward"
    };
    return JsonSerializer.Serialize( ir );
  }
  private string GetEntryListResponse( IList<VMPlayer> replayCars,IList<VMCarInfo>? additionalCars )
  {
    SortedList<string,string> arCarsType = [];
    List<EntryListResponseCar> arCars = [];

    foreach( VMPlayer player in replayCars )
    {
      EntryListResponseCar entry = new ( )
      {
        Model = player.PlayerInfo.CarInfo.CarID,
        Skin = player.PlayerInfo.CarSkinInfo.SkinID + "/ACA3",
        IsEntryList = true,
        DriverName = player.PlayerInfo.PlayerName != "" ? player.PlayerInfo.PlayerName : null,
        DriverTeam = player.PlayerInfo.PlayerTeam != "" ? player.PlayerInfo.PlayerTeam : null,
        IsConnected = player.IsVisible
      };
      arCars.Add( entry );

      if( !arCarsType.ContainsKey( player.PlayerInfo.CarInfo.CarID ) )
      {
        entry = new( )
        {
          Model = player.PlayerInfo.CarInfo.CarID,
          Skin = player.PlayerInfo.CarSkinInfo.SkinID + "/ACA3",
          IsEntryList = true,
          DriverName = null,
          DriverTeam = null,
          IsConnected = false,
        };
        arCars.Add( entry );

        arCarsType.Add( player.PlayerInfo.CarInfo.CarID,player.PlayerInfo.CarInfo.CarID );
      }
    }
    if( additionalCars != null )
    {
      foreach( VMCarInfo info in additionalCars )
      {
        if( !arCarsType.ContainsKey( info.CarID ) )
        {
          EntryListResponseCar entry = new( )
          {
            Model = info.CarID,
            Skin = info.SelectedSkinID + "/ACA3",
            IsEntryList = true,
            DriverName = null,
            DriverTeam = null,
            IsConnected = false,
          };
          arCars.Add( entry );

          arCarsType.Add( info.CarID,info.CarID );
        }
      }
    }
    EntryListResponse el = new ( )
    {
      Cars = arCars,
      Features = ["WEATHERFX_V1","SPECTATING_AWARE","LOWER_CLIENTS_SENDING_RATE","EMOJI","CLIENT_MESSAGES","CUSTOM_UPDATE"],
    };
    return JsonSerializer.Serialize( el );
  }

  private static List<Session> GetSessions( )
  {
    List<Session> sessions =
      [
        new Session( )
        {
          Id = 1,
          Type = SessionType.Practice,
          Name = "Practice",
          Time = 12 * 60,
          Laps = 0,
        },
      ];
    return sessions;
  }
  private static int SelectSessionId( IList<VMPlayer> replayCars )
  {
    int id = 0;
    foreach( VMPlayer player in replayCars )
    {
      if( player.PlayerID > id++ )
        return id;
    }
    return replayCars.Count;
  }
  private static string SelectSkin( IList<VMPlayer> replayCars,IList<VMCarInfo>? additionalCars,string requestedCar )
  {
    if( additionalCars != null )
    {
      foreach( VMCarInfo info in additionalCars )
      {
        if( info.CarID.Equals( requestedCar ) )
          return info.SelectedSkinID;
      }
    }
    foreach( VMPlayer player in replayCars )
    {
      if( player.PlayerInfo.CarInfo.CarID.Equals( requestedCar ) )
        return player.PlayerInfo.CarSkinInfo.SkinID;
    }
    return string.Empty;
  }
  private static string? LoadCspExtraOptions( string? path,ACServerSettings settings )
  {
    string cspSettings = string.Empty;
    if( settings.AllowWrongWay )
      cspSettings += "[EXTRA_RULES]\r\nALLOW_WRONG_WAY = 1\r\nLIMIT_LOCK_CONTROLS_TIME = 0\r\nLIMIT_LOCK_CONTROLS_TOTAL_TIME = 0\r\n\r\n";
    if( settings.DisableCollisions )
      cspSettings += "[CUSTOM_PHYSICS]\r\nREAL_MASS = 0.000001\r\n[CUSTOM_COLLISIONS]\r\nSOFT_ERP = 0.999999\r\nSOFT_CFM = 1\r\nBOUNCE = 0.0\r\nFRICTION = 0.0\r\nINTENSITY = 0\r\nMAX_DEPTH = 0.0\r\n\r\n";
    if( settings.EnableRain )
      cspSettings += $"[RAIN_PREVIEW]\r\nINTENSITY = {settings.Weather.WeatherData.RainIntensity.ToString( ).Replace(",",".")}\r\nREQUIRED = 0\r\nWITH_PHYSICS = 1\r\n\r\n";

    if( path != null )
      return System.IO.File.Exists( path ) ? cspSettings + System.IO.File.ReadAllText( path ) : cspSettings;
    return cspSettings;
  }

  #region TCP and UDP packets
  private void SendTCPPacket( INetworkPacket packet )
  {
    tcpServer?.SendPacket( packet );

    _serverDebug?.AddOutgoingPacket( packet );
  }
  private void SendUDPPacket( INetworkPacket packet )
  {
    udpServer?.SendPacket( udpServer.Address,packet );

    _serverDebug?.AddOutgoingPacket( packet );
  }

  private void TCPPacketReceived( INetworkPacket packet )
  {
    if( tcpServer is not null )
    {
      if( packet.GetID( ) == ACServerProtocol.CleanExitDrive )
      {
        _carConnected = false;

        tcpCancellationTokenSource?.Cancel( );
        tcpCancellationTokenSource = null;

        tcpServer.StopAsync( );
        tcpServer = null;

        _tcpClient?.Close( );
        _tcpClient = null;

        _loopsRunning = false;
      }
    }
  }
  private void UDPPacketReceived( INetworkPacket packet )
  {
    if( udpServer is not null )
    {
      if( packet is PositionUpdateIn && !_carConnected )
      {
        _carConnected = true;
      }
    }
  }

  private async Task CheckTCPPacketsLoop( )
  {
    if( tcpServer is not null )
    {
      try
      {
        while( !tcpStoppingToken.IsCancellationRequested )
        {
          await foreach( var packet in tcpServer.IncomingPackets.Reader.ReadAllAsync( ) )
          {
            TCPPacketReceived( packet );

            PacketReceivedHandler?.Invoke( packet );
          }
        }
      }
      catch( ChannelClosedException ) { }
      catch( ObjectDisposedException ) { }
      catch( OperationCanceledException ) { }
#if !DEBUG
      catch( Exception )
      {
        tcpCancellationTokenSource = null;

        tcpServer?.StopAsync( );
        tcpServer = null;

        _tcpClient?.Close( );
        _tcpClient = null;

        _loopsRunning = false;
      }
#endif
    }
  }
  private async Task CheckUDPPacketsLoop( )
  {
    if( udpServer is not null )
    {
      try
      {
        while( !udpStoppingToken.IsCancellationRequested )
        {
          await foreach( var packet in udpServer.IncomingPackets.Reader.ReadAllAsync( ) )
          {
            UDPPacketReceived( packet );

            PacketReceivedHandler?.Invoke( packet );
          }
        }
      }
      catch( ChannelClosedException ) { }
      catch( ObjectDisposedException ) { }
      catch( OperationCanceledException ) { }
#if !DEBUG
      catch( Exception )
      {
      }
#endif
    }
  }
  #endregion
}
