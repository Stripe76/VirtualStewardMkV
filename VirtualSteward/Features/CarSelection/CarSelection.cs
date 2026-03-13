using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using ACLibrary.Cars;

using Avalonia.Controls.Templates;

using Framework.UI;
using Framework.UI.ViewModels;
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

  public VMProgress Progress
  {
    get;
    set;
  }

  public VMCarInfoList SelectedCars { get; set; }
  public TreePath<VMCarInfo> SelectCarsTree { get; }

  //public static readonly RoutedCommand Command_CreateNewList = new ( "CreateNewList",typeof( CarSelection ) );

  public CarSelection( DataTemplates? templates,string title,FilesManager filesManager,VMCarInfoList selectedCars )
  {
    _filesManager = filesManager;
    
    Progress = new VMProgress( );
    
    SelectedCars = selectedCars;
    //SelectedCars.SelectedItems.ListChanged += SelectedCars_ListChanged;
    SelectCarsTree = new TreePath<VMCarInfo>( SelectedCars,["Brand"] ) { ShowCheckbox = true };

    if( templates != null )
      AddDataTemplates( templates );
  }

  public override Feature AddDataTemplates( DataTemplates templates )
  {
    templates.Add( new FuncDataTemplate<CarSelection>( ( _,_ ) => new Pages.CarSelectionNew( ) ) );
    //templates.Add( new FuncDataTemplate<VMCarInfo>( ( _,_ ) => new Controls.Car( ) ) );
    templates.Add( new FuncDataTemplate<TreePath<VMCarInfo>>( ( _,_ ) => new Framework.UI.Controls.TreePathView( ) ) );
    //templates.Add( new FuncDataTemplate<VMCarInfoList>( ( _,_ ) => new TreePathView( ) ) );

    return this;
  }

  public override async Task OnLoaded( Settings settings )
  {
    //if( _carsListsFolder != null && LoadCarsLists( _carsListsFolder,CarsLists,_logger ) )
      //SelectedCarsList = CarsLists[0];

    await LoadCarsInfosListAsync( _filesManager.ACCarsFolder,SelectedCars,Progress,null );
  }

  /*
  public override void AddCommand( IList<FeatureCommand>? commands,UIElement? routingCtrl = null )
  {
    routingCtrl?.CommandBindings.Add( new CommandBinding( Command_CreateNewList,CreateNewList,CreateNewList_CanExecute ) );
  }
  */

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
}