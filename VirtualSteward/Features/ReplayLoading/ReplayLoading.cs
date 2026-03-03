using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Controls.Templates;

using ACLibrary.Replays;

using Framework.UI;

using VirtualSteward.Classes;
using VirtualSteward.Features.CarSelection.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ProgressBar.ViewModel;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.ReplayLoading;

public partial class ReplayLoading : StateFeature
{
	private readonly FilesManager _fileManager;
	private readonly MessageManager _messageManager;
	
	private readonly VMProgress _progress = new();
	private readonly FeatureCommand _openReplay; 

	[ObservableProperty] private string _title = "Current file:";

	public ReplayLoading( State state,DataTemplates templates,FilesManager fileManager,MessageManager messageManager ) : base( state,templates )
	{
		_fileManager = fileManager;
		_messageManager = messageManager;

		_openReplay = new FeatureCommand( )
		{
			Icon = "\xf1f9;",
			Text = "Open replay file",
			Tooltip = "Open replay file",
			RoutedCommand = LoadReplayCommand
		};
	}

	public override Feature AddDataTemplates( DataTemplates templates )
	{
		//templates.Add( new FuncDataTemplate<ReplayLoading>( ( value,namescope ) => new Controls.ReplayLoadingPanel( ) ) );
		return this;
	}
	public override Feature AddCommands( UIItemList commands )
	{
		commands.Add( _openReplay );
		
		return this;
	}

	public override void OnReplayChanged(VMReplay replay)
	{
		OnPropertyChanged(nameof(CurrentReplay));
	}
	
	[RelayCommand( CanExecute = nameof(CanLoadReplay) )]
	private async Task LoadReplay( string? e )
	{
		IStorageFile? file = null;
		if( e != null )
		{
			file = OpenFile( e );
		}
		else
		{
			var task = PickFilesAsync( "/mnt/data/Users/Sim Racing/Documents/Assetto Corsa/replay/",ACReplayFiles );
			if( task is not null && await task is { Count: > 0 } )
			{
				file = task.Result[0];
			}
		}
		if( file != null )
		{
			_openReplay.IsBusy = true;

			await LoadReplay( file,_state,_fileManager,_messageManager,_progress );
			
			_openReplay.IsBusy = false;
		}
	}
	private bool CanLoadReplay()
	{
		//return !IsWorking.Now( IsWorking.Tasks.ReplayFileLoading );
		return true;
	}

	private static async Task LoadReplay( IStorageFile file,State state,FilesManager filesManager,MessageManager messageManager,VMProgress? progress = null )
	{
		try
		{
			using IsWorking loading = new( IsWorking.Tasks.ReplayFileLoading );

			progress?.Report( 0 );
			if( progress != null )
				messageManager.ShowProgress( "Loading replay file",progress );

			string? filename = file.TryGetLocalPath( );
			if( filename != null )
			{
				Replay? acReplay = await Task.Run( () => Replay.LoadReplay( filename,progress ) );
				
				if( acReplay != null )
				{
					VMReplay replay = new( acReplay,acReplay.TrackObjects,acReplay.TrackObjectsNumber );
					VMPlayerList players = state.Players;

					players.SupressNotification = true;
					players.Clear( );

					int id = 0;
					foreach( var newCar in acReplay.Cars )
					{
						VMCarInfo carInfo = new ( state.GetCarInfo( newCar.CarID ),newCar.NumberOfWings,filesManager.ACCarsFolder );
						IImmutableSolidColorBrush carColor = VMMapLineStyle.LineColors[id % VMMapLineStyle.LineColors.Count];
						
						VMPlayer newPlayer = new ( 
							id,
							newCar,
							acReplay.TailData[id],
							carInfo,
							carInfo.GetSkin( newCar.CarSkinID ),
							new VMMapLineStyle( 2,carColor ),
							new VMMapImage( filesManager.GetCarImage( newCar.CarID,newCar.CarSkinID,carColor ) )
							)
						{
							//PlayerName = newCar.PlayerName
							//ShowDetails = true
						};
						players.Add( newPlayer );

						id++;
					}
					//state.Replay.FileName = replay.FileName;
					players.SupressNotification = false;
					
					state.Replay = replay;
				}
				progress?.Report( -1 );
				messageManager.ShowSuccess("Replay loaded");
			}
		}
		catch( TaskAlreadyRunning )
		{
			//logger?.Error("Task already running: {message}", tx.Task);
			messageManager.ShowError("Error loading replay","Another replay is already loading");
		}
		catch( Exception ex )
		{
			//logger?.Error("Error in LoadReplayFileAsync: {message}", ex.Message);

			progress?.Report( -2 );
			messageManager.ShowError("Error loading replay",ex.Message);
		}
	}

	private static FilePickerFileType ACReplayFiles { get; } = new("Assetto Corsa replay file (*.acreplay)")
	{
		Patterns = ["*.acreplay"],
	};  
}
