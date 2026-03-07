using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using NodaTime;
using Avalonia.Threading;

using ACConnection.Model;
using ACConnection.Network.Packets;
using ACConnection.Network.Packets.Protocol;

using Framework.Bindables;

using VirtualSteward.ACNetwork;
using VirtualSteward.ACNetwork.Shared;
using VirtualSteward.Datasources.ViewModels;
using VirtualSteward.Features.CarSelection.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Server.ViewModels;

namespace VirtualSteward.Features.Server.Classes; 

public class ServerManager : BindableBase
{
  private ACServer? _acServer = null;
  private DispatcherTimer? _pingTimer = null;

  private readonly Stopwatch _timeSource;
  private readonly ACServerSettings _settings;
  private readonly VMServerDebug? _serverDebug = null;

  private byte _pakSequenceId = 0;
  private double _replayFrequency = 30;
  private uint _timeOffset = 0,_frameOffset = 0,_lastFrame = 0,_loopStart = 0,_loopEnd = 0;

  private bool _isRunning = false,_isPlaying = false,_firstUpdateReceived = false;

  private float _lastSunAngle = 0;
  private readonly VMTrackObjectData _currentObjectData = new( );

  private ACServerWeather? _lastWeather = null;

  private VMPlayerList _replayCars;
  private VMTrackObjects _trackObjects;

  private ObservableCollection<VMCarInfo>? _additionalCars;

  private CancellationTokenSource? _serverLoopTokenSource = null;

  public bool IsRunning
  {
    get => _isRunning;
    internal set { SetProperty( ref _isRunning,value ); }
  }
  public bool IsPlaying
  {
    get => _isRunning && _isPlaying;
    internal set { SetProperty( ref _isPlaying,value ); }
  }

  public uint CurrentFrame
  {
    get => _lastFrame;
  }

  public Vector3 CurrentPlayerSpeed;
  public Vector3 CurrentPlayerPosition;
  public Vector3 CurrentPlayerRotation;

  public ServerManager( ACServerSettings settings,VMPlayerList players,VMTrackObjects trackObjects,VMServerDebug? serverDebug )
  {
    _settings = settings;
    _replayCars = players;
    _trackObjects = trackObjects;
    _timeSource = new Stopwatch( );

    _serverDebug = serverDebug;
  }

  public void Play( uint loopStart,uint loopEnd )
  {
    _frameOffset = _lastFrame;
    _timeOffset = (uint)_timeSource.Elapsed.TotalMilliseconds;

    SetLoopFrames( loopStart,loopEnd );

    IsPlaying = true;
  }
  public void Stop( )
  {
    _lastFrame += 1;

    IsPlaying = false;
  }

  public void SetLoopFrames( uint loopStart,uint loopEnd )
  {
    _loopStart = loopStart;
    _loopEnd = loopEnd;
  }

  public void StartServer( ObservableCollection<VMCarInfo>? additionalCars,double replayFrequency,uint startingFrame,Serilog.ILogger? logger = null )
  {
    _replayFrequency = replayFrequency;

    _acServer ??= new ACServer( _timeSource,Server_PacketReceived,_serverDebug )
    {
      ServerName = _settings.ServerName,
      ServerAddress = _settings.ServerAddress,
      HttpPort = _settings.HttpPort,
      TcpPort = _settings.TcpPort,
      UdpPort = _settings.UdpPort,
    };
    if( _acServer != null )
    {
      _additionalCars = additionalCars;

      if( !_timeSource.IsRunning )
        _timeSource.Start( );

      _pingTimer ??= new DispatcherTimer( );
      _pingTimer.Interval = TimeSpan.FromSeconds( 1 );
      _pingTimer.Tick -= PingUpdate_Tick;
      _pingTimer.Tick += PingUpdate_Tick;
      _pingTimer.Start( );

      SetStartingFrame( startingFrame );

      _acServer.StartServer( _settings.TrackID,_replayCars,_additionalCars,logger );

      _serverLoopTokenSource = new( );

      StartTask( _replayCars,_serverLoopTokenSource.Token );

      IsRunning = true;
    }
    else
    {
      throw new Exception( "Server manager: Cannot create ACServer instance");
    }
  }
  public void StopServer( )
  {
    _timeSource.Stop( );

    _serverLoopTokenSource?.Cancel( );
    _serverLoopTokenSource = null;

    _acServer?.StopServer( );
    _acServer = null;

    IsRunning = IsPlaying = false;
  }

  public void SetStartingFrame( uint frame )
  {
    _frameOffset = _lastFrame = frame;
    _timeOffset = (uint)_timeSource.Elapsed.TotalMilliseconds;
  }

  public void ResendWeather( )
  {
    _lastWeather = null;
  }

