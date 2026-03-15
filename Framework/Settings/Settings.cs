using Framework.Bindables;
using Framework.IniFiles;

namespace Framework.Settings;

public class Settings : BindableBase
{
  private readonly IniFile _iniFile;

  public Settings( string file )
  {
    _iniFile = new( file );
  }
  
  public void SaveFile( string? filename = null )
  { 
    if ( filename != null )
      _iniFile.SaveAs( filename );
    else
      _iniFile.Save(  );
  }

  public int LoadInt( string section,string name,int @default = 0 )
  {
    return _iniFile.GetIntValue( name,section,@default );
  }
  public bool LoadBool( string section,string name,bool @default = false )
  {
    return _iniFile.GetBoolValue( name,section,@default );
  }
  public float LoadFloat( string section,string name,float @default = 0 )
  {
    return _iniFile.GetFloatValue( name,section,@default );
  }
  public double LoadDouble( string section,string name,double @default = 0 )
  {
    return _iniFile.GetDoubleValue( name,section,@default );
  }
  public string? LoadString( string section,string name )
  {
    return _iniFile.GetValue( name,section );
  }

  public void Save( string section,string name,int value )
  {
    _iniFile.WriteValue( name,section,value );
  }
  public void Save( string section,string name,bool value )
  {
    _iniFile.WriteValue( name,section,value );
  }
  public void Save( string section,string name,float value )
  {
    _iniFile.WriteValue( name,section,value );
  }
  public void Save( string section,string name,double value )
  {
    _iniFile.WriteValue( name,section,value );
  }
  public void Save( string section,string name,string value )
  {
    _iniFile.WriteValue( name,section,value );
  }
}