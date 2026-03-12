using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using Framework.Bindables;

namespace VirtualSteward.Features.CarSelection.ViewModels;

public class VMCarsList : BindableBase
{
  private string _listName = string.Empty;

  public string FileName { get; set; }
  public string FullFilePath { get; set; }

  public string ListName 
  {
    get => _listName;
    set => SetProperty( ref _listName,value );
  }

  public bool IsTrafficCars = false;

  public VMCarsListItemList Cars { get; set; } = [];

  public VMCarsList( string listName, IList<VMCarInfo>? cars = null )
  {
    FileName = listName;
    ListName = listName;
    FullFilePath = string.Empty;

    if( cars != null )
    {
      foreach( var car in cars )
        Cars.Add( new( car.CarID,car.SkinID ) );
    }
  }
  public VMCarsList( string file,string[] fileLines )
  {
    FullFilePath = file;
    FileName = Path.GetFileNameWithoutExtension( file );
    ListName = FileName;

    foreach( string line in fileLines )
    {
      if( line.StartsWith( "## " ) )
        ListName = line.Substring( 3 );
      else
      {
        string carID = line;
        string skinID = "";

        if( line.Contains( ';' ) )
        {
          string[] ar = line.Split( ';' );

          carID = ar[0];
          skinID = ar[1];
        }
        Cars.Add( new( carID,skinID ) );
      }
    }
  }
}

public class VMCarsListList : BindingList<VMCarsList>
{

}

public class VMCarsListItem( string carID, string skinID)
{
  public string CarID { get; set; } = carID;
  public string SkinID { get; set; } = skinID;
}

public class VMCarsListItemList : BindingList<VMCarsListItem>
{

}
