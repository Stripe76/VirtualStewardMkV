using System.IO;
using System.Linq;
using ACLibrary.Cars;
using Framework.Bindables;
using Framework.UI;

namespace VirtualSteward.Features.CarSelection.ViewModels;

public class VMCarInfo : UIItem
{
  private readonly VMCarSkinInfoList _skinInfoList = new ( );

  public string Title => Model;

  public string CarID { get; init; }
  public string Model { get; init; }
  public string Brand { get; init; }

  public uint NumberOfWings { get; init; }

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

  public int SelectedSkinIndex
  {
    get
    {
      for( int i = 0; i < _skinInfoList.Count; i++ )
        if( _skinInfoList[i] == SelectedSkin )
          return i+1;
      return 0;
    }
    set
    {
      value--;
      if( value >= 0 && value < _skinInfoList.Count )
        SelectedSkin = _skinInfoList[value];
      OnPropertyChanged( nameof( SelectedSkinIndex ) );
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
    CarID = carID;
    Model = carID;
    Brand = "";

    NumberOfWings = 0;

    _skinInfoList = [];
  }
  public VMCarInfo( CarInfo? info,uint numberOfWings,string? skinsFolder = null )
  {
    CarID = info?.CarID??"";
    Model = info?.Model??"";
    Brand = info?.Brand??"";

    NumberOfWings = numberOfWings;
      
    if( info != null && skinsFolder != null )
    {
      foreach( var skin in info.Skins )
        _skinInfoList.Add( new VMCarSkinInfo( skin,Path.Combine( skinsFolder,CarID,"skins",skin.Name,"preview.jpg" ) ) );
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

