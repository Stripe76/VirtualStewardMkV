namespace Framework.UI.Values;

public class RangedInt : BaseUnmanagedValue<int>
{
  public RangedInt( int minValue,int maxValue,string name,string title ) : base( minValue,maxValue,name,title )
  {
  }
}

public class RangedUInt : BaseUnmanagedValue<uint>
{
  public RangedUInt( uint minValue,uint maxValue,string name,string title ) : base( minValue,maxValue,name,title )
  {
  }
}

public class RangedFloat : BaseUnmanagedValue<float>
{
  public RangedFloat( float minValue,float maxValue,string name,string title ) : base( minValue,maxValue,name,title )
  {
  }
}