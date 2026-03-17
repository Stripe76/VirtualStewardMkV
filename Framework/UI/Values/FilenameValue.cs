using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
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

  private bool _showOverwrite;
  
  public bool ShowOverwrite
  {
    get => _checkOverwrite && File.Exists( Value );
  }

  [ObservableProperty] private bool _canOverwrite;
  [ObservableProperty] private bool _checkOverwrite;

  public List<FilePickerFileType> FilesFilter = []; 

  public FilenameValue( string name,string title,DialogType dialogMode = DialogType.Open ) : base( "",name,title )
  {
    DialogMode = dialogMode;
  }

  protected override void OnValueChanged( )
  {
    CanOverwrite = false;

    OnPropertyChanged( nameof( FileName ) );

    if( CheckOverwrite )
    {
      CanOverwrite = !File.Exists( Value );

      OnPropertyChanged( nameof( ShowOverwrite ) );
    }
    else
    {
      CanOverwrite = true;
    }
    base.OnValueChanged( );
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
          //ShowOverwritePrompt = !CheckOverwrite,
          FileTypeChoices = FilesFilter,
          //SuggestedStartLocation = directory
        } );
        if( await task is not null && task.Result is not null )
        {
          Value = task.Result.TryGetLocalPath( );

          CanOverwrite = true;
        }
      }
    }
    return;
  }
}
