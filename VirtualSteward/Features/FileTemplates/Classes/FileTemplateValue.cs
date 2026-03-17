using Framework.UI.Values;

namespace VirtualSteward.Features.FileTemplates.Classes;

public class FileTemplateValue( FileTemplateList fileTemplates,string name,string title ) : BaseValue<FileTemplate>( fileTemplates.Count>0?fileTemplates[0]:null,name,title )
{
  private readonly FileTemplateList _fileTemplates = fileTemplates;

  public FileTemplateList Items => _fileTemplates;
}
