using System.Windows.Input;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Platform.Storage;
using Framework.UI.Values;

using ActivePage = Framework.UI.ViewModels.ActivePage;
using Configuration = Framework.UI.Configurations.Configuration;
using SideBar = Framework.UI.ViewModels.SideBar;
using Toolbar = Framework.UI.ViewModels.Toolbar;

namespace Framework.UI;

public partial class Feature : UIItem
{
    [ObservableProperty] private string _headerTitle;

    public Feature(DataTemplates? templates = null, string headerTitle = "")
    {
        HeaderTitle = headerTitle;

        if (templates != null)
            AddDataTemplates(templates);
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

    }
    public virtual void OnLoaded( Settings.Settings settings )
    {

    }
    public virtual void OnClosing( Settings.Settings settings )
    {

    }

    public static void AddDefaultDataTemplates(DataTemplates templates)
    {
        templates.Add(new FuncDataTemplate<Configuration>((_, _) => new Controls.Configuration()));

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

        templates.Add(new FuncDataTemplate<RepeatCommand>((_, _) => new Controls.RepeatCommand()));
        templates.Add(new FuncDataTemplate<FeatureCommand>((_, _) => new Controls.FeatureCommand()));

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

    public string Text { get; set; } = "";
    public string Tooltip { get; set; } = "";

    public ICommand? RoutedCommand { get; set; } = null;
}

public class RepeatCommand : FeatureCommand
{
    
}

public class FeatureCommandList : ObservableCollection<FeatureCommand>
{

}