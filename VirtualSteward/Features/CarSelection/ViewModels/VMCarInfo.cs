using System.IO;
using System.Linq;
using ACLibrary.Cars;
using Framework.Bindables;
using Framework.UI;

namespace VirtualSteward.Features.CarSelection.ViewModels;

public class VMCarInfo : UIItem
{
  private readonly VMCarSkinInfoList _skinInfoList = new ( );

  private string _carID;

  public string Model { get; init; }
  public string Brand { get; init; }

  public string SkinID 
  {
    get => CarSkinsList.SelectedItem != null ? CarSkinsList.SelectedItem.ID : "";
    set 
    {
      VMCarSkinInfo? selectSkin = _skinInfoList.FirstOrDefault(skinInfo => skinInfo.ID.Equals(value));
      if( selectSkin != null )
        SelectedSkin = selectSkin;
    }
  }

  public VMCarSkinInfo? SelectedSkin
  {
    get => CarSkinsList.SelectedItem;

    set
    {
      CarSkinsList.SelectedItem = value; 
      
      OnPropertyChanged( nameof( SkinID ) );
      OnPropertyChanged( nameof( SelectedSkin ) );
    }
  }

  public string? SearchKeys = null;

  public bool IsFilterMatch( string filter )
  {
    return true;
  }

  public VMCarSkinInfoList CarSkinsList
  {
    get => _skinInfoList; 
  }

  public VMCarInfo( string carID )
  {
    _carID = carID;
    Model = carID;
    Brand = "";

    _skinInfoList = [];
  }
  public VMCarInfo( CarInfo? info,string? skinsFolder = null )
  {
    _carID = info?.CarID??"";
    
    Model = info?.Model??"";
    Brand = info?.Brand??"";

    if( skinsFolder != null )
    {
      foreach( var skin in info.Skins )
        _skinInfoList.Add( new VMCarSkinInfo( skin,Path.Combine( skinsFolder,_carID,"skins",skin.Name,"preview.jpg" ) ) );
      if( _skinInfoList.Count > 0 )
        _skinInfoList[0].IsSelected = true;
    }
  }

  public VMCarSkinInfo GetSkin( string skinID )
  {
    foreach( var skinInfo in _skinInfoList )
      if( skinInfo.ID.Equals( skinID ) )
        return skinInfo;
    return new VMCarSkinInfo( skinID );
  }

  public override string ToString( )
  {
    return Model;
  }
}

public class VMCarInfoList( bool multiSelect ) : MultiList<VMCarInfo>( multiSelect,false,false )
{
}

