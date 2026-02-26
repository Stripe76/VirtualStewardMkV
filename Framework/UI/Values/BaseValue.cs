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

  public Action<T?>? ValueChanged = null;
}

public class BaseUnmanagedValue<T> : BaseValue<T> where T : unmanaged
{
  public override string? TextValue
  {
    get => $"{string.Format( "{0:" + Format + "}",Value )} {Unit}";
  }

  public T Minimum { get; protected set; }
  public T Maximum { get; protected set; }

  public string Unit { get; set; } = string.Empty;
  public string Format { get; set; } = "0";

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