using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct HandshakeRequest : INetworkPacket
{
  public ushort ClientVersion;
  public ulong Guid;
  public string Name;
  public string Team;
  public string Nation;
  public string RequestedCar;
  public string Password;
  public string? Features;
  public byte[]? SessionTicket;

  public ACServerProtocol GetID( ) { return ACServerProtocol.Handshake; }

  public override string ToString( )
  {
    return $"""
            ClientVersion: {ClientVersion}
            Guid: {Guid}
            Name: {Name}
            Team: {Team}                        
            Nation: {Nation}                        
            RequestedCar: {RequestedCar}                        
            Password: {Password}                        
            Features: {Features}                        
            """;
  }

  public void FromReader( PacketReader reader )
  {
    ClientVersion = reader.Read<ushort>( );
    Guid = ulong.Parse( reader.ReadUTF8String( ) );
    Name = reader.ReadUTF32String( );
    Team = reader.ReadUTF8String( );
    Nation = reader.ReadUTF8String( );
    RequestedCar = reader.ReadUTF8String( );
    Password = reader.ReadUTF8String( );

    if( reader.Buffer.Length > reader.ReadPosition + 2 )
    {
      Features = reader.ReadUTF8String( true );

      if( reader.Buffer.Length > reader.ReadPosition + 2 )
      {
        short ticketLength = reader.Read<short>();
        if( ticketLength == reader.Buffer.Length - reader.ReadPosition )
        {
          SessionTicket = new byte[ticketLength];
          reader.ReadBytes( SessionTicket );
        }
      }
    }
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.RequestNewConnection );
    writer.Write( ClientVersion );
    writer.WriteString( Guid.ToString( ),System.Text.Encoding.UTF8 );
    writer.WriteString( Name,System.Text.Encoding.UTF32 );
    writer.WriteString( Team,System.Text.Encoding.UTF8 );
    writer.WriteString( Nation,System.Text.Encoding.UTF8 );
    writer.WriteString( RequestedCar,System.Text.Encoding.UTF8 );
    writer.WriteString( Password,System.Text.Encoding.UTF8 );

    if( Features is not null )
      writer.WriteString( Features,System.Text.Encoding.UTF8,2 );

    if( SessionTicket is not null && SessionTicket.Length > 0 )
    {
      writer.Write( (short)SessionTicket.Length );
      writer.WriteBytes( SessionTicket );
    }
  }
}