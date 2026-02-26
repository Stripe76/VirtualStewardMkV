using Framework.UI.Values;

using VirtualSteward.Features.Tracklines.ViewModels;

namespace VirtualSteward.Features.Tracklines.Values;

public class TracklineFileValue(VMTracklineFileList files) : MultilistValue<VMTracklineFile>("TracklineFile","",files)
{
}