using System;
using System.Collections.Generic;
using Avalonia.Platform.Storage;
using Framework.Bindables;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Tracklines.ViewModels;

namespace VirtualSteward.Features.ReplayExport.Exports;

public abstract class BaseExport( string name,string title )
{
    public string Name { get; } = name;
    public string Title { get; } = title;

    public virtual bool ShowTimelineExport { get; } = true;

    public virtual string FilesExtension { get; } = "";
    public virtual List<FilePickerFileType> FilesFilter { get; } = [];

    public abstract void ExportReplay( string filename,VMReplay replay,VMTracklineFile? tracklineFile,IList<VMPlayer> players,uint startFrame,uint endFrame,IProgress<float>? progress = null );
}

public class BaseExportList : ObservableCollectionEx<BaseExport>
{
    
}