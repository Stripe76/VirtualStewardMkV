using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;

namespace Framework.UI.Values;

public partial class FilenameValue : BaseValue<string>
{
  public enum DialogType
  {
    Open,
    Save,
  };

  public DialogType DialogMode;

  public string FileName
  {
    get => Value != null ? Path.GetFileNameWithoutExtension( Value ) : string.Empty;
    set => Value = Path.Combine( FileFolder,value + FileExtension );
  }
  public string FileFolder
  {
    get => Value != null ? Path.GetDirectoryName( Value ) ?? string.Empty : string.Empty;
  }
  public string FileExtension
  {
    get => Value != null ? Path.GetExtension( Value ) : string.Empty;
    set => Value = Path.Combine( FileFolder,FileName + value );
  }

  public List<FilePickerFileType> FilesFilter = []; 

  public FilenameValue( string name,string title,DialogType dialogMode = DialogType.Open ) : base( "",name,title )
  {
    DialogMode = dialogMode;

    ValueChanged = ( value ) => { OnPropertyChanged( nameof( FileName ) ); };
  }

  [RelayCommand] protected async Task Browse( )
  {
    if( Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop )
      return;

    var window = desktop.MainWindow;

    if( DialogMode == DialogType.Open )
    {
      if( window is { StorageProvider.CanOpen: true } )
      {
        //var directory = window.StorageProvider.TryGetFolderFromPathAsync(new Uri(folder));

        var task = window.StorageProvider.OpenFilePickerAsync( new FilePickerOpenOptions( )
        {
          FileTypeFilter = FilesFilter,
          //SuggestedStartLocation = 
        } );
        if( await task is { Count: > 0 } )
        {
          Value = task.Result[0].Name;
        }
      }
    }
    else
    {
      if( window is { StorageProvider.CanSave: true } )
      {
        //var directory = window.StorageProvider.TryGetFolderFromPathAsync(new Uri(folder));

        var task = window.StorageProvider.SaveFilePickerAsync( new FilePickerSaveOptions( )
        {
          FileTypeChoices = FilesFilter,
          //SuggestedStartLocation = directory
        } );
        if( await task is not null && task.Result is not null )
        {
          Value = task.Result.TryGetLocalPath( );
        }
      }
    }
    return;
  }
}
