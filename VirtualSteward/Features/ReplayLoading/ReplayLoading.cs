using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

using Avalonia.Controls.Templates;
using Avalonia.Platform.Storage;

using ACLibrary.Replays;
using Avalonia.Media;
using Framework.UI;

using VirtualSteward.Classes;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ProgressBar.ViewModel;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.ReplayLoading;

public partial class ReplayLoading : StateFeature
{
	private readonly FilesManager _fileManager;
	private readonly VMProgress _progress = new();

	[ObservableProperty] private string _title = "Current file:";
	[ObservableProperty] private FeatureCommandList _commands = [];

	/*
	public VMReplay CurrentReplay
	{
		get => _state.Replay;
	}
	*/

	public ReplayLoading( State state,DataTemplates templates,FilesManager fileManager,UIBaseList? progressList = null ) : base( state,templates )
	{
		_fileManager = fileManager;
		
		_commands.Add(
			new FeatureCommand( )
			{
				Icon = "\xf1f9;",
				Text = "Open replay file",
				Tooltip = "Open replay file",
				RoutedCommand = LoadReplayCommand
			}
		);
		progressList?.Add(_progress);
	}

	public override void AddDataTemplates( DataTemplates templates )
	{
		//templates.Add( new FuncDataTemplate<ReplayLoading>( ( value,namescope ) => new Controls.ReplayLoadingPanel( ) ) );
	}
	public override void AddCommands(UIItemList commands)
	{
		foreach (var command in Commands)
			commands.Add(command);
	}

	public override Feature AddProgress(UIBaseList controls)
	{
		controls.Add(_progress);

		return this;
	}

	public override void OnReplayChanged(VMReplay replay)
	{
		OnPropertyChanged(nameof(CurrentReplay));
	}
	
	[RelayCommand( CanExecute = nameof(CanLoadReplay) )]
	public async void LoadReplay( string? e )
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
			//using (new WaitCursor())
			{
				LoadReplay( file,_state,_fileManager,_progress );
			}
		}
	}
	private bool CanLoadReplay()
	{
		//return !IsWorking.Now( IsWorking.Tasks.ReplayFileLoading );
		return true;
	}

	private static async void LoadReplay( IStorageFile file,State state,FilesManager filesManager,VMProgress? progress = null )
	{
		try
		{
			using IsWorking loading = new( IsWorking.Tasks.ReplayFileLoading );

			progress?.Report( 0 );

			string? filename = file.TryGetLocalPath( );
			if( filename != null )
			{
				Replay? acReplay = await Task.Run( () => Replay.LoadReplay( filename,progress ) );
				
				if( acReplay != null )
				{
					VMReplay replay = new( acReplay );
					VMPlayerList players = state.Players;
					//VMTrackObjects trackObjects = state.TrackObjects;

					players.Clear( );

					int id = 0;
					foreach( var newCar in acReplay.Cars )
					{
						//VMCarInfo carInfo = state.GetCarInfo( newCar.CarID );
						IImmutableSolidColorBrush carColor = VMMapLineStyle.LineColors[id % VMMapLineStyle.LineColors.Count];
						
						VMPlayer newPlayer = new ( 
							id,
							newCar,
							acReplay.TailData[id],
							new VMMapImage( filesManager.GetCarImage( newCar.CarID,newCar.CarSkinID,carColor ) ),
							new VMMapLineStyle( 2,carColor ))
						{
							//PlayerName = newCar.PlayerName
							//ShowDetails = true
						};
						players.Add( newPlayer );

						id++;
					}
					/*
					if( trackObjects != null )
					{
					  trackObjects.SetData(acReplay.TrackObjects, acReplay.TrackObjectsNumber);
					}
					*/
					//state.Replay.FileName = replay.FileName;
					state.Replay = replay;
				}
				progress?.Report( -1 );
			}
		}
		catch( TaskAlreadyRunning )
		{
			//logger?.Error("Task already running: {message}", tx.Task);
		}
		catch( Exception  )
		{
			//logger?.Error("Error in LoadReplayFileAsync: {message}", ex.Message);

			progress?.Report( -2 );
		}
	}

	private static FilePickerFileType ACReplayFiles { get; } = new("Assetto Corsa replay file (*.acreplay)")
	{
		Patterns = ["*.acreplay"],
	};  
}
