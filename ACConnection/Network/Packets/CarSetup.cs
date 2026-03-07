using System.Text;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class CarSetup : INetworkPacket
{
  public bool Something;
  public int Count;
  public Dictionary<string, float> Setup = [];

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.CarSetup;
  }

  public override string ToString( )
  {
    StringBuilder sb = new ( );
    foreach( var pair in Setup )
    {
      sb.AppendLine( $"\t{pair.Key}: {pair.Value}" );
    }
    return $"""
            Boh: {Something}
            Count: {Count}
            {sb}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    Something = reader.Read<bool>( );
    Count = reader.Read<byte>( );

    for( int i = 0; i < Count; i++ )
    {
      string sKey = reader.ReadUTF8String( );
      if( !Setup.ContainsKey( sKey ) )
        Setup.Add( sKey,reader.Read<float>( ) );
      else
      {
        int c = 0;
      }
    }
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.CarSetup );
    writer.Write( true );
    writer.Write( (byte)Setup.Count );
    foreach( var (name, val) in Setup )
    {
      writer.WriteUTF8String( name );
      writer.Write( (float)val );
    }
  }
}