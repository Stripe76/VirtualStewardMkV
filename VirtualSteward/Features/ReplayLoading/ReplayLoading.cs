using System;
using System.IO;
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
	private readonly FeatureCommand _openReplay,_mergeReplay; 

	[ObservableProperty] private string _title = "Current file:";

	public ReplayLoading( State state,DataTemplates templates,FilesManager fileManager,MessageManager messageManager ) : base( state,templates )
	{
		_fileManager = fileManager;
		_messageManager = messageManager;

		_openReplay = new FeatureCommand( )
		{
			Icon = "\xf1f9",
			Tooltip = "Open replay file",
			RoutedCommand = LoadReplayCommand
		};
		_mergeReplay = new FeatureCommand( )
		{
			Icon = "\xf1fa",
			Tooltip = "Merge replay file",
			RoutedCommand = MergeReplayCommand
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
		commands.Add( _mergeReplay );
		
		return this;
	}

	public override void OnReplayChanged(VMReplay replay)
	{
		OnPropertyChanged( nameof( CurrentReplay ) );

		MergeReplayCommand.NotifyCanExecuteChanged(  );
	}
	
	[RelayCommand( CanExecute = nameof(CanLoadReplay) )]
	public async Task LoadReplay( string? e )
	{
		string? filename = e; 
		if( e == null )
		{
			var task = PickFilesAsync( "/mnt/data/Users/Sim Racing/Documents/Assetto Corsa/replay/",ACReplayFiles );
			if( task is not null && await task is { Count: > 0 } )
			{
				IStorageFile file = task.Result[0];
				filename = file.TryGetLocalPath( );
			}
		}
		if( filename != null )
		{
			_openReplay.IsBusy = true;

			await LoadReplay( filename,_state,_fileManager,_messageManager,false,_progress );
			
			_openReplay.IsBusy = false;
		}
		else
		{
			// TODO: error
		}
	}
	private bool CanLoadReplay()
	{
		//return !IsWorking.Now( IsWorking.Tasks.ReplayFileLoading );
		return true;
	}

	[RelayCommand( CanExecute = nameof(CanMergeReplay) )]
	public async Task MergeReplay( string? e )
	{
		string? filename = e; 
		if( e == null )
		{
			var task = PickFilesAsync( "/mnt/data/Users/Sim Racing/Documents/Assetto Corsa/replay/",ACReplayFiles );
			if( task is not null && await task is { Count: > 0 } )
			{
				IStorageFile file = task.Result[0];
				
				filename = file.TryGetLocalPath( );
			}
		}
		if( filename != null )
		{
			string? error = CheckReplayCompatibility( _state.Replay.FileFullPath,filename );
			
			if( error is null )
			{
				_openReplay.IsBusy = true;

				await LoadReplay( filename,_state,_fileManager,_messageManager,true,_progress );

				_openReplay.IsBusy = false;
			}
			else
			{
				_messageManager.ShowError( "Incompatible replays",error );
			}
		}
		else
		{
			// TODO: error
		}
	}
	private bool CanMergeReplay()
	{
		return _state.Replay.IsLoaded;
	}

	private static async Task LoadReplay( string filename,State state,FilesManager filesManager,MessageManager messageManager,bool mergeReplay,VMProgress? progress = null )
	{
		try
		{
			using IsWorking loading = new( IsWorking.Tasks.ReplayFileLoading );

			progress?.Report( 0 );
			if( progress != null )
				messageManager.ShowProgress( "Loading replay file",progress );

			Replay? acReplay = await Task.Run( () => Replay.LoadReplay( filename,progress ) );
			
			if( acReplay != null )
			{
				VMReplay replay = mergeReplay ? state.Replay : new VMReplay( acReplay,acReplay.TrackObjects,acReplay.TrackObjectsNumber );
				VMPlayerList players = state.Players;

				//players.SupressNotification = true;
				if( !mergeReplay )
					players.Clear( );

				int id = 0;
				foreach( var newCar in acReplay.Cars )
				{
					VMCarInfo carInfo = new ( state.GetCarInfo( newCar.CarID ),newCar.NumberOfWings,filesManager.ACCarsFolder );
					IImmutableSolidColorBrush carColor = VMMapLineStyle.LineColors[id % VMMapLineStyle.LineColors.Count];

					VMPlayer newPlayer = new(
						id,
						newCar,
						acReplay.TailData[id],
						carInfo,
						carInfo.GetSkin( newCar.CarSkinID ),
						state.GetPlayerLabelStyle( ),
						state.GetPlayerLineStyle( id ),
						state.GetPlayerCarImage( id,newCar.CarID,newCar.CarSkinID ) 
						);
					players.Add( newPlayer );

					id++;
				}
				//state.Replay.FileName = replay.FileName;
				players.SupressNotification = false;
				
				if( !mergeReplay )
					state.Replay = replay;
			}
			progress?.Report( -1 );
			messageManager.ShowSuccess( "Replay loaded" );
		}
		catch( TaskAlreadyRunning )
		{
			//logger?.Error("Task already running: {message}", tx.Task);
			messageManager.ShowError( "Error loading replay","Another replay is already loading" );
		}
		catch( Exception ex )
		{
			//logger?.Error("Error in LoadReplayFileAsync: {message}", ex.Message);

			progress?.Report( -2 );
			messageManager.ShowError( "Error loading replay",ex.Message );
		}
	}

	private static FilePickerFileType ACReplayFiles { get; } = new("Assetto Corsa replay file (*.acreplay)")
	{
		Patterns = ["*.acreplay"],
	};  
	
	private static string? CheckReplayCompatibility( string replayA,string replayB )
	{
		if( File.Exists( replayA ) && File.Exists( replayB ) )
		{
			try
			{
				ReplayInfo? infoA = ReplayInfo.LoadReplayInfo( replayA );
				ReplayInfo? infoB = ReplayInfo.LoadReplayInfo( replayB );

				if( infoA != null && infoB != null )
				{
					if( infoA.TrackID != infoB.TrackID || infoA.TrackVariantID != infoB.TrackVariantID )
					{
						return "Cannot merge replay files from different tracks";
					}
					if( infoA.Frequency != infoB.Frequency )
					{
						return "Cannot merge replay files with different recording frequency";
					}
					return null;
				}
			}
			catch( Exception )
			{
			}
		}
		return "Error checking replays compatibility";
	}
}
