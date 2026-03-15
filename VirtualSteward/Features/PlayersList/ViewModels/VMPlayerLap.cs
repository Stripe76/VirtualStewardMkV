using Avalonia.Media;

using Framework.Bindables;
using Framework.UI;

using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.PlayersList.ViewModels;

public class VMPlayerLap( uint lapNumber,uint startFrame,uint endFrame,VMMapLineStyle lineStyle ) : UIItem
{
    public uint LapNumber { get; set; } = lapNumber;
    public uint LapTime { get; set; } = 0;

    public string LapName
    { 
        get => LapNumber.ToString( ); 
    }
    public string LapTimeText
    {
        get => $"{LapTime / 60000:00}:{LapTime / 1000 % 60:00}:{LapTime % 1000:000}";
    }
    public string LapTimeFullText
    {
        get => $"Lap {LapNumber}: {LapTime / 60000:00}:{LapTime / 1000 % 60:00}:{LapTime % 1000:000}";
    }

    public VMMapLineStyle LineStyle => lineStyle;

    public uint StartFrame = startFrame;
    public uint EndFrame = endFrame;

    public VMMapLineNewList? Lines = null;
        
    public override string ToString()
    {
        return LapName;
    }
}

public class VMPlayerLapList( bool multiSelect = false ) : MultiList<VMPlayerLap>( multiSelect,false )
{

}
