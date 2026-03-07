using System.Text;
using System.ComponentModel;
using ACConnection.Network.Packets;
using ACConnection.Network.Packets.Handshake;

namespace ACConnection.Network;

/*
public delegate void EventHandler<TSender, TArgs>( TSender sender,TArgs args ) where TArgs : EventArgs;
public delegate void EventHandlerIn<TSender, TArg>( TSender sender,in TArg args ) where TArg : struct;

public class WelcomeMessageSentEventArgs : EventArgs
{
  public required string WelcomeMessage { get; init; }
  public required string ExtraOptions { get; init; }
  public required string EncodedWelcomeMessage { get; init; }
}

public class WelcomeMessageSendingEventArgs : EventArgs
{
  public required StringBuilder Builder { get; init; }
}

public class CSPServerExtraOptionsSendingEventArgs : EventArgs
{
  public required StringBuilder Builder { get; init; }
}

public class HandshakeAcceptedEventArgs : EventArgs
{
  public required HandshakeResponse HandshakeResponse { get; init; }
}

public class ClientAuditEventArgs : EventArgs
{
  public KickReason Reason { get; init; }
  public string? ReasonStr { get; init; }
}

public class ChatEventArgs : CancelEventArgs
{
  public string Message { get; }

  public ChatEventArgs( string message )
  {
    Message = message;
  }
}

public class ChatMessageEventArgs : EventArgs
{
  public ChatMessage ChatMessage { get; init; }
}

public class LapCompletedEventArgs : EventArgs
{
  public LapCompletedServer Packet { get; }

  public LapCompletedEventArgs( LapCompletedServer packet )
  {
    Packet = packet;
  }
}

public class CarListResponseSendingEventArgs : EventArgs
{
  public CarListResponse Packet { get; }

  public CarListResponseSendingEventArgs( CarListResponse packet )
  {
    Packet = packet;
  }
}
*/