using System;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.Input;
using Framework.UI;
using VirtualSteward.ACNetwork.Shared;
using VirtualSteward.ACNetwork.Weather;
using VirtualSteward.Classes;
using VirtualSteward.ViewModels;

using VirtualSteward.Features.Server.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Server.Classes;
using VirtualSteward.Features.Server.Configurations;
using VirtualSteward.Features.Server.Values;

namespace VirtualSteward.Pages.Server;

public partial class Server : StateFeature
{
    private readonly MessageManager _messageManager;

    private readonly ACServerSettings _settings = new ACServerSettings( );
    private readonly VMServerDebug _serverDebug = new VMServerDebug( );

    private ServerManager? _serverManager = null;

    public string Icon { get; } = "\xf202";

    public FeatureCommand ServerStart { get; }

    public CMServerPorts ServerPorts { get; }
    public CMServerOptions ServerOptions { get; }
    public CMServerWeather ServerWeather { get; }

    public Server( State state,DataTemplates templates,string title,FilesManager filesManager,MessageManager messageManager ) : base( state,templates,title )
    {
        _messageManager = messageManager;

        ServerPorts = new CMServerPorts( _settings );
        ServerOptions = new CMServerOptions( _settings );
        ServerWeather = new CMServerWeather( _settings )
        {
            WeatherType =
            {
                ValueChanged = WeatherTypeValueChanged
            }
        };

        ServerStart = new FeatureCommand( )
        {
            IsDefault = true,
            Text = "Start server",
            RoutedCommand = StartServerCommand
        };
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<Server>( ( _,_ ) => new Pages.Server( ) ) );
        templates.Add( new FuncDataTemplate<WeatherTypeValue>( ( _,_ ) => new Framework.UI.Inputs.ComboboxInput( ) ) );

        return this;
    }

    [RelayCommand( CanExecute = nameof( CansStartServer ) )]
    protected void StartServer( )
    {
        if( _serverManager == null || !_serverManager.IsRunning )
        {
            _serverManager?.StopServer( );

            VMReplay replay = _state.Replay;
            if( replay.IsLoaded )
            {
                try
                {
                    _serverManager = StartServer( _settings,replay,_state.Players,_state.Track,_serverDebug );
                    _serverManager.Play( 0,10000 );
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

    protected bool CansStartServer( )
    {
        return _state.Replay.IsLoaded;
    }

    private static ServerManager StartServer( ACServerSettings settings,VMReplay replay,VMPlayerList players,VMTrackInfo trackInfo,VMServerDebug serverDebug )
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

        serverManager.StartServer( null,replay.ReplayFrequency,0 );

        return serverManager;
    }

    private void WeatherTypeValueChanged( WeatherFxType value )
    {
        WeatherType weatherType = WeatherTypeValue.WeatherTypeProvider.GetWeatherType( value );

        ServerWeather.RainIntensity.Value = weatherType.RainIntensity;
        ServerWeather.RainWater.Value = weatherType.RainWater;
        ServerWeather.RainWetness.Value = weatherType.RainWetness;

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
