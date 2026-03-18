using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;

namespace Framework.UI.Values;

public partial class FolderValue : BaseValue<string>
{
    public string FileName
    {
        get => Value ?? "";
        set => Value = value;
    }

    public bool ShowOverwrite => false;

    public FolderValue( string value,string name,string title ) : base( value,name,title )
    {
    }

    [RelayCommand]
    protected async Task Browse( )
    {
        if( Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop )
            return;

        var window = desktop.MainWindow;
        if( window is { StorageProvider.CanPickFolder: true } )
        {
            //var directory = window.StorageProvider.TryGetFolderFromPathAsync(new Uri(folder));

            var task = window.StorageProvider.OpenFolderPickerAsync( new FolderPickerOpenOptions( ) );

            if( await task is { Count: > 0 } )
            {
                Value = task.Result[0].Path.ToString( ).Replace( "file:///","" ).Replace( "file://","" );

                OnPropertyChanged( nameof( FileName ) );
            }
        }
    }
}