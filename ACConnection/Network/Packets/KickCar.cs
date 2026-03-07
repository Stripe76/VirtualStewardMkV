using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class KickCar : INetworkPacket
{
  public byte SessionId;
  public KickReason Reason;

  public ACServerProtocol GetID( ) { return ACServerProtocol.KickCar; }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            Reason: {Reason}

            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    Reason = reader.Read<KickReason>( );
  }

  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.KickCar );
    writer.Write( SessionId );
    writer.Write( Reason );
  }
}

public enum KickReason : byte
{
  VoteKicked,
  VoteBanned,
  VoteBlacklisted,
  ChecksumFailed,
  Kicked
}
