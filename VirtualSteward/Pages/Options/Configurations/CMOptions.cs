using Framework.UI.Configurations;
using Framework.UI.Values;
using VirtualSteward.Classes;

namespace VirtualSteward.Pages.Options.Configurations;

public class CMOptions( Options options,FilesManager filesManager ) : Configuration( "SETTINGS","Settings" )
{
    public readonly FolderValue ACFolder = new FolderValue( filesManager.ACFolder,nameof( ACFolder ),"AC folder" )
    {
        MinWidth = 700,
        ValueChanged = (value) =>
        {
            filesManager.ACFolder = value ?? filesManager.ACFolder;

            options.CheckSettings( );
        }
    };

    public readonly FolderValue ReplaysFolder = new FolderValue( filesManager.ReplaysFolder,nameof( ReplaysFolder ),"Replays folder" )
    {
        MinWidth = 700,
        ValueChanged = ( value ) =>
        {
            filesManager.ReplaysFolder = value ?? filesManager.ReplaysFolder;

            options.CheckSettings( );
        }
    };
}