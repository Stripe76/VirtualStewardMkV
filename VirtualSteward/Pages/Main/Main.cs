using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

using ShadUI;
using Avalonia.Controls.Templates;

using Framework.Settings;

using Framework.UI;
using Framework.UI.ViewModels;

using VirtualSteward.Classes;

namespace VirtualSteward.Pages.Main;

public partial class Main : Feature
{
    private readonly State _state;
    private readonly Settings _settings;
    private readonly FilesManager _fileManager;
    private readonly MessageManager _messageManager;

    public SideBar SideBar { get; }
    public ActivePage Pages { get; } = [];

    public ToastManager ToastManager { get; } = new ();
   
    // ReSharper disable VirtualMemberCallInConstructor
    public Main( MainWindow window,DataTemplates templates,ThemeWatcher themeWatcher,Settings settings,Settings carsSettings )
    {
#if !DEBUG
#error Update version
#endif
        window.Title = "Virtual Steward MkV - BETA 1";

        _settings = settings;
        _fileManager = new FilesManager( _settings,carsSettings );
        _state = new State( _fileManager );
        
        _themeWatcher = themeWatcher;
        _messageManager = new MessageManager(ToastManager);

        _ = new Features.ProgressBar.ProgressBar(templates);

        Replays.Replays replays = new Replays.Replays( _state,templates,"Replay",window,_fileManager,_messageManager );
        Server.Server server = new Server.Server( _state,templates,"Server",replays.Timelines.ReplayTimeline,replays.FrameValidation,_fileManager,_messageManager );
        Options.Options options = new Options.Options( _state,templates,"Settings",_fileManager );

        Pages.Add( new Home.Home( _state,templates,"Home",_fileManager,_messageManager,replays,server ),false,true );
        Pages.Add( new Separator(  ) );
        Pages.Add( replays );
        Pages.Add( server );
        Pages.Add( new Separator(  ) );
        Pages.Add( options,false,options.CheckSettings(  ) );

        SideBar = new SideBar( Pages ) { IsExpanded = true };

        AddDataTemplates( templates );
        AddDefaultDataTemplates( templates );

        OnWindowLoading( );
    }

    public override Feature AddDataTemplates(DataTemplates templates)
    {
        templates.Add( new FuncDataTemplate<Main>( (_,_) => new Features.Main.Pages.Main( ) ) );

        return this;
    }

    public void OnWindowLoading( )
    {
        foreach( var page in Pages )
        {
            if( page is Feature feature )
                feature.OnLoading( _settings );
        }
        //_settings.SaveFile( );
        SideBar.IsExpanded = _settings.LoadBool( "SETTINGS","SiderExpanded",true );
    }
    public async Task OnWindowLoaded( )
    {
        foreach( var page in Pages )
        {
            if( page is Feature feature )
                await feature.OnLoaded( _settings );
        }
    }
    public void OnWindowClosing( )
    {
        _settings.Save( "SETTINGS","SiderExpanded",SideBar.IsExpanded );
        
        foreach( var page in Pages )
        {
            if( page is Feature feature )
                feature.OnClosing( _settings );
        }
        _settings.SaveFile( _fileManager.GetSettingsFilename(  ) );
    }

    #region Theme
    private readonly ThemeWatcher _themeWatcher;

    public ThemeMode CurrentTheme
    {
        get;
        private set => SetProperty(ref field, value);
    }

    [RelayCommand]
    private void SwitchTheme()
    {
        CurrentTheme = CurrentTheme switch
        {
#if DEBUG
            ThemeMode.Dark => ThemeMode.Light,
            ThemeMode.Light => ThemeMode.Dark,
            ThemeMode.System => ThemeMode.Light,
#else
            ThemeMode.System => ThemeMode.Light,
            ThemeMode.Light => ThemeMode.Dark,
            _ => ThemeMode.System
#endif
        };
        _themeWatcher.SwitchTheme(CurrentTheme);
    }
    #endregion
}