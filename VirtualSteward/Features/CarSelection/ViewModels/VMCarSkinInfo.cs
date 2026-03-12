using ACLibrary.Cars;
using Avalonia.Media.Imaging;
using Framework.UI;
using Framework.Bindables;

namespace VirtualSteward.Features.CarSelection.ViewModels;

public class VMCarSkinInfo : UIItem
{
    private Bitmap? _imageBitmap = null; 
    
    public string ID;
    public string Title {  get; set; }
    public string ImageFile { get; set; } = "";

    public Bitmap? ImageBitmap
    {
        get
        {
            return _imageBitmap ??= new Bitmap( ImageFile );
        }
    }

    public VMCarSkinInfo( string id )
    {
        ID = id;
        Title = id;
    }
    public VMCarSkinInfo( CarSkinInfo info,string imageFile )
    { 
        ID = info.Name;
        Title = info.Title;
        ImageFile = imageFile;
    }

    public override string ToString( )
    {
        return Title;
    }
}

public class VMCarSkinInfoList : MultiList<VMCarSkinInfo>
{
}
