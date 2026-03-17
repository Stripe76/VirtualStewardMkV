using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VirtualSteward.Features.FileTemplates.Classes;

public class FileTemplate( string fullFilePath,string title )
{
  private FileFieldList? _fields = null;

  public string Title => title;
  public string Filename
  {
    get => Path.GetFileNameWithoutExtension( FullFilePath );
  }
  public string FullFilePath { get; set; } = fullFilePath;

  public FileFieldList? Fields
  { 
    get
    {
      return _fields ??= LoadFields( FullFilePath );
    }
  }

  private static FileFieldList? LoadFields( string fullFilePath )
  {
    if( File.Exists( fullFilePath ) )
    {
      string[] lines = File.ReadAllLines( fullFilePath );

      FileFieldList fields = [];
      for( int i = 0; i < lines.Length; i++ )
      {
        if( lines[i].Length > 0 && lines[i][0] == '#' )
          continue;
        int split = lines[i].IndexOf( '=' );
        if( split >= 0 )
          fields.Add( new( lines[i][..split],lines[i][(split + 1)..] ) );
      }
      return fields;
    }
    return null;
  }
}

public class FileTemplateList : ObservableCollection<FileTemplate>
{

}

public class FileField( string header,string field )
{
  public string Header { get; } = header;
  public string Field { get; } = field;
}

public class FileFieldList : List<FileField> 
{
}