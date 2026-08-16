using Framework.UI.Values;

namespace Framework.UI.Configurations;

public class Configuration : UIBase
{
  private List<object>? _values = null;

  public string Name { get; }
  public double Width { get; init; }
  public object? Header { get; }
  
  public List<object> Values => _values ??= PopulateValues( );

  public Configuration( string name,object? header = null,double width = 340f )
  {
    Name = name;
    Header = header;
    Width = width;

    IsExpanded = true;
  }

  private List<object> PopulateValues( )
  {
    List<object>? values = [];

    // var propValue = obj.GetType( )?.GetProperty( prop )?.GetValue( obj )?.ToString( );
    var type = GetType( );
    foreach( var field in type.GetFields( ) )
    {
      var value = field.GetValue( this );
      if( value != null )
        values.Add( value );
    }
    return values;
  }

  public void Serialize( Settings.Settings settings )
  {
    string section = Name;
    foreach( var value in Values )
    {
      if( value is BaseValue<int> saveInt )
        settings.Save( section,saveInt.Name,saveInt.Value );
      else if( value is BaseValue<int> saveUInt )
        settings.Save( section,saveUInt.Name,saveUInt.Value );
      else if( value is BaseValue<float> saveFloat )
        settings.Save( section,saveFloat.Name,saveFloat.Value );
      else if( value is BaseValue<bool> saveBool )
        settings.Save( section,saveBool.Name,saveBool.Value );
      else if( value is BaseValue<string> saveString )
        settings.Save( section,saveString.Name,saveString.Value ?? "" );
    }
  }
  public void Deserialize( Settings.Settings settings )
  {
    string section = Name;
    foreach( var value in Values )
    {
      if( value is BaseValue<int> loadInt )
        loadInt.Value = settings.LoadInt( section,loadInt.Name,loadInt.Value );
      else if( value is BaseValue<int> loadUInt )
        loadUInt.Value = settings.LoadInt( section,loadUInt.Name,loadUInt.Value );
      else if( value is BaseValue<float> loadFloat )
        loadFloat.Value = settings.LoadFloat( section,loadFloat.Name,loadFloat.Value );
      else if( value is BaseValue<bool> loadBool )
        loadBool.Value = settings.LoadBool( section,loadBool.Name,loadBool.Value );
      else if( value is BaseValue<string> loadString )
        loadString.Value = settings.LoadString( section,loadString.Name,loadString.Value );
    }
  }
}

public class ConfigurationList : List<Configuration>
{
  
}
