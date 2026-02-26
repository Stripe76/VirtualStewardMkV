namespace Framework.UI.Values;

public class MappedValue<T> : BaseUnmanagedValue<T> where T : unmanaged
{
  private readonly IList<T> _values;

  private int _inputValue = 0;
  private SortedList<int,string>? _displayTexts = null;

  public int InputValue 
  {
    get => _inputValue;
    set
    {
      if( SetProperty( ref _inputValue,value ) )
      {
        Value = _values[_inputValue];
      }
    }
  }
  public int ValuesNumber 
  {
    get => _values.Count-1;
  }

  public override string? TextValue 
  {
    get
    {
      if( _displayTexts != null && _displayTexts.ContainsKey( _inputValue ) )
        return _displayTexts[_inputValue];
      return base.TextValue;
    }
  }

  public SortedList<int,string> DisplayTexts
  {
    set => _displayTexts = value;
  }

  public MappedValue( string name,string title,IList<T> values ) : base( name,title )
  {
    _values = values;

    Value = _values[0];
  }
}

public class MappedValueInt( string name,string title,IList<int> values ) : MappedValue<int>( name,title,values )
{

}

public class MappedValueUInt( string name,string title,IList<uint> values ) : MappedValue<uint>( name,title,values )
{

}
