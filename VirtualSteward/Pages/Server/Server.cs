using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Framework.UI;
using Framework.Settings;

using VirtualSteward.ViewModels;
using VirtualSteward.ACNetwork.Shared;
using VirtualSteward.ACNetwork.Weather;
using VirtualSteward.Classes;
using VirtualSteward.Features.CarSelection;
using VirtualSteward.Features.CarSelection.ViewModels;

using VirtualSteward.Features.Server.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.Realtime.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Server.Classes;
using VirtualSteward.Features.Server.Configurations;
using VirtualSteward.Features.Server.Values;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Pages.Server.ViewModels;

namespace VirtualSteward.Pages.Server;

public partial class Server : StateFeature
{
    public string Icon { get; } = "\xf202";

    private readonly VMFrameValidationTimeline _frameValidation;

    private readonly FilesManager _fileManager;
    private readonly MessageManager _messageManager;
    
    private readonly ACServerSettings _settings = new ACServerSettings( );
    private readonly VMServerDebug _serverDebug = new VMServerDebug( );

    private ServerManager? _serverManager;

    public FeatureCommand ServerStart { get; }
    public VMFrameValidationTimeline FrameValidation => _frameValidation;
        
    public CMServerStartOptions StartOptions { get; } 
    public CMServerPorts ServerPorts { get; }
    public CMServerOptions ServerOptions { get; }
    public CMServerWeather ServerWeather { get; }
    public CarSelection CarSelection { get; }

    [ObservableProperty] private VMServerStatus _serverStatus = new VMServerStatus( );

    public Server( State state,DataTemplates templates,string title,VMTimeline timeline,VMFrameValidationTimeline frameValidation,FilesManager filesManager,MessageManager messageManager ) : base( state,templates,title )
    {
        _fileManager = filesManager;
        _messageManager = messageManager;
        _frameValidation = frameValidation;

        StartOptions = new CMServerStartOptions( this,timeline );
        ServerPorts = new CMServerPorts( _settings ) { Width = 395 } ;
        ServerOptions = new CMServerOptions( _settings ) { Width = 395 } ;
        ServerWeather = new CMServerWeather( _settings,this )
        {
            Width = 395,
            WeatherType =
            {
                ValueChanged = WeatherTypeValueChanged
            }
        };
        CarSelection = new CarSelection( templates,"",filesManager,new VMCarInfoList( true ) { LastSelectedAsActive = true } );

        ServerStart = new FeatureCommand( )
        {
            IsDefault = true,
            Text = "Start server",
            RoutedCommand = StartServerCommand
        };
    }

    public override async Task OnLoaded( Settings settings )
    {
        await CarSelection.OnLoaded( settings );
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<Server>( ( _,_ ) => new Pages.Server( ) ) );
        templates.Add( new FuncDataTemplate<WeatherTypeValue>( ( _,_ ) => new Framework.UI.Inputs.ComboboxInput( ) ) );
        templates.Add( new FuncDataTemplate<HeadlightsValue>( ( _,_ ) => new Framework.UI.Inputs.ComboboxInput( ) ) );

        templates.Add( new FuncDataTemplate<VMServerStatus>( ( _,_ ) => new Controls.ServerStatus( ) ) );

