using System;
using System.ComponentModel;
using Framework.Bindables;
using ACConnection.Network.Packets.Protocol;
using Avalonia.Media;

namespace VirtualSteward.Features.Server.ViewModels;

public class VMServerDebugItem : BindableBase
{
  private int _nParam1;
  private int _nParam2;
  private string? _sParam1;
  private object? _oParam1;

  public IImmutableSolidColorBrush Color = Brushes.Black;

  public int nParam1
  {
    get => _nParam1;
    set { SetProperty( ref _nParam1,value ); }
  }
  public int nParam2
  {
    get => _nParam2;
    set { SetProperty( ref _nParam2,value ); }
  }
  public string? sParam1
  {
    get => _sParam1;
    set { SetProperty( ref _sParam1,value ); }
  }
  public object? oParam1
  {
    get => _oParam1;
    set { SetProperty( ref _oParam1,value ); }
  }
}

public class VMServerDebug
{
  public BindingList<VMServerDebugItem> Tracings1 = [];
  public BindingList<VMServerDebugItem> Tracings2 = [];

  public void AddCarlist( /*GUICarInfoList carList*/ )
  {
    Tracings1.Add(
      new VMServerDebugItem( )
      {
        //sParam1 = $"{DateTime.Now:mm:ss.fff} - {carList.Name}",
      } );
  }

  public void AddIncomingPacket( INetworkPacket packet )
  {
    AddPacket( packet,"<-",Brushes.DarkBlue );
  }
  public void AddOutgoingPacket( INetworkPacket packet )
  {
    AddPacket( packet,"->",Brushes.ForestGreen );
  }

  private void AddPacket( INetworkPacket packet,string sPrefix,IImmutableSolidColorBrush color )
  {
    if(
        packet.GetID( ) == ACServerProtocol.PingUpdate ||
        packet.GetID( ) == ACServerProtocol.PositionUpdate ||
        packet.GetID( ) == ACServerProtocol.CarConnect ||
        packet.GetID( ) == ACServerProtocol.SunAngleUpdate ||
        packet.GetID( ) == ACServerProtocol.LobbyCheck ||
        packet.GetID( ) == ACServerProtocol.SessionRequest ||
        packet.GetID( ) == ACServerProtocol.MandatoryPitUpdate ||
        packet.GetID( ) == ACServerProtocol.TyreCompoundChange ||
        packet.GetID( ) == ACServerProtocol.Extended ||
        packet.GetID( ) == ACServerProtocol.LapCompleted ||
        packet.GetID( ) == ACServerProtocol.CurrentSessionUpdate ||
        packet.GetID( ) == ACServerProtocol.P2PUpdate ||
        packet.GetID( ) == ACServerProtocol.SectorSplit ||
        packet.GetID( ) == ACServerProtocol.DamageUpdate ||
        packet.GetID( ) == ACServerProtocol.RaceStart ||
        packet.GetID( ) == ACServerProtocol.PingPong
      )
    {
      bool bFound = false;
      for( int i = 0; i < Tracings2.Count; i++ )
      {
        VMServerDebugItem item = Tracings2[i];
        if( item != null )
        {
          if( item.oParam1 is INetworkPacket n && n.GetID( ) == packet.GetID( ) && item.sParam1.StartsWith( sPrefix ) )
          {
            bFound = true;

            Tracings2[i].sParam1 = $"{sPrefix} {DateTime.Now:mm:ss.fff} - {packet.GetID( )}";
            Tracings2[i].oParam1 = packet;
          }
        }
      }
      if( !bFound )
        Tracings2.Add(
          new VMServerDebugItem( )
          {
            sParam1 = $"{sPrefix} {DateTime.Now:mm:ss.fff} - {packet.GetID( )}",
            oParam1 = packet,
            Color = color
          } );
    }
    else
    {
      Tracings1.Add(
        new VMServerDebugItem( )
        {
          sParam1 = $"{sPrefix} {DateTime.Now:mm:ss.fff} - {packet.GetID( )}",
          oParam1 = packet,
          Color = color
        } );
    }
  }
}
