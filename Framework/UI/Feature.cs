using System.Windows.Input;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Platform.Storage;

using Framework.UI.Values;
using Framework.UI.ViewModels;
using Framework.UI.Configurations;

namespace Framework.UI;

public partial class Feature : UIItem
{
    private readonly FeatureList _loadingList = [];
    private readonly ConfigurationList _configurations = [];

    [ObservableProperty] private bool _error = false;
    [ObservableProperty] private bool _warning = false;
    [ObservableProperty] private bool _success = false;
    
    public  string HeaderTitle { get; init; }

    public Feature(DataTemplates? templates = null, string headerTitle = "")
    {
        HeaderTitle = headerTitle;

        if (templates != null) AddDataTemplates(templates);
    }

    public void AddConfiguration( Configuration configuration )
    {
        _configurations.Add( configuration );
    }
    public void AddLoadingPage( Feature feature )
    {
        _loadingList.Add( feature );
    }

    public virtual Feature AddDataTemplates(DataTemplates templates)
    {
        //templates.Add(new FuncDataTemplate<Module>((value,namescope) => new Controls.ReplayLoadingPanel()));
        return this;
    }
    public virtual Feature AddCommands(UIItemList commands)
    {
        return this;
    }

    public virtual Feature AddPage(UIBaseList pages, string? headerTitle = null)
    {
        /*
        if( headerTitle != null )
          pages.Add(new VMTabItem(_headerTitle, new ContentControl() { Content = this }));
        else
          */
        pages.Add(this);

        return this;
    }
    public virtual Feature AddFooter(UIBaseList pages, string? headerTitle = null)
    {
        /*
        if( headerTitle != null )
          pages.Add(new VMTabItem(_headerTitle, new ContentControl() { Content = this }));
        else
          */
        pages.Add(this);

        return this;
    }

    public virtual Feature AddProgress(UIBaseList controls)
    {
        return this;
    }

    public virtual void OnLoading( Settings.Settings settings )
    {
        foreach( var configuration in _configurations )
            configuration.Deserialize( settings );
        foreach( var feature in _loadingList )
            feature.OnLoading( settings );
    }
    public virtual async Task OnLoaded( Settings.Settings settings )
    {
        foreach( var feature in _loadingList )
            await feature.OnLoaded( settings );
    }
    public virtual void OnClosing( Settings.Settings settings )
    {
        foreach( var feature in _loadingList )
            feature.OnClosing( settings );
        foreach( var configuration in _configurations )
            configuration.Serialize( settings );
    }

    public static void AddDefaultDataTemplates(DataTemplates templates)
    {
        templates.Add(new FuncDataTemplate<Configuration>((_, _) => new Controls.Configuration()));

        templates.Add( new FuncDataTemplate<TreeLeafCheckbox>( ( _,_ ) => new Controls.TreePathViewLeafCheckbox( ) ) );
        templates.Add( new FuncDataTemplate<TreeLeaf>( ( _,_ ) => new Controls.TreePathViewLeaf( ) ) );
        templates.Add( new FuncDataTemplate<TreeNode>( ( _,_ ) => new Controls.TreePathViewNode( ) ) );

        templates.Add( new FuncDataTemplate<BaseInt>( ( _,_ ) => new Inputs.TextboxInput( ) ) );
        templates.Add( new FuncDataTemplate<BaseBool>( ( _,_ ) => new Inputs.CheckboxInput( ) ) );
        templates.Add( new FuncDataTemplate<BaseSwitchBool>( ( _,_ ) => new Inputs.SwitchInput( ) ) );
        templates.Add( new FuncDataTemplate<BaseThreeStateBool>( ( _,_ ) => new Inputs.CheckboxInput( ) ) );

        templates.Add( new FuncDataTemplate<RangedInt>( ( _,_ ) => new Inputs.SliderInput( ) ) );
        templates.Add( new FuncDataTemplate<RangedUInt>( ( _,_ ) => new Inputs.SliderInput( ) ) );
        templates.Add( new FuncDataTemplate<RangedFloat>( ( _,_ ) => new Inputs.SliderInput( ) ) );

        templates.Add(new FuncDataTemplate<MappedValueInt>((_, _) => new Inputs.MappedInput()));
        templates.Add(new FuncDataTemplate<MappedValueUInt>((_, _) => new Inputs.MappedInput()));
        
        templates.Add(new FuncDataTemplate<SideBar>((_, _) => new Controls.SideBar()));
        templates.Add(new FuncDataTemplate<Toolbar>((_, _) => new Controls.Toolbar()));
        templates.Add(new FuncDataTemplate<ActivePage>((_, _) => new Controls.ActivePage()));

        templates.Add( new FuncDataTemplate<RepeatCommand>( ( _,_ ) => new Controls.RepeatCommand( ) ) );
        templates.Add( new FuncDataTemplate<ToggleCommand>( ( _,_ ) => new Controls.ToggleCommand( ) ) );
        templates.Add( new FuncDataTemplate<FeatureCommand>( ( _,_ ) => new Controls.FeatureCommand( ) ) );

        templates.Add( new FuncDataTemplate<FolderValue>( ( _,_ ) => new Inputs.FilenameInput( ) ) );
        templates.Add( new FuncDataTemplate<FilenameValue>( ( _,_ ) => new Inputs.FilenameInput( ) ) );
    }

    public override string ToString()
    {
        return HeaderTitle;
    }

    protected IStorageFile? OpenFile(string path)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;
        var window = desktop.MainWindow;
        if (window is { StorageProvider.CanOpen: true })
        {
            return window.StorageProvider.TryGetFileFromPathAsync(new Uri(path)).GetAwaiter().GetResult();
        }
        return null;
    }

    protected Task<IReadOnlyList<IStorageFile>>? PickFilesAsync(string folder, FilePickerFileType fileTypes)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var window = desktop.MainWindow;
        if (window is { StorageProvider.CanOpen: true })
        {
            //var directory = window.StorageProvider.TryGetFolderFromPathAsync(new Uri(folder));

            var task = window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                FileTypeFilter = [fileTypes],
                //SuggestedStartLocation = directory
            });
            //return await task;

            return task;
        }

        return null;
    }
}

public partial class FeatureCommand : UIItem
{
    [ObservableProperty] private bool _isIcon;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isCancel;
    [ObservableProperty] private bool _isDefault;

    public string Icon
    {
        get;
        set
        {
            field = value;

            IsIcon = true;

            OnPropertyChanged( nameof( Icon ) );
        }
    } = "";

    public string Text { get; init; } = "";
    public string Tooltip { get; init; } = "";

    public ICommand? RoutedCommand { get; init; } = null;
    public object? CommandParameter { get; init; } = null;
}

public class FeatureList : List<Feature>
{

}

public class RepeatCommand : FeatureCommand
{
    
}

public partial class ToggleCommand : FeatureCommand
{
    [ObservableProperty] private object? _object;
    [ObservableProperty] private string? _property;

    public bool IsChecked
    {
        get
        {
            if( _property != null )
            {
                var property = _object?.GetType( ).GetProperty( _property );
                if( property != null )
                {
                    if( property.GetValue( _object ) is not null and bool value )
                        return value;
                }
            }
            return false;
        }
        set
        {
            if( _property != null )
            {
                var property = _object?.GetType( ).GetProperty( _property );
                if( property != null )
                    property.SetValue( _object,value );
            }
        }
    }
}

public class FeatureCommandList : ObservableCollection<FeatureCommand>
{

}
