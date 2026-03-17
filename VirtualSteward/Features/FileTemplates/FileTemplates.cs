using System.IO;
using Avalonia.Controls.Templates;
using Framework.UI;
using VirtualSteward.Classes;
using VirtualSteward.Features.FileTemplates.Classes;

namespace VirtualSteward.Features.FileTemplates;

public class FileTemplates : Feature
{
    public FileTemplateList TemplateFiles { get; set; } = [];

    public FileTemplates( DataTemplates templates, FilesManager filesManager ) : base( templates )
    {
        var files = filesManager.GetFileTemplateFiles(  );
        if( files != null )
        {
            foreach( string file in files )
            {
                TemplateFiles.Add( new FileTemplate( file,$"With {Path.GetFileName( file )} fields" ) );
            }
        }
    }

    public override Feature AddDataTemplates( DataTemplates templates )
    {
        //templates.Add( new FuncDataTemplate<FileTemplateValue>( ( _,_ ) => new Controls.FileTemplateInput( ) ) );
        templates.Add( new FuncDataTemplate<FileTemplateValue>( ( _,_ ) => new Framework.UI.Inputs.MultiListInput( ) ) );

        return this;
    }
}