using System;
using System.Collections.Generic;
using System.IO;
using Framework.Bindables;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;

namespace VirtualSteward.Features.ReplayExport.Exports;

public abstract class BaseExport( string name,string title )
{
    public string Name { get; } = name;
    public string Title { get; } = title;
    
    public abstract void ExportReplay( string filename,VMReplay replay,IList<VMPlayer> players,uint startFrame,uint endFrame,IProgress<float>? progress = null );
}

public class BaseExportList : ObservableCollectionEx<BaseExport>
{
    
}