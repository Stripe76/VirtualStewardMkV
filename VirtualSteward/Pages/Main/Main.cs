using ShadUI;
using CommunityToolkit.Mvvm.Input;
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
    public ActivePage Pages { get; } = new ( );

    public ToastManager ToastManager { get; } = new ();
   
    // ReSharper disable VirtualMemberCallInConstructor
    public Main( MainWindow window,DataTemplates templates,ThemeWatcher themeWatcher,Settings settings,Settings carsSettings )
    {
        _settings = settings;
        _fileManager = new FilesManager( _settings,carsSettings );
        _state = new State(_fileManager);
        
        _themeWatcher = themeWatcher;
        _messageManager = new MessageManager(ToastManager);

        _ = new Features.ProgressBar.ProgressBar(templates);

        Replays.Replays replays = new Replays.Replays( _state,templates,"Replay",window,_fileManager,_messageManager );
        Server.Server server = new Server.Server( _state,templates,"Server",_fileManager,_messageManager ); 

        Pages.Add( new Home.Home( _state,templates,"Home",_fileManager,_messageManager,replays,server ),false,true );
        Pages.Add( replays );
        Pages.Add( server );

        SideBar = new SideBar( Pages ) { IsExpanded = true };

        AddDataTemplates(templates);
        AddDefaultDataTemplates(templates);
    }

    public override Feature AddDataTemplates(DataTemplates templates)
    {
        templates.Add( new FuncDataTemplate<Main>( (_,_) => new Features.Main.Pages.Main( ) ) );

        return this;
    }

    public void OnWindowLoaded( )
    {
        foreach( var page in Pages )
        {
            if( page is Feature )
                ((Feature)page).OnLoaded( _settings );
        }
        //replayLoading.LoadReplay( "/mnt/data/Users/Sim Racing/Documents/Assetto Corsa/replay/AC_240224-220234_R_ks_mazda_mx5_cup_mugello_.acreplay" );
        //CreateFolder( Path.Combine( _folders.DocumentsFolder,"Cache" ) );


        /*
        bool showDialog = false;
        foreach( var feature in _settingsFeatures )
        {
            if( feature.CheckSettings( ) )
                showDialog = true;
        }
        if( showDialog )
            SettingsDialog.Command_ShowSettings.Execute( null,Application.Current.MainWindow );
    */
    }
    public void OnWindowClosing( )
    {
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