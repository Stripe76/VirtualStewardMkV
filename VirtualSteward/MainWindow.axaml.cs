using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Framework.Settings;
using ShadUI;
using VirtualSteward.Pages.Main;

namespace VirtualSteward;

public partial class MainWindow : ShadUI.Window
{
    public MainWindow()
    {
        InitializeComponent();

        string settingsFile = Path.Combine( AppContext.BaseDirectory,"Settings","VirtualSteward.ini" );
        if( !File.Exists( settingsFile ) )
            settingsFile = Path.Combine( AppContext.BaseDirectory,"Virtual Steward.ini" );
        string carsSettingsFile = Path.Combine( AppContext.BaseDirectory,"Cars","Cars.ini" );

        if( Application.Current != null )
            DataContext = new Main( this,DataTemplates,new ThemeWatcher( Application.Current ),new Settings( settingsFile ),new Settings( carsSettingsFile ) );
    }

    #region Window events
    private async void Window_Loaded( object? sender,RoutedEventArgs e )
    {
        if( DataContext is not null and Main main )
            await main.OnWindowLoaded( );
    }
    private void Window_Closing( object? sender,WindowClosingEventArgs e )
    {
        if( DataContext is not null and Main application )
            application.OnWindowClosing( );
    }
    #endregion

}