  private void StartTask( VMPlayerList players,CancellationToken cancellationToken )
  {
    Task.Factory.StartNew( ( ) => ServerLoopTask( players,cancellationToken ),TaskCreationOptions.LongRunning );
  }
  private void ServerLoopTask( VMPlayerList players,CancellationToken cancellationToken )
  {
    int sleepMs = 1000 / _settings.ServerFrequency;
    long nextTick = _timeSource.ElapsedMilliseconds + sleepMs;

#if !DEBUG
    try
#endif
    {
      while( !cancellationToken.IsCancellationRequested )
      {
        if( _isPlaying )
        {
          uint timeStamp = (uint)_timeSource.Elapsed.TotalMilliseconds;
          uint milliseconds = timeStamp - _timeOffset;
          uint frame = ValidateFrame( _frameOffset + (uint)(milliseconds / _replayFrequency) );
          uint frameTimeStamp = _timeOffset + (uint)((frame-_frameOffset) * _replayFrequency);

          _pakSequenceId += (byte)(frame - _lastFrame);
          _lastFrame = frame;

          float fVelMul = 1;
          //if( !_realtime.IsPlaying )
          //fVelMul = 0;

          ServerTick( players,frame,frameTimeStamp,_pakSequenceId,fVelMul );
        }
        else
        {
          uint frame = _lastFrame;
          uint timeStamp = (uint)_timeSource.Elapsed.TotalMilliseconds;
          uint frameTimeStamp = _timeOffset + (uint)((frame-_frameOffset) * _replayFrequency);

          float fVelMul = 1;
          //if( !_realtime.IsPlaying )
          //fVelMul = 0;

          ServerTick( players,frame,timeStamp,++_pakSequenceId,fVelMul );
        }
        nextTick = WaitTicks( nextTick,1000 / _settings.ServerFrequency );
      }
    }
#if !DEBUG
    catch( Exception e )
    {
      int c = 0;
    }
#endif
    long WaitTicks( long nextTick,int sleepMs )
    {
      long currentTick = _timeSource.ElapsedMilliseconds;
      if( currentTick < nextTick )
        Thread.Sleep( (int)(nextTick - currentTick) );
      return _timeSource.ElapsedMilliseconds + sleepMs;
    }
  }

  private void PacketReceived( INetworkPacket packet )
  {
    if( _acServer is not null )
    {
      _serverDebug?.AddIncomingPacket( packet );

      if( packet.GetID( ) == ACServerProtocol.Handshake )
      {
        _acServer.SendHandshake( (HandshakeRequest)packet,_replayCars,_additionalCars,_settings );
      }
      else if( packet.GetID( ) == ACServerProtocol.CarListRequest )
      {
        CarListRequest request = (CarListRequest)packet;

        _acServer.SendCarListResponse( _replayCars,request.PageIndex );

        _firstUpdateReceived = false;
      }
      else if( packet.GetID( ) == ACServerProtocol.CarConnect )
      {
        _acServer.SendCarConnect( );
      }
      else if( packet.GetID( ) == ACServerProtocol.SessionRequest )
      {
        if( ((SessionRequest)packet).SessionType != SessionType.Practice )
          _acServer.SendSessionUpdate( );
      }
      else if( packet.GetID( ) == ACServerProtocol.PositionUpdate )
      {
        PositionUpdateIn pos = (PositionUpdateIn)packet;

        CurrentPlayerSpeed = new Vector3( pos.Velocity.X,pos.Velocity.Z,pos.Velocity.Y );
        CurrentPlayerPosition = new Vector3( pos.Position.X,pos.Position.Z,pos.Position.Y );
        CurrentPlayerRotation = new Vector3( pos.Rotation.X,pos.Rotation.Z,pos.Rotation.Y );

        if( !_firstUpdateReceived )
        {
          _acServer.SendWelcomeMessage( _settings );
          
          _firstUpdateReceived = true;

          ResendWeather( );
        }
      }
      else if( packet.GetID( ) == ACServerProtocol.Chat )
      {
        ChatMessage cm = (ChatMessage)packet;

        if( cm.Message.Equals( "reset" ) )
          SetStartingFrame( _loopStart );
        else if( cm.Message.Equals( "start" ) )
          Play( _loopStart,_loopEnd );
        else if( cm.Message.Equals( "pause" ) || cm.Message.Equals( "stop" ) )
          Stop( );

        _acServer.SendChat( cm.Message );
      }
    }
    /*
    //this.Dispatcher.Invoke( new Action( ( ) =>
    {
      if( _acServer is not null )
      {
        _debug?.AddIncomingPacket( packet );

        else if( packet.GetID( ) == ACServerProtocol.Chat )
        {
          ChatMessage cm = (ChatMessage)packet;

          if( cm.Message.Equals( "reset" ) )
            Application.Current.Dispatcher.Invoke( new Action( ( ) => { _timeline.CurrentFrame = _timeline.ScrubA; } ) );
          else if( cm.Message.Equals( "start" ) )
            Application.Current.Dispatcher.Invoke( new Action( ( ) => { MediaCommands.Play.Execute( null,Application.Current.MainWindow ); } ) );
          else if( cm.Message.Equals( "pause" ) || cm.Message.Equals( "stop" ) )
            Application.Current.Dispatcher.Invoke( new Action( ( ) => { MediaCommands.Stop.Execute( null,Application.Current.MainWindow ); } ) );

          _acServer.SendChat( cm.Message );
        }
        else if( packet.GetID( ) == ACServerProtocol.PositionUpdate )
        {
          PositionUpdateIn pos = (PositionUpdateIn)packet;

          CurrentPlayerSpeed = new Vector3( pos.Velocity.X,pos.Velocity.Z,pos.Velocity.Y );
          CurrentPlayerPosition = new Vector3( pos.Position.X,pos.Position.Z,pos.Position.Y );
          CurrentPlayerRotation = new Vector3( pos.Rotation.X,pos.Rotation.Z,pos.Rotation.Y );

          if( !_firstUpdateReceived )
          {
            _acServer.SendWelcomeMessage( _settings );

            _firstUpdateReceived = true; 
          }
        }
      }
    }
    */
  }

