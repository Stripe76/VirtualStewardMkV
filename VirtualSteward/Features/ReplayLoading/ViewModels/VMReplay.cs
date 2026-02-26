using System.IO;
using ACLibrary.Replays;
using Framework.UI;

namespace VirtualSteward.Features.ReplayLoading.ViewModels;

public class VMReplay : UIBase
{
    private readonly bool _isLoaded = false;

    private string _fileName = "";

    public string FileFullPath = string.Empty;

    public string Weather = string.Empty;

    public string CarID = string.Empty;
    public string TrackID = string.Empty;
    public string TrackVariantID = string.Empty;

    public double ReplayFrequency = 0f;
    public uint TrackObjectsNumber = 0;

    public int TailDataRecords = 0;
    public int TailDataVersion = 0;

    public bool IsLoaded
    {
        get => _isLoaded;
    }

    public string FileName
    {
        get => _fileName;
        set => SetProperty( ref _fileName,value );
    }

    public VMReplay( )
    {
        _isLoaded = false;
    }
    public VMReplay( Replay replay )
    {
        _isLoaded = true;
        _fileName = Path.GetFileNameWithoutExtension( replay.FileFullPath );

        FileFullPath = replay.FileFullPath;

        Weather = replay.Weather;

        CarID = (replay.Cars.Length > 0) ? replay.Cars[0].CarID : "";
        TrackID = replay.TrackID;
        TrackVariantID = replay.TrackVariantID;

        ReplayFrequency = replay.ReplayFrequency;
        TrackObjectsNumber = replay.TrackObjectsNumber;

        TailDataRecords = replay.TailDataRecords;
        TailDataVersion = replay.TailDataVersion;
    }
}