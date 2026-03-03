using System.IO;
using ACLibrary.Replays;
using Framework.UI;

namespace VirtualSteward.Features.ReplayLoading.ViewModels;

public class VMReplay : UIBase
{
    private string _fileName = "";

    public string FileFullPath = string.Empty;

    public string Weather = string.Empty;

    public string CarID = string.Empty;
    public string TrackID = string.Empty;
    public string TrackVariantID = string.Empty;

    public double ReplayFrequency = 33f;

    public VMTrackObjects TrackObjects;

    public readonly int TailDataRecords = 0;
    public readonly int TailDataVersion = 0;

    public bool IsLoaded { get; } = false;

    public string FileName
    {
        get => _fileName;
        set => SetProperty( ref _fileName,value );
    }

    public VMReplay( )
    {
        IsLoaded = false;
        TrackObjects = new VMTrackObjects( );
    }
    public VMReplay( Replay replay,TrackObject[] trackObjects,uint trackObjectsNumber )
    {
        IsLoaded = true;
        
        FileName = Path.GetFileNameWithoutExtension( replay.FileFullPath );

        FileFullPath = replay.FileFullPath;

        Weather = replay.Weather;

        CarID = (replay.Cars.Length > 0) ? replay.Cars[0].CarID : "";
        TrackID = replay.TrackID;
        TrackVariantID = replay.TrackVariantID;

        ReplayFrequency = replay.ReplayFrequency;

        TrackObjects = new VMTrackObjects( trackObjects,trackObjectsNumber );

        TailDataRecords = replay.TailDataRecords;
        TailDataVersion = replay.TailDataVersion;
    }
}