        return this;
    }

    public override void OnReplayChanged( VMReplay replay )
    {
        StopServer( );
        LoadServerConfigurations(  );

        startServerCommand?.NotifyCanExecuteChanged( );
    }

    public void UpdateWeather( )
    {
        _serverManager?.ResendWeather(  );
    }
    
    [RelayCommand( CanExecute = nameof( CanStartServer ) )]
    public void StartServer( )
    {
        if( _serverManager == null || !_serverManager.IsRunning )
        {
            Success = false;
            
            _serverManager?.StopServer( );

            VMReplay replay = _state.Replay;
            if( replay.IsLoaded )
            {
                try
                {
                    ServerStatus.SetServerManager( _serverManager = StartServer( _settings,replay,_state.Track,_state.Players,CarSelection.CarsList.SelectedItems,_serverDebug ) );

                    StartOptions.LoopReplay.Value = _frameValidation.LoopReplay;
                    StartOptions.LoopScrubs.Value = _frameValidation.LoopScrubs; 
                    StartOptions.LoopStart.Value = (int)_frameValidation.ScrubA;
                    StartOptions.LoopEnd.Value = (int)_frameValidation.ScrubB;
                    
                    SaveServerConfigurations( );

                    ServerStatus.ServerLink = new Uri( "acmanager://race/online/join?query=race/online/join&ip=" + _settings.ServerAddress + "&httpPort=" + _settings.HttpPort );

                    string message = StartOptions.LaunchCM ? "" : "Click to launch CM: ";
                    ServerStatus.ServerAddress = $"{message}{_settings.ServerName} - {_settings.ServerAddress}:{_settings.HttpPort}";
                    
                    if( StartOptions.StartReplay )
                        StartReplay(  );
                    if( StartOptions.LaunchCM )
                        StartContentManager( );

                    Success = true;
                }
                catch( Exception ex )
                {
                    _messageManager.ShowError( "Error starting server",ex.Message );
                }
            }
            else
            {
                _messageManager.ShowError( "Error starting server","No loaded replay" );
            }
        }
        else
        {
            _serverManager.StopServer( );
            _serverManager = null;
        }
    }
    protected bool CanStartServer( )
    {
        return _state.Replay.IsLoaded;
    }

    [RelayCommand] private void StopServer( )
    {
        _serverManager?.StopServer( );
        _serverManager = null;

        Success = false;
    }

    [RelayCommand] private void StartReplay( )
    {
        _serverManager?.Play( _frameValidation );
    }
    [RelayCommand] private void StopReplay( )
    {
        _serverManager?.Stop( );
    }

    [RelayCommand] private void StartContentManager( )
    {
        ServerStatus.LauncCM( null );
    }

    [RelayCommand] private void SetLaunchCM(  )
    {
        StartOptions.LaunchCM.Value = !StartOptions.LaunchCM.Value;
    }
    [RelayCommand] private void SetLaunchAC(  )
    {
        //StartOptions.LaunchAC = !LaunchAC;
    }
    [RelayCommand] private void SetLaunchReplay(  )
    {
        StartOptions.StartReplay.Value = !StartOptions.StartReplay.Value;
    }

    [RelayCommand] private void SetLoopReplay(  )
    {
        FrameValidation.LoopReplay = !FrameValidation.LoopReplay;
    }
    [RelayCommand] private void SetLoopScrubs(  )
    {
        FrameValidation.LoopScrubs = !FrameValidation.LoopScrubs;
    }

    private static ServerManager StartServer( ACServerSettings settings,VMReplay replay,VMTrackInfo trackInfo,VMPlayerList players,ObservableCollection<VMCarInfo> additionalCars,VMServerDebug serverDebug )
    {
        settings.TrackID = trackInfo.TrackID;
        settings.VariantID = trackInfo.VariantID;

        //logger?.Information( "Creating server: {trackID} - {variantID}",settings.TrackID,settings.VariantID );
        if( trackInfo.PitBoxes > 0 && players.Count >= trackInfo.PitBoxes )
            players.RemoveAt( players.Count - 1 );

        /*
        if( trackInfo != null )
            settings.CSPSettingsFile = _state.GetCSPSettingsFile( trackInfo.TrackID );
        else
        */
        settings.CSPSettingsFile = null;

        //if( settings.CSPSettingsFile != null )
        //  logger?.Information( "CSP settings: {cspFile}",settings.CSPSettingsFile );

        //logger?.Information( "Creating server manager" );
        ServerManager serverManager = new ServerManager( settings,players,replay.TrackObjects,serverDebug );
        //_serverManager.PropertyChanged += ServerManager_PropertyChanged;

        //logger?.Information( "Starting server" );

        serverManager.StartServer( additionalCars,replay.ReplayFrequency,0 );

        return serverManager;
    }

    private void LoadServerConfigurations( )
    {
        if( _state.Replay.IsLoaded )
        {
            Settings settings = _fileManager.GetServerSettings( _state.Replay.FileName );

            StartOptions.Deserialize( settings );
            ServerOptions.Deserialize( settings );
            ServerWeather.Deserialize( settings );
            ServerPorts.Deserialize( settings );

            while( CarSelection.CarsList.SelectedItems.Count > 0 )
                CarSelection.CarsList.SelectedItems[0].IsSelected = false;
            CarSelection.CarsList.ActiveItem = null;
            
            LoadSelectedCars( settings );
        }
    }
    private void SaveServerConfigurations( )
    {
        Settings settings = _fileManager.GetServerSettings( _state.Replay.FileName );
        
        StartOptions.Serialize( settings );
        ServerOptions.Serialize( settings );
        ServerWeather.Serialize( settings );
        ServerPorts.Serialize( settings );

        SaveSelectedCars( settings );
        
        settings.SaveFile(  );
    }

    private void LoadSelectedCars( Settings settings )
    {
        int count = settings.LoadInt( "CARS","Cars" );
        for( int i = 0; i < count; i++ )
        {
            string? s = settings.LoadString( "CARS",i.ToString( ) );

            if( s != null )
            {
                string[] split = s.Split( ';' );

                if( split.Length == 2 )
                {
                    var car = CarSelection.CarsList.FirstOrDefault( ( x ) => x.CarID.Equals( split[0] ) );
                    if( car != null )
                    {
                        car.IsSelected = true;
                        car.SelectedSkinID = split[1];
                    }
                }
            }
        }
    }
    private void SaveSelectedCars( Settings settings )
    {
        int index = 0;
        foreach( var car in CarSelection.CarsList.SelectedItems )
        {
            settings.Save( "CARS",index++.ToString( ),$"{car.CarID};{car.SelectedSkinID}" );
        }
        settings.Save( "CARS","Cars",index );
    }

    private void WeatherTypeValueChanged( string value )
    {
        if( Enum.TryParse( value,out WeatherFxType v ) )
        {
            WeatherType weatherType = WeatherTypeValue.WeatherTypeProvider.GetWeatherType( v );

            _settings.Weather.WeatherData.Type = v;
            _settings.Weather.WeatherData.UpcomingType = v;

            ServerWeather.RainIntensity.Value = weatherType.RainIntensity;
            ServerWeather.RainWater.Value = weatherType.RainWater;
            ServerWeather.RainWetness.Value = weatherType.RainWetness;

            UpdateWeather( );
        }
        /*
        _settings.Weather.WeatherData.Type = value;
        _settings.Weather.WeatherData.UpcomingType = value;
        _settings.Weather.WeatherData.RainIntensity = weatherType.RainIntensity;
        _settings.Weather.WeatherData.RainWater = weatherType.RainWater;
        _settings.Weather.WeatherData.RainWetness = weatherType.RainWetness;
        _settings.Weather.WeatherData.Humidity = weatherType.Humidity;
        */
    }
}
