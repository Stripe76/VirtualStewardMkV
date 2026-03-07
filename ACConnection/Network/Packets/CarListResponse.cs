using ACConnection.Model;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class CarListResponse : INetworkPacket
{
  public int PageIndex;
  public int EntryCarsCount;
  public List<EntryCar> EntryCars = [];

  public ACServerProtocol GetID( ) { return ACServerProtocol.CarList; }

  public override string ToString( )
  {
    string s = $"""
               PageIndex: {PageIndex}
               EntryCarsCount: {EntryCarsCount}           

               """;

    foreach( EntryCar car in EntryCars )
      s = s + "\r\n" + car.ToString( );

    return s;
  }

  public void FromReader( PacketReader reader )
  {
    PageIndex = reader.Read<byte>( );
    EntryCarsCount = reader.Read<byte>( );

    for( int i = 0; i < EntryCarsCount; i++ )
    {
      EntryCar ec = new EntryCar( );
      ec.SessionId = reader.Read<byte>( );
      ec.Model = reader.ReadUTF8String( );
      ec.Skin = reader.ReadUTF8String( );
      ec.Name = reader.ReadUTF8String( );
      ec.Team = reader.ReadUTF8String( );
      ec.NationCode = reader.ReadUTF8String( );
      ec.IsSpectator = reader.Read<bool>( );
      ec.Damage = reader.Read<DamageZoneLevel>( );

      EntryCars.Add( ec );
    }
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.CarList );
    writer.Write( (byte)PageIndex );
    writer.Write( (byte)EntryCarsCount );

    foreach( var car in EntryCars )
    {
      writer.Write( car.SessionId );
      writer.WriteUTF8String( car.Model );
      writer.WriteUTF8String( car.Skin );
      writer.WriteUTF8String( car.Name );
      writer.WriteUTF8String( car.Team );
      writer.WriteUTF8String( car.NationCode );
      writer.Write( car.IsSpectator );
      writer.Write( car.Damage );
    }
  }
}

public class EntryCar : INetworkPacket
{
  public byte SessionId;
  public string Model;
  public string Skin;
  public string Name;
  public string Team;
  public string NationCode;
  public bool IsSpectator;
  public DamageZoneLevel Damage;

  public ACServerProtocol GetID( ) { return ACServerProtocol.CarList; }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            Model: {Model}
            Skin: {Skin}
            Name: {Name}
            Team: {Team}
            NationCode: {NationCode}
            IsSpectator: {IsSpectator}
            Damage: {Damage}

            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    Model = reader.ReadUTF8String( );
    Skin = reader.ReadUTF8String( );
    Name = reader.ReadUTF8String( );
    Team = reader.ReadUTF8String( );
    NationCode = reader.ReadUTF8String( );
    IsSpectator = reader.Read<bool>( );
    Damage = reader.Read<DamageZoneLevel>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    throw new NotImplementedException( );
  }
}
