using Avalonia.Controls.Templates;
using Framework.UI;
using VirtualSteward.Features.ProgressBar.ViewModel;

namespace VirtualSteward.Features.ProgressBar;

public class ProgressBar(DataTemplates templates) : Feature(templates)
{
    public override void AddDataTemplates(DataTemplates templates)
    {
        templates.Add(new FuncDataTemplate<VMProgress>((_,_) => new Controls.ProgressBar() ));
    }
}