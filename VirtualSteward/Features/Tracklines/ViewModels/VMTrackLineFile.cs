using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Avalonia.Media;

using ACLibrary.Tracklines;
using Avalonia;
using Framework.UI;
using Framework.Bindables;

using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.Tracklines.ViewModels;

public class VMTracklineFile( string filePath ) : UIItem
{
  private VMTracklineList? _lines = null;

  public VMTracklineList? Lines
  {
    get => _lines;
    set => SetProperty( ref _lines,value );
  }

  public string Title => ToString( );

  public string FileName { get; set; } = System.IO.Path.GetFileName( filePath );
  public string FileFullPath { get; set; } = filePath;

  public override string ToString( )
  {
    return FileName;
  }

  #region Colors
  public static IList<IImmutableSolidColorBrush> LineColors
  {
    get => [.. _lineBrushes];
  }

  private static readonly IImmutableSolidColorBrush[] _lineBrushes = { Brushes.Black,
                                                             Brushes.Red,
                                                             Brushes.Green,
                                                             Brushes.Blue,
                                                             Brushes.BlueViolet,
                                                             Brushes.MediumTurquoise,
                                                             Brushes.Brown,
                                                             Brushes.Orange,
                                                             Brushes.LimeGreen,
                                                             Brushes.Olive,
                                                             Brushes.Bisque,
                                                             Brushes.Plum,
                                                             Brushes.PowderBlue,
                                                             Brushes.Purple,
                                                             Brushes.Salmon,
                                                             Brushes.SteelBlue,
                                                             Brushes.Goldenrod,
                                                             Brushes.DarkSlateBlue,
                                                             Brushes.Tan,
                                                             Brushes.Khaki,
                                                           };
  #endregion
}

public class VMTracklineFileList( ) : MultiList<VMTracklineFile>( )
{
  public VMTracklineFile? FindFile( string filename )
  {
    foreach( var file in Items )
    {
      if( file.FileName.Equals( filename,StringComparison.CurrentCultureIgnoreCase ) )
        return file;
    }
    return null;
  }
}

