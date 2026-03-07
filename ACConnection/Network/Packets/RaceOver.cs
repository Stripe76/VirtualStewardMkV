using ACConnection.Model;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class RaceOver : INetworkPacket
{
  public bool PickupMode;
  public bool IsRace;
  public required Dictionary<byte, EntryCarResult> Results;

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.RaceOver;
  }

  public void FromReader( PacketReader reader )
  {
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.RaceOver );

    foreach( var (sessionId, result) in Results ) // .OrderBy(r => IsRace ? r.Value.TotalTime : r.Value.BestLap)
    {
      writer.Write( sessionId );
      writer.Write( IsRace ? result.TotalTime : result.BestLap );
      writer.Write( (ushort)result.NumLaps );
    }
    writer.Write( PickupMode );
  }
}