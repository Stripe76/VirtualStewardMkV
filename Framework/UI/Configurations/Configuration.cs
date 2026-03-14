using Framework.IniFiles;
using Framework.UI.Values;

namespace Framework.UI.Configurations;

public class Configuration : UIBase
{
  private List<object>? _values = null;

  public string Name { get; }
  public double Width { get; init; } = 340.0f;
  public object? Header { get; }
  
  public List<object> Values => _values ??= PopulateValues( );

  public Configuration( string name,object? header = null )
  {
    Name = name;
    Header = header;

    IsExpanded = true;
  }

  private List<object> PopulateValues( )
  {
    List<object>? values = [];

    // var propValue = obj.GetType( )?.GetProperty( prop )?.GetValue( obj )?.ToString( );
    var type = GetType( );
    foreach( var property in type.GetFields( ) )
    {
      var value = property.GetValue( this );
      if( value != null )
        values.Add( value );
    }
    return values;
  }

  public void Serialize( IniFile iniFile )
  {
    string section = Name;
    foreach( var value in Values )
    {
      if( value is BaseValue<int> saveInt )
        iniFile.WriteValue( saveInt.Name,section,saveInt.Value );
      else if( value is BaseValue<int> saveUInt )
        iniFile.WriteValue( saveUInt.Name,section,saveUInt.Value );
      else if( value is BaseValue<float> saveFloat )
        iniFile.WriteValue( saveFloat.Name,section,saveFloat.Value );
      else if( value is BaseValue<bool> saveBool )
        iniFile.WriteValue( saveBool.Name,section,saveBool.Value );
    }
  }
  public void Deserialize( IniFile iniFile )
  {
    string section = Name;
    foreach( var value in Values )
    {
      if( value is BaseValue<int> loadInt )
        loadInt.Value = iniFile.GetIntValue( loadInt.Name,section,loadInt.Value );
      else if( value is BaseValue<int> loadUInt )
        loadUInt.Value = iniFile.GetIntValue( loadUInt.Name,section,loadUInt.Value );
      else if( value is BaseValue<float> loadFloat )
        loadFloat.Value = iniFile.GetFloatValue( loadFloat.Name,section,loadFloat.Value );
      else if( value is BaseValue<bool> loadBool )
        loadBool.Value = iniFile.GetBoolValue( loadBool.Name,section,loadBool.Value );
    }
  }
}
