using System.IO;
using Avalonia.Controls.Templates;

using Framework.UI;

using VirtualSteward.Classes;
using VirtualSteward.Pages.Options.Configurations;

namespace VirtualSteward.Pages.Options;

public class Options : StateFeature
{
    public string Icon { get; } = "\xf2cc";

    public CMOptions Settings { get; }

    public Options( State state,DataTemplates templates,string header,FilesManager filesManager ) : base( state,templates,header )
    {
        AddConfiguration( Settings = new CMOptions( this,filesManager ) { Width = 800 } );
    }

    public bool CheckSettings( )
    {
        bool errors = false,warnings = false;
        if( !File.Exists( Path.Combine( Settings.ACFolder.Value ?? "","acs.exe" ) ) )
        {
            errors = true;
            Settings.ACFolder.Error = "AC not found";
        }
        else
        {
            Settings.ACFolder.Error = null;
        }
        if( !Directory.Exists( Settings.ReplaysFolder.Value ?? "" ) )
        {
            warnings = true;
            Settings.ReplaysFolder.Warning = "Folder not found";
        }
        else
        {
            Settings.ReplaysFolder.Warning = null;
        }
        Error = errors;
        Warning = !errors && warnings;
        
        return errors;
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        templates.Add( new FuncDataTemplate<Options>( ( _,_ ) => new Pages.Options( ) ) );

        return this;
    }
}