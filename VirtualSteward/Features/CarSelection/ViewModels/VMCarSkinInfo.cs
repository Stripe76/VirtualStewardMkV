using System;
using System.IO;
using ACLibrary.Cars;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Framework.UI;
using Framework.Bindables;

namespace VirtualSteward.Features.CarSelection.ViewModels;

public class VMCarSkinInfo : UIItem
{
    private Bitmap? _imageBitmap = null; 
    
    public string SkinID;
    public string Title {  get; set; }

    public Bitmap? PreviewImageBitmap
    {
        get
        {
            if( File.Exists( PreviewImageFile ) )
                return _imageBitmap ??= new Bitmap( PreviewImageFile );
            return _imageBitmap = new Bitmap( AssetLoader.Open( new Uri( "avares://VirtualSteward/Assets/ADLarge.png" ) ) );
        }
    }
    public string PreviewImageFile { get; internal set; }

    public VMCarSkinInfo( string carID,string skinID,string carsFolder )
    {
        SkinID = skinID;
        Title = skinID;
        PreviewImageFile = Path.Combine( carsFolder,carID,"skins",skinID,"preview.jpg" );
    }
    public VMCarSkinInfo( VMCarSkinInfo copySkinInfo )
    {
        SkinID = copySkinInfo.SkinID;
        Title = copySkinInfo.Title;
        PreviewImageFile = copySkinInfo.PreviewImageFile;
    }
    public VMCarSkinInfo( CarSkinInfo info,string imageFile )
    { 
        SkinID = info.Name;
        Title = info.Title;
        PreviewImageFile = imageFile;
    }

    public override string ToString( )
    {
        return Title;
    }
}

public class VMCarSkinInfoList : MultiList<VMCarSkinInfo>
{
}
