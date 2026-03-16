using System;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Framework.Settings;

namespace VirtualSteward.Classes;

public class FilesManager
{
  private string _vsFolder;
  private string _vsDocsFolder;
  private string _vsSettingsFolder;

  private Settings _carsSettings;

  public string ACFolder { get; set; }
  public string ReplaysFolder { get; set; }

  public string VSCarsFolder => Path.Combine( _vsFolder,"Cars" );

  public string ACCarsFolder => Path.Combine( ACFolder,"content","cars" );
  public string ACTracksFolder => Path.Combine( ACFolder,"content","tracks" ); 

  public FilesManager(Settings settings, Settings carsSettings)
  {
    _carsSettings = carsSettings;

    _vsFolder = AppDomain.CurrentDomain.BaseDirectory;
    _vsSettingsFolder = Path.Combine( _vsFolder,"Settings" );
    _vsDocsFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ),"Virtual Steward" );

    #if DEBUG
    ACFolder = settings.LoadString( "SETTINGS","ACFolder",@"/mnt/data/Steam_Linux/steamapps/common/assettocorsa/" ) ?? "";
    #else
    ACFolder = settings.LoadString( "SETTINGS","ACFolder",@"C:\Program Files (x86)\Steam\steamapps\common\assettocorsa\" ) ?? "";
    #endif
    ReplaysFolder = settings.LoadString( "SETTINGS","ReplaysFolder",Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ),"Assetto Corsa","replay" ) ) ?? "";
  }

  public string GetSettingsFilename(  )
  {
    return CreateFolder( Path.Combine( _vsSettingsFolder ),"VirtualSteward.ini" );
  }

  public Settings GetServerSettings( string replayFilename )
  {
    return new Settings( CreateFolder( Path.Combine( _vsSettingsFolder,"Servers" ),replayFilename + ".vsreplaysettings" ) );
  }

  public Bitmap GetCarImage(string carID, string skinID, IImmutableSolidColorBrush carColor, bool bFallBack = true)
  {
    string? file = _carsSettings.LoadString(carID,skinID);
    if (file == null && bFallBack)
    {
      if (skinID == "Shadow")
      {
        file = _carsSettings.LoadString("Generic","Base");
      }
      else
      {
        string? body = _carsSettings.LoadString(carID,"Base");
        body ??= _carsSettings.LoadString("Generic","Base" );

        if (body != null)
        {
          body = Path.Combine(VSCarsFolder,body);

          if (File.Exists(body))
          {
            return CreateCarImage(body, carColor);
          }
        }
      }
    }
    if (file != null)
    {
      file = $"{VSCarsFolder}{file}";
      if (File.Exists(file))
      {
        return LoadImage(file);
      }
    }
    return CreateCarRectangle(carColor);
  }

  private static Bitmap LoadImage(string file)
  {
    return new Bitmap(file);
  }
  private static Bitmap CreateCarRectangle(IImmutableSolidColorBrush carColor)
  {
    WriteableBitmap bodyBitmap = new WriteableBitmap(new PixelSize(150, 300), new Vector(96, 96));

    using ILockedFramebuffer bodyFrame = bodyBitmap.Lock();
    unsafe
    {
      var mask = new Span<uint>((byte*)bodyFrame.Address, bodyFrame.RowBytes * bodyFrame.Size.Height);

      uint color = carColor.Color.ToUInt32();
      int width = bodyFrame.Size.Width, height = bodyFrame.Size.Height;
      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          int offset = x + y * width;
          mask[offset] = color;
        }
      }
    }
    return bodyBitmap;
  }
  private static Bitmap CreateCarImage(string bodyFile, IImmutableSolidColorBrush carColor)
  {
    using var bodyStream = File.OpenRead(bodyFile);

    WriteableBitmap bodyBitmap = WriteableBitmap.DecodeToHeight(bodyStream, 300, BitmapInterpolationMode.None);

    using ILockedFramebuffer bodyFrame = bodyBitmap.Lock();
    unsafe
    {
      var mask = new Span<uint>((byte*)bodyFrame.Address, bodyFrame.RowBytes * bodyFrame.Size.Height);

      uint color = carColor.Color.ToUInt32();
      int width = bodyFrame.Size.Width, height = bodyFrame.Size.Height;
      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          int offset = x + y * width;
          if (mask[offset] == 0xFFFF00FF)
            mask[offset] = color;
        }
      }
    }
    return bodyBitmap;
  }
  
  #region Helpers
  private static string CreateFolder( string folder,string? file = null )
  {
    if( !Directory.Exists( folder ) )
      Directory.CreateDirectory( folder );
    return file != null ? Path.Combine( folder,file ) : folder;
  }
  #endregion

}