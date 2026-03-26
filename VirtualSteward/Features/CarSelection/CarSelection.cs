using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using ACLibrary.Cars;

using Avalonia.Controls.Templates;

using Framework.UI;
using Framework.Settings;
using VirtualSteward.Classes;
using VirtualSteward.Features.CarSelection.ViewModels;
using VirtualSteward.Features.ProgressBar.ViewModel;

namespace VirtualSteward.Features.CarSelection;

public class CarSelection : Feature
{
  private readonly FilesManager _filesManager;

  private static List<CarInfo>? _allCars = null;
  private static Task<List<CarInfo>>? loadingTask = null;

  public VMProgress Progress { get; set; }

  public VMCarsTree CarsTree { get; }
  public VMCarInfoList SelectedCars { get; set; }

  public CarSelection( DataTemplates? templates,string title,FilesManager filesManager,VMCarInfoList selectedCars )
  {
    _filesManager = filesManager;

    Progress = new VMProgress( );

    SelectedCars = selectedCars;
    CarsTree = new VMCarsTree( SelectedCars,SearchFilterChanged );

    if( templates != null )
      AddDataTemplates( templates );
  }

  public override Feature AddDataTemplates( DataTemplates templates )
  {
    templates.Add( new FuncDataTemplate<CarSelection>( ( _,_ ) => new Pages.CarSelection( ) ) );
    templates.Add( new FuncDataTemplate<VMCarsTree>( ( _,_ ) => new Controls.CarsTree( ) ) );
    templates.Add( new FuncDataTemplate<VMSearchFilter>( ( _,_ ) => new Controls.SearchFilter( ) ) );

    return this;
  }

  public override async Task OnLoaded( Settings settings )
  {
    //if( _carsListsFolder != null && LoadCarsLists( _carsListsFolder,CarsLists,_logger ) )
    //SelectedCarsList = CarsLists[0];

    await LoadCarsInfosListAsync( _filesManager.ACCarsFolder,SelectedCars,Progress,null );
  }

  public async Task LoadCarsInfosListAsync( string carsFolder,VMCarInfoList cars,IProgress<float>? progress = null,Serilog.ILogger? logger = null )
  {
    try
    {
      progress?.Report( 0 );

      if( Path.Exists( carsFolder ) )
      {
        if( IsWorking.Now( IsWorking.Tasks.CarsInfosLoading ) )
        {
          if( loadingTask != null )
            _allCars = await loadingTask;
        }
        else
        {
          using IsWorking loading = new( IsWorking.Tasks.CarsInfosLoading );

          if( _allCars == null )
          {
            loadingTask = Task.Run( ( ) => CarInfo.GetCarsInfos( carsFolder,progress ) );

            _allCars = await loadingTask;
          }
        }
        if( _allCars != null )
        {
          cars.SupressNotification = true;
          foreach( var info in _allCars )
            cars.Add( new VMCarInfo( info,0,carsFolder ) );
          cars.SupressNotification = false;
        }
      }
      progress?.Report( -1 );
    }
    catch( TaskAlreadyRunning tx )
    {
      logger?.Error( "Task already running: {message}",tx.Task );
    }
    catch( Exception ex )
    {
      logger?.Error( "Error during LoadCarsInfosListAsync: {message}",ex.Message );

      progress?.Report( -2 );
    }
  }

  private void SearchFilterChanged( string? searchFilter )
  {
    {
      if( IsValidSearchText( searchFilter ) )
      {
        string[] keys = searchFilter.Split( " " );
        foreach( var info in SelectedCars )
        {
          info.IsEnabled = IsMatch( info,keys );
        }
        CarsTree.ExpandAll = true;

        SelectedCars.Refresh(  );
      }
      else if( CarsTree.ExpandAll )
      {
        foreach( var info in SelectedCars )
          info.IsEnabled = true;
        CarsTree.ExpandAll = false;
        
        SelectedCars.Refresh(  );
      }
    }
  }

  private static readonly char[] _numbers = ['0','1','2','3','4','5','6','7','8','9'];
  private static bool IsValidSearchText( string searchText )
  {
    if( string.IsNullOrEmpty( searchText ) )
      return false;

    string[] keys = searchText.Split( " " );
    foreach( string key in keys )
    {
      if( key.Length >= 3 )
        return true;
      if( key.Length >= 2 && searchText.IndexOfAny( _numbers ) >= 0 )
        return true;
    }
    return false;
  }
  private static bool IsMatch( VMCarInfo info,string[] keys )
  {
    info.SearchKeys ??= info.Brand.ToLower( ) + " " + info.Model.ToLower( );

    if( info.SearchKeys != null )
    {
      foreach( string key in keys )
      {
        if( info.SearchKeys.IndexOf( key,StringComparison.Ordinal ) < 0 )
          return false;
      }
      return true;
    }
    return false;
  }
}