using System.Text;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class DriverInfoUpdate : INetworkPacket
{
  public class DriverInfo
  {
    public byte SessionId;
    public string Name = "";
  }

  public DriverInfo[]? DriverInfos;
  //public required IEnumerable<IEntryCar<IClient>> ConnectedCars { get; init; }

  public ACServerProtocol GetID( ) { return ACServerProtocol.DriverInfoUpdate; }

  public override string ToString( )
  {
    StringBuilder sb = new StringBuilder( );
    if( DriverInfos != null )
    {
      foreach( DriverInfo info in DriverInfos )
        sb.AppendLine( $"SessionId: {info.SessionId} - Name: {info.Name}" );
    }
    return sb.ToString( );
  }

  public void FromReader( PacketReader reader )
  {
    int nCount = reader.Read<byte>( );

    DriverInfos = new DriverInfo[nCount];
    for( int i = 0; i < nCount; i++ )
    {
      DriverInfos[i] = new DriverInfo
      {
        SessionId = reader.Read<byte>( ),
        Name = reader.ReadUTF32String( ),
      };
    }
  }
  public void ToWriter( ref PacketWriter writer )
  {
    throw new NotImplementedException( );
  }

  /*
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.DriverInfoUpdate );
    writer.Write( (byte)ConnectedCars.Count( ) );

    foreach( var car in ConnectedCars )
    {
      writer.Write( car.SessionId );
      writer.WriteUTF32String( car.AiControlled ? car.AiName : car.Client?.Name );
    }
  }
  */
}
