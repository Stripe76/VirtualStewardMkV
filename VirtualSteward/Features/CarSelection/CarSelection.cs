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
  public VMCarInfoList CarsList { get; set; }

  public CarSelection( DataTemplates? templates,string title,FilesManager filesManager,VMCarInfoList selectedCars )
  {
    _filesManager = filesManager;

    Progress = new VMProgress( );

    CarsList = selectedCars;
    CarsTree = new VMCarsTree( CarsList );

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

    await LoadCarsInfosListAsync( _filesManager.ACCarsFolder,CarsList,Progress,null );
  }

  public static async Task LoadCarsInfosListAsync( string carsFolder,VMCarInfoList cars,IProgress<float>? progress = null,Serilog.ILogger? logger = null )
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
}