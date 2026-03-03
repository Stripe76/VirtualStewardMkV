using Framework.UI.Values;
using VirtualSteward.Features.ReplayExport.Exports;

namespace VirtualSteward.Features.ReplayExport.Values;

public class ExporterValue( BaseExportList exporters ) : BaseValue<BaseExport>(exporters.Count>0?exporters[0]:null,"Exporter","")
{
    public BaseExportList Items => exporters;
}