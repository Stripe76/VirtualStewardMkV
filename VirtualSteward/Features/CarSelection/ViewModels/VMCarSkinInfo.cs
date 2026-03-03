using ACLibrary.Cars;

using Framework.UI;
using Framework.Bindables;

namespace VirtualSteward.Features.CarSelection.ViewModels;

public class VMCarSkinInfo : UIItem
{
    public string ID;
    public string Title {  get; set; }
    public string ImageFile { get; set; } = "";

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
