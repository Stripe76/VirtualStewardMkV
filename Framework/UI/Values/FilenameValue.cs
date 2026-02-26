using System.IO;

namespace Framework.UI.Values;

public class FilenameValue : BaseValue<string>
{
  public enum DialogType
  {
    None,
    Open,
    Save,
  };

  public DialogType DialogMode = DialogType.None;

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
    get => Value != null ? Path.GetExtension( Value ) :  string.Empty;
  }

  public string FilesFilter = "All files|*.*";

  public FilenameValue( string name,string title,DialogType dialogMode = DialogType.Open ) : base( "",name,title )
  {
    DialogMode = dialogMode;

    ValueChanged = ( value ) => { OnPropertyChanged( nameof( FileName ) ); };
  }
}
