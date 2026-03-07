using Avalonia.Controls.Templates;
using Framework.UI;

using VirtualSteward.Features.PlayersList.ViewModels;

namespace VirtualSteward.Features.PlayersList;

public partial class PlayersList( DataTemplates templates ) : Feature(templates)
{
    public override Feature AddDataTemplates(DataTemplates templates)
    {
        templates.Add( new FuncDataTemplate<VMPlayer>( (_,_) => new Controls.Player() ) );
        templates.Add( new FuncDataTemplate<VMPlayerLapList>( (_,_) => new Controls.PlayerLaps() ) );
        templates.Add( new FuncDataTemplate<VMPlayerInfoEditing>( (_,_) => new Controls.PlayerInfoEditing() ) );
    
        return this;
    }
}