  private void ServerTick( IList<VMPlayer> players,uint frame,uint frameTimeStamp,byte pakSequenceId,float velMul )
  {
    if( _acServer != null )
    {
      UpdatePlayers( players,frame,frameTimeStamp,pakSequenceId );
      UpdateWeather( frame );
    }
  }

  private void UpdatePlayers( IList<VMPlayer> players,uint frame,uint frameTimeStamp,byte pakSequenceId )
  {
    var prevServerData = new VMServerData( );
    foreach( VMPlayer player in players )
    {
      VMServerData? pos = player.Datasource.GetServerData( frame );
      if( player.IsVisible && pos != null )
      {
        if( frame > 0 && player.PlayerID != 0 && _settings.RecalcVelocities )
        {
          VMServerData? prevPos = player.Datasource.GetServerData( frame-1,prevServerData );
          if( prevPos != null )
          {
            pos.Velocity.X = (float)(((pos.Position.X - prevPos.Position.X) / _replayFrequency) * 1000f);
            pos.Velocity.Y = (float)(((pos.Position.Y - prevPos.Position.Y) / _replayFrequency) * 1000f);
            pos.Velocity.Z = (float)(((pos.Position.Z - prevPos.Position.Z) / _replayFrequency) * 1000f);
          }
        }
        if( PostProcessServerData( pos ) )
        {
          PositionUpdateOut pu = _acServer.GetPositionUpdateServer( player.PlayerID,pakSequenceId,frameTimeStamp,pos );

          _acServer.SendPositionUpdateServer( pu );
        }
      }
    }
  }
  private bool PostProcessServerData( VMServerData serverData )
  {
    if( _settings.HeadlightsOnOff.HasValue )
    {
      serverData.Flags = (VMServerData.StatusFlags)((uint)serverData.Flags & ~0x20);
      serverData.Flags |= _settings.HeadlightsOnOff.Value ? VMServerData.StatusFlags.LightsOn : 0;
    }
    if( !_isPlaying )
    {
      serverData.Velocity.X = 0;
      serverData.Velocity.Y = 0;
      serverData.Velocity.Z = 0;

      serverData.FLAngular = 0;
      serverData.FRAngular = 0;
      serverData.RLAngular = 0;
      serverData.RRAngular = 0;
    }
    return true;
  }

  private uint ValidateFrame( uint frame )
  {
    uint resultFrame = frame;
    if( _loopStart == _loopEnd )
    {
      if( resultFrame < 0 )
        resultFrame = 0;
      else if( resultFrame >= _loopEnd )
        resultFrame = _loopEnd;
    }
    else
    {
      if( resultFrame < _loopStart )
        resultFrame = _loopEnd;
      else if( resultFrame >= _loopEnd )
        resultFrame = _loopStart;
    }
    if( resultFrame != frame )
    {
      _frameOffset = resultFrame;
      _timeOffset = (uint)_timeSource.ElapsedMilliseconds;
    }
    return resultFrame;
  }

  private void UpdateWeather( uint frame )
  {
    if( _acServer != null )
    {
      if( _lastWeather == null )
      {
        _acServer.SendWeather( _settings.Weather.WeatherData,new( SystemClock.Instance.GetCurrentInstant( ),DateTimeZone.Utc ) );
        //_acServer.SendWeatherFx( _settings.Weather.WeatherData,new( SystemClock.Instance.GetCurrentInstant( ),DateTimeZone.Utc ) );

        _lastWeather = _settings.Weather;
      }
      // 16*((HOURS*3600)+(MINUTES*60)+SECONDS-46800)/(50400-46800)
      float sunAngle = (_settings.TimeOfDay+0.45833f) * 384.0f;
      //if( _trackObjects.GetObjectData( frame,_currentObjectData ) )
        //sunAngle = _currentObjectData.SunAngle;

      if( _lastSunAngle != sunAngle )
        _acServer.SendSunAngle( _lastSunAngle = sunAngle );
    }
  }

  private void PingUpdate_Tick( object? sender,EventArgs e )
  {
    if( _isRunning )
    {
      _acServer?.SendPing( );
    }
  }
  private void Server_PacketReceived( INetworkPacket packet )
  {
    PacketReceived( packet );
  }
}
