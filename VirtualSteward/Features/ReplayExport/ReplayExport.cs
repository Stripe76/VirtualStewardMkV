using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls.Templates;

using Framework.UI;
using Framework.UI.Values;

using VirtualSteward.Classes;
using VirtualSteward.Features.FileTemplates.Classes;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ProgressBar.ViewModel;
using VirtualSteward.Features.Timelines.ViewModels;
using VirtualSteward.Features.ReplayExport.Values;
using VirtualSteward.Features.ReplayExport.Exports;
using VirtualSteward.Features.ReplayExport.Exports.ACReplay;
using VirtualSteward.Features.ReplayExport.Exports.CSVFile;
using VirtualSteward.Features.ReplayLoading.ViewModels;

namespace VirtualSteward.Features.ReplayExport;

public partial class ReplayExport : StateFeature
{
	private readonly MessageManager _messageManager;

	private readonly VMProgress _progress = new( );
	private readonly FeatureCommandList _commands = [];

	public ExporterValue Exporters { get; }
	public PlayersExportValue PlayersExport { get; }
	public TimelineExportValue TimelineExport { get; }
	public FilenameValue FilenameExport { get; }
	public FeatureCommand ExportCommand { get; }
	public FeatureCommand CancelCommand { get; }

	public ReplayExport( 
		State state,
		DataTemplates templates,
		VMPlayerList players,
		VMTimelineList timelines,
		FileTemplateList fileTemplates,
		FilesManager fileManager,
		MessageManager messageManager ) : base( state,templates )
	{
		_messageManager = messageManager;

		_commands.Add(
			new FeatureCommand( )
			{
				Icon = "\xf2be",
				Text = "Export replay file",
				Tooltip = "Export replay file",
				RoutedCommand = ShowReplayExportPageCommand
			}
		);
		PlayersExport = new PlayersExportValue( players );
		TimelineExport = new TimelineExportValue( timelines );
		FilenameExport = new FilenameValue( "","",FilenameValue.DialogType.Save ) { CheckOverwrite = true };

		Exporters = new ExporterValue( [new ACReplayExport( ),new CSVFileExport( fileTemplates )] )
		{
			ValueChanged = OnExporters_ValueChanged
		};
		if( Exporters.Value != null )
		{
			FilenameExport.FilesFilter = Exporters.Value.FilesFilter;
		}
		ExportCommand = new FeatureCommand( )
		{
			IsDefault = true,
			Text = "Export",
			Tooltip = "Export replay file",
			RoutedCommand = ExportReplayCommand
		};
		CancelCommand = new FeatureCommand( )
		{
			IsCancel = true,
			Icon = "\xf344",
			Tooltip = "Close",
			RoutedCommand = CloseCommand
		};
	}

	public override Feature AddDataTemplates( DataTemplates templates )
	{
		templates.Add( new FuncDataTemplate<ReplayExport>( ( value,namescope ) => new Pages.ReplayExport( ) ) );
		templates.Add( new FuncDataTemplate<ExporterValue>( ( value,namescope ) => new Framework.UI.Inputs.MultiListInput(  ) ) );
		templates.Add( new FuncDataTemplate<PlayersExportValue>( ( value,namescope ) => new Framework.UI.Inputs.MultiListInput( ) ) );
		templates.Add( new FuncDataTemplate<TimelineExportValue>( ( value,namescope ) => new Framework.UI.Inputs.MultiListInput( ) ) );

		return this;
	}
	public override Feature AddCommands( UIItemList commands )
	{
		foreach( var command in _commands )
			commands.Add( command );
		return this;
	}

	public override void OnReplayChanged( VMReplay replay )
	{
		IsActive = false;

		FilenameExport.Value = replay.FileFullPath;

		ShowReplayExportPageCommand.NotifyCanExecuteChanged( );
	}

	[RelayCommand] protected async void ExportReplay( )
	{
		var exporter = Exporters.Value;
		if( exporter != null )
		{
			var players = PlayersExport.Value?.Players;
			if( players != null )
			{
				var timeline = TimelineExport.Value;
				if( timeline != null )
				{
					uint startFrame = timeline.OnlySegment ? timeline.Timeline.ScrubA : 0;
					uint endFrame = timeline.OnlySegment ? timeline.Timeline.ScrubB : timeline.Timeline.End;
					string? filename = FilenameExport.Value;

					if( filename != null )
					{
						if( !FilenameExport.CanOverwrite )
						{
							_messageManager.ShowError( "File already exists",$"Cannot overwrite \"{FilenameExport.FileName}{FilenameExport.FileExtension}\" file" );
							
							return;
						}
						ExportCommand.IsBusy = true;

						_messageManager.ShowProgress( "Exporting replay file",_progress );

						string? result = await ExportReplay( filename,exporter,_state.Replay,players,startFrame,endFrame,_progress );

						if( result != null )
							_messageManager.ShowError( "Error exporting replay",result );
						else
							_messageManager.ShowSuccess( "Replay exported",result );

						ExportCommand.IsBusy = false;
					}
				}
			}
		}
	}
	private bool CanExportReplay( )
	{
		return FilenameExport is { Value: not null,CanOverwrite: true };
	}
	
	[RelayCommand(CanExecute = nameof(CanShowReplayExportPage))] protected void ShowReplayExportPage( )
	{
		IsActive = !IsActive;
	}
	private bool CanShowReplayExportPage( )
	{
		return _state.Replay.IsLoaded;
	}

	[RelayCommand] protected void Close( )
	{
		IsActive = false;
	}

	private static async Task<string?> ExportReplay( string filename,
		BaseExport exporter,
		VMReplay replay,
		IList<VMPlayer> players,
		uint startFrame,
		uint endFrame,
		VMProgress? progress = null )
	{
		try
		{
			using IsWorking loading = new( IsWorking.Tasks.ReplayFileExporting );

			progress?.Report( 0 );

			await Task.Run( ( ) => exporter.ExportReplay( filename,replay,players,startFrame,endFrame,progress ) );

			progress?.Report( -1 );
		}
		catch( TaskAlreadyRunning )
		{
			//logger?.Error("Task already running: {message}", tx.Task);
			return "Another replay is already exporting";
		}
		catch( Exception ex )
		{
			//logger?.Error("Error in LoadReplayFileAsync: {message}", ex.Message);

			progress?.Report( -2 );

			return ex.Message;
		}
		return null;
	}
	
	private void OnExporters_ValueChanged( BaseExport? exporter )
	{
		if( exporter != null )
		{
			FilenameExport.FileExtension = exporter.FilesExtension;
			FilenameExport.FilesFilter = exporter.FilesFilter;
		}
	}
}