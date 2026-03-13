using System;
using System.IO;
using ACLibrary.Tracks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Framework.UI;
using Framework.Bindables;

using VirtualSteward.Features.Tracklines.ViewModels;

namespace VirtualSteward.ViewModels;

public class VMTrackInfo : UIItem
{
  private Bitmap? _imageBitmap = null; 
  private string? _cspSettingsFile = null;

  private bool _isLoaded = false;

  public bool IsLoaded
  {
    get => _isLoaded;
  }

  public new bool IsHighlighted
  {
    get => HasAILines;
  }

  public string TrackID { get; internal set; }
  public string VariantID { get; internal set; }
  public string TrackName { get; internal set; }
  public string VariantName { get; internal set; }

  public string Nation {  get; internal set; }

  public string MapImageFile { get; internal set; }
  public string PreviewImageFile { get; internal set; }

  public Bitmap? PreviewImageBitmap
  {
    get
    {
      if( File.Exists( PreviewImageFile ) )
        return _imageBitmap ??= new Bitmap( PreviewImageFile );
      return _imageBitmap = new Bitmap( AssetLoader.Open( new Uri( "avares://VirtualSteward/Assets/AD.png" ) ) );
    }
  }

  public string Track
  {
    get => TrackName;
  }
  public string Variant
  {
    get => VariantName + (HasAILines ? " - AI lines" : "");
  }

  public int PitBoxes = 0;

  public string? CSPSettingsFilePath
  {
    get => _cspSettingsFile;
    set
    {
      if( SetProperty( ref _cspSettingsFile,value ) )
        OnPropertyChanged( nameof( CSPSettingsFileName ) );
    }
  }
  public string? CSPSettingsFileName
  {
    get => _cspSettingsFile != null ? Path.GetFileName( _cspSettingsFile ) : "Not found";
  }

  public bool HasAILines
  { 
    get => TracklineFiles.Count > 0;
  }

  public VMTracklineFileList TracklineFiles
  {
    get;
  }

  public VMTrackInfo( string trackID,string variantID )
  {
    TrackID = trackID;
    VariantID = variantID;
    TrackName = trackID;
    VariantName = variantID;
    Nation = "ITA";
    MapImageFile = string.Empty;
    PreviewImageFile = string.Empty;

    TracklineFiles = new ( );
  }
  public VMTrackInfo( TrackInfo info,string? tracksFolder = null )
  { 
    TrackID = info.TrackID;
    VariantID = info.VariantID;
    TrackName = info.TrackName;
    VariantName = info.VariantName;
    Nation = info.Nation;
    PitBoxes = info.PitBoxes;

    if( tracksFolder != null )
    {
      MapImageFile = Path.Combine( tracksFolder,TrackID,"ui",VariantID,"outline.png" );
      PreviewImageFile = Path.Combine( tracksFolder,TrackID,"ui",VariantID,"preview.png" );
    }
    else
    {
      MapImageFile= string.Empty;
      PreviewImageFile = string.Empty;
    }
    TracklineFiles = new( );
    foreach( var track in info.TracklineFiles )
      TracklineFiles.Add( new VMTracklineFile( track ) );

    _isLoaded = true;
  }

  public override string ToString( )
  {
    return TrackName;
  }
}

public class VMTrackInfoList : MultiList<VMTrackInfo>
{
}