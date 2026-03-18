using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ACLibrary.Replays;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Framework.UI;
using Framework.Settings;
using Framework.UI.ViewModels;

using VirtualSteward.Classes;
using VirtualSteward.ViewModels;
using VirtualSteward.Pages.Home.ViewModels;
using VirtualSteward.Features.CarSelection.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Pages.Home.Configurations;

namespace VirtualSteward.Pages.Home;

public partial class Home : StateFeature
{
    private readonly FilesManager _filesManager;
    private readonly Replays.Replays _replays;
    private readonly Server.Server _server;
    private readonly VMReplayPreviewList _allReplays = [];

    [ObservableProperty] private bool _replaysLoading = false;

    public string Icon { get; } = "\xf225";

    public CMHomeSettings HomeSettings;

    public VMReplayPreviewList LatestReplays { get; } = new( "Latests" );
    public VMReplayPreviewList RecentReplays { get; } = new( "Recents" );

    public TreePath<VMReplayPreview,VMReplayGroupTreeNode>? ReplaysTree { get; set; }

    public Home( State state,
                 DataTemplates templates,
                 string title,
                 FilesManager filesManager,
                 MessageManager messageManager,
                 Replays.Replays replays,
                 Server.Server server ) : base( state,templates,title )
    {
        _server = server;
        _replays = replays;
        _filesManager = filesManager;

        AddConfiguration( HomeSettings = new CMHomeSettings( this ) );

        ReplaysLoading = LatestReplays.IsBusy = true;
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<Home>( ( _,_ ) => new Pages.Home( ) ) );

        templates.Add( new FuncDataTemplate<VMCarInfo>( ( _,_ ) => new Controls.CarInfoPreview( ) ) );
        templates.Add( new FuncDataTemplate<VMReplayPreview>( ( _,_ ) => new Controls.ReplayPreview( ) ) );
        templates.Add( new FuncDataTemplate<VMReplayPreviewList>( ( _,_ ) => new Controls.ReplayPreviewList( ) ) );
        templates.Add( new FuncDataTemplate<VMReplayGroupTreeNode>( ( _,_ ) => new Controls.ReplayGroupTreeNode( ) ) );

        templates.Add( new FuncDataTemplate<TreePath<VMReplayPreview,VMReplayGroupTreeNode>>( ( _,_ ) => new Framework.UI.Controls.TreePathView( ) ) );

        return this;
    }

    public override void OnACFolderChanged( )
    {
        if( ReplaysTree != null )
            LoadReplays( _filesManager );
    }

    public override void OnReplayFolderChanged( )
    {
        if( ReplaysTree != null )
            LoadReplays( _filesManager );
    }

    public override void OnReplayChanged( VMReplay replay )
    {
        if( replay.IsLoaded && replay.FileFullPath != string.Empty )
            AddRecentReplay( _state,replay.FileFullPath );
    }

    public override void OnLoading( Settings settings )
    {
        base.OnLoading( settings );
        
        LoadRecentReplays( _state,settings );
    }
    public override async Task OnLoaded( Settings settings )
    {
        await LoadReplays( _filesManager );

        ReplaysLoading = LatestReplays.IsBusy = false;

        ReplaysTree = new TreePath<VMReplayPreview,VMReplayGroupTreeNode>( _allReplays,
        [
            "{By date}/MonthGrouping",
            "{By track}/TrackName",
            "{By car}/CarName",
            "{By player}/PlayerName",
        ] );
        OnPropertyChanged( nameof( ReplaysTree ) );

        await base.OnLoaded( settings );
    }
    public override void OnClosing( Settings settings )
    {
        int c = 1;
        foreach( var replay in RecentReplays )
        {
            if( replay.FileFullPath != string.Empty )
                settings.Save( "RECENT_REPLAYS",c++.ToString( ),replay.FileFullPath );
        }
        HomeSettings.LatestCollapsed.Value = !LatestReplays.IsExpanded;
        HomeSettings.RecentCollapsed.Value = !RecentReplays.IsExpanded;

        base.OnClosing( settings );
    }

    private void AddRecentReplay( State state,string? file )
    {
        if( file is { Length: > 0 } )
        {
            bool found = false;
            foreach( var info in RecentReplays )
            {
                if( info.FileFullPath.Equals( file ) )
                {
                    found = true;

                    RecentReplays.Remove( info );
                    RecentReplays.Insert( 0,info );

                    break;
                }
            }
            if( !found )
            {
                ReplayInfo? replay = ReplayInfo.LoadReplayInfo( file );
                if( replay != null )
                {
                    var trackInfo = new VMTrackInfo( state.GetTrackInfo( replay.TrackID,replay.TrackVariantID,false ),_filesManager.ACTracksFolder );
                    var carInfo = new VMCarInfo( state.GetCarInfo( replay.CarID ),0,_filesManager.ACCarsFolder );
                    var carSKinInfo = new VMCarSkinInfo( replay.CarID,replay.CarSkinID,_filesManager.ACCarsFolder );

                    RecentReplays.Insert( 0,new VMReplayPreview( new VMReplayInfo( replay,trackInfo,carInfo,carSKinInfo ),GetCommands( replay.FileFullPath ) ) );
                    while( RecentReplays.Count > 6 )
                        RecentReplays.RemoveAt( RecentReplays.Count - 1 );
                }
            }
        }
    }
    private void LoadRecentReplays( State state,Settings settings )
    {
        for( int c = 1; c <= 6; c++ )
        {
            string? file = settings.LoadString( "RECENT_REPLAYS",c.ToString( ) );
            if( !string.IsNullOrEmpty( file ) )
            {
                ReplayInfo? replay = ReplayInfo.LoadReplayInfo( file );
                if( replay != null )
                {
                    var trackInfo = new VMTrackInfo( state.GetTrackInfo( replay.TrackID,replay.TrackVariantID,false ),_filesManager.ACTracksFolder );
                    var carInfo = new VMCarInfo( state.GetCarInfo( replay.CarID ),0,_filesManager.ACCarsFolder );
                    var carSKinInfo = new VMCarSkinInfo( replay.CarID,replay.CarSkinID,_filesManager.ACCarsFolder );

                    RecentReplays.Add( new VMReplayPreview( new VMReplayInfo( replay,trackInfo,carInfo,carSKinInfo ),GetCommands( replay.FileFullPath ) ) );
                }
                while( RecentReplays.Count > 6 )
                    RecentReplays.RemoveAt( RecentReplays.Count - 1 );
            }
        }
    }

    private async Task LoadReplays( FilesManager filesManager,IProgress<float>? progress = null )
    {
        try
        {
            using IsWorking loading = new( IsWorking.Tasks.ReplaysListLoading );

            progress?.Report( 0 );

            if( Directory.Exists( filesManager.ReplaysFolder ) )
            {
                var replays = await Task.Run( ( ) => ReplayInfo.GetReplaysInfos( filesManager.ReplaysFolder,progress ) );

                List<VMReplayPreview> unsorted = [];
                foreach( var replay in replays )
                {
                    var trackInfo = new VMTrackInfo( _state.GetTrackInfo( replay.TrackID,replay.TrackVariantID,false ),filesManager.ACTracksFolder );
                    var carInfo = new VMCarInfo( _state.GetCarInfo( replay.CarID ),0,filesManager.ACCarsFolder );
                    var carSKinInfo = new VMCarSkinInfo( replay.CarID,replay.CarSkinID,filesManager.ACCarsFolder );

                    unsorted.Add( new VMReplayPreview( new VMReplayInfo( replay,trackInfo,carInfo,carSKinInfo ),GetCommands( replay.FileFullPath ) ) );
                }
                _allReplays.SupressNotification = true;
                _allReplays.Clear(  );
                _allReplays.Add( unsorted.OrderByDescending( ( x ) => x.ReplayInfo.ReplayDate ).ToList( ) );
                _allReplays.SupressNotification = false;

                LatestReplays.SupressNotification = true;
                LatestReplays.Clear(  );
                LatestReplays.Add( _allReplays.Take( 6 ).ToList( ) );
                LatestReplays.SupressNotification = false;
            }
        }
        catch( TaskAlreadyRunning tx )
        {
            //logger?.Error( "Task already running: {message}",tx.Task );
        }
        catch( Exception ex )
        {
            //logger?.Error( "Error in LoadReplaysListAsync: {message}",ex.Message );

            progress?.Report( -1 );
        }
    }

    private FeatureCommandList GetCommands( string filename )
    {
        return
        [
            new FeatureCommand( )
            {
                Icon = "\xf1ec",
                Text = "Loads the replay",
                IsDefault = true,
                RoutedCommand = LoadReplayCommand,
                CommandParameter = filename
            },
            new FeatureCommand( )
            {
                Icon = "\xf202",
                Text = "Starts the server",
                RoutedCommand = StartServerCommand,
                CommandParameter = filename
            },
            new FeatureCommand( )
            {
                Icon = "\xf22f",
                Text = "Replay information"
            }
        ];
    }

    [RelayCommand] private async Task LoadReplay( string filename )
    {
        await _replays.LoadReplay( filename );
    }
    [RelayCommand] private async Task StartServer( string filename )
    {
        _server.IsActive = true;

        await _replays.LoadReplay( filename,false,false );

        _server.StartServer( );
    }
}