using Framework.Bindables;
using System.Collections;

namespace Framework.UI.Values;

public class BaseValue<T>( T? value,string name,string title ) : UIBase
{
  private T? _value = value;

  public T? Value 
  {
    get => _value;
    set
    {
      if( SetProperty( ref _value,value ) )
      {
        OnPropertyChanged( nameof( TextValue ) );

        ValueChanged?.Invoke( _value );
      }
    }
  }

  public virtual string? TextValue 
  {
    get => _value?.ToString( );
  }

  public string Name { get; } = name;
  public string Title { get; } = title;
  public string Description { get; set; } = title;

  public bool Separator { get; set; } = false;
  public double MinWidth { get; } = 200;

  public Action<T?>? ValueChanged = null;
}

public class BaseUnmanagedValue<T> : BaseValue<T> where T : unmanaged
{
  public override string? TextValue
  {
    get => $"{string.Format( "{0:" + Format + "}",(FormatValue!=null)?(FormatValue(Value)):(Value) )} {Unit}";
  }

  public T Minimum { get; protected set; }
  public T Maximum { get; protected set; }

  public string Unit { get; set; } = string.Empty;
  public string Format { get; set; } = "0";
  public Func<T,string>? FormatValue { get; set; }

  public BaseUnmanagedValue( string name,string title ) : base( default,name,title )
  {
    Description = title;
  }
  public BaseUnmanagedValue( T minValue,T maxValue,string name,string title ) : base( default,name,title )
  {
    Description = title;

    Minimum = minValue;
    Maximum = maxValue;

    if( Comparer.Default.Compare( Value,maxValue ) < 0 )
      Value = minValue;
  }
}


public class BaseInt( string name,string title ) : BaseUnmanagedValue<int>( int.MinValue,int.MaxValue,name,title )
{
}

public class BaseBool( string name,string title ) : BaseUnmanagedValue<bool>( false,true,name,title )
{
}

public class BaseSwitchBool( string name,string titleOn,string titleOff ) : BaseUnmanagedValue<bool>( false,true,name,titleOn )
{
  public string TitleOff => titleOff;
}

public class BaseThreeStateBool( string name,string title ) : BaseValue<bool?>( null,name,title )
{
  public bool IsThreeState => true;
}