using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Avalonia.Platform.Storage;
using VirtualSteward.Datasources;
using VirtualSteward.Features.FileTemplates.Classes;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;

namespace VirtualSteward.Features.ReplayExport.Exports.CSVFile;

public class CSVFileExport( FileTemplateList templates ) : BaseExport( "CSVFile","As CSV data file" )
{
  private readonly FileTemplateValue _fileTemplates = new FileTemplateValue( templates,"FilesTemplates","" );  

  public object? Parameter => _fileTemplates;

  public override string FilesExtension { get; } = ".csv";
  public override List<FilePickerFileType> FilesFilter { get; } = [new FilePickerFileType( "CSV files" ) { Patterns = ["*.csv"] },new FilePickerFileType( "All files" ) { Patterns = ["*.*"] }];

  public override void ExportReplay( string filename,VMReplay replay,IList<VMPlayer> players,uint startFrame,uint endFrame,IProgress<float>? progress = null )
  {
    if( _fileTemplates.Value != null )
    {
      FileFieldList? fields = _fileTemplates.Value.Fields;
      if( fields != null )
      {
        int count = players.Count;
        for( int i = 0; i < count; i++ )
        {
          VMPlayer player = players[i];
          string fileName = filename;
          if( count > 1 )
            fileName = fileName.Replace( ".csv",$" - {SanitazeFileName( player.PlayerInfo.PlayerName )}.csv" );

          using( TextWriter writer = new StreamWriter( File.Open( fileName,FileMode.Create,FileAccess.Write,FileShare.None ) ) )
          {
            WriteCSVHeader( writer,fields,',' );
            WriteCSVData( writer,1.0f / replay.ReplayFrequency,player,player.Datasource,fields,',' );
          }
          progress?.Report( (i+1)/(float)count );
        }
      }
    }
  }

  private static void WriteCSVHeader( TextWriter writer,FileFieldList fields,char separator )
  {
    StringBuilder sb = new( );
    foreach( var field in fields )
    {
      if( field == null )
        continue;
      if( sb.Length > 0 )
        sb.Append( separator );
      sb.Append( field.Header );
    }
    writer.WriteLine( sb );
  }

  private static void WriteCSVData( TextWriter writer,double replayFrequency,VMPlayer player,CarDatasource datasource,FileFieldList fields,char separator )
  {
    int count = datasource.Length;

    StringBuilder sb = new StringBuilder( );
    for( uint i = 0; i < count; i++ )
    {
      foreach( var field in fields )
      {
        if( field == null )
          continue;
        if( sb.Length > 0 )
          sb.Append( separator );
        if( field.Field == "Frame" )
        {
          sb.Append( i );
          continue;
        }
        if( field.Field == "Time" )
        {
          sb.Append( (i * replayFrequency).ToString( ).Replace( ',','.' ) );
          continue;
        }
        sb.Append( datasource.GetFieldValue( i,field.Field ) );
      }
      writer.WriteLine( sb );
      sb.Clear( );
    }
  }

  private static string SanitazeFileName( string fileName )
  {
    // Source - https://stackoverflow.com/a
    // Posted by DenNukem, modified by community. See post 'Timeline' for change history
    // Retrieved 2026-01-21, License - CC BY-SA 3.0

    char[] invalids =[.. System.IO.Path.GetInvalidFileNameChars( ),':','/','\\'];
    return String.Join( "_",fileName.Split( invalids,StringSplitOptions.RemoveEmptyEntries ) ).TrimEnd( '.' );
  }
}

public class CSVField( string header,string field )
{
  public string Header { get; } = header;
  public string Field { get; } = field;
}