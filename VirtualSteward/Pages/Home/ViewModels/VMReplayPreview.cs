using Framework.Bindables;
using Framework.UI;

namespace VirtualSteward.Pages.Home.ViewModels;

public class VMReplayPreview : UIItem
{
    public VMReplayInfo ReplayInfo { get; }
    public FeatureCommandList Commands { get; }

    public string Title => ReplayInfo.FileName;
    
    public string FileName => ReplayInfo.FileName;
    public string FileFullPath => ReplayInfo.FileFullPath;

    public string PlayerName => ReplayInfo.PlayerName;
    public string TrackName => ReplayInfo.TrackName;
    public string CarName => ReplayInfo.CarName;
    public string Weather => ReplayInfo.Weather;

    public string BestLapText => ReplayInfo.BestLapText;
    public string AverageLapText => ReplayInfo.AverageLapText;
    public string MonthGrouping => ReplayInfo.MonthGrouping;

    public VMReplayPreview( VMReplayInfo replayInfo,FeatureCommandList commands )
    {
        ReplayInfo = replayInfo;
        Commands = commands;
    }
}

public class VMReplayPreviewList( string title = "" ) : ObservableCollectionEx<VMReplayPreview>
{
    public string Title { get; } = title;
}