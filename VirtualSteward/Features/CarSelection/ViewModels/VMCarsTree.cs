using System;
using Framework.UI.ViewModels;

namespace VirtualSteward.Features.CarSelection.ViewModels;

public class VMCarsTree : TreePath<VMCarInfo>
{
    public VMSearchFilter SearchFilter { get; }

    public VMCarsTree( VMCarInfoList carsList,Action<string>? serachFilterChanged ) : base( carsList,["Brand"] )
    {
        var serachFilterChanged1 = serachFilterChanged;
        ShowCheckbox = true;

        SearchFilter = new VMSearchFilter( )
        {
            MinWidth = 300,
            ValueChanged = (value) => serachFilterChanged1?.Invoke( value ?? "" ) 
        };
    }
}