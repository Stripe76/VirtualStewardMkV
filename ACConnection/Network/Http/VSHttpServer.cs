using System.Net;
using System.Text;

public class VSHttpServer
{
  public string ServerAddress = "127.0.0.1";
  public int Port = 8080;

  private HttpListener? _listener;

  private string _sInfoResponse = "";
  private string _sEntryListResponse = "";

  public VSHttpServer( string server, int httpPort )
  {
    ServerAddress = server;
    Port = httpPort;
  }

  public void Start( Serilog.ILogger? logger = null )
  {
    _listener = new HttpListener( );
    _listener.Prefixes.Add( $"http://{ServerAddress}:" + Port.ToString( ) + "/" );

    _listener.Start( );

    Receive( );
  }
  public void Stop( )
  {
    _listener?.Stop( );
  }

  public void SetInfoResponse( string sInfo )
  {
    _sInfoResponse = sInfo;
  }
  public void SetEntryListResponse( string sEntryList )
  {
    _sEntryListResponse = sEntryList;
  }

  private void Receive( )
  {
    _listener.BeginGetContext( new AsyncCallback( ListenerCallback ),_listener );
  }

  private void ListenerCallback( IAsyncResult result )
  {
    try
    {
      if( _listener.IsListening )
      {
        var context = _listener.EndGetContext( result );
        var request = context.Request;

        if( request.Url.ToString( ).EndsWith( "/INFO" ) )
        {
          Receive( );

          var response = context.Response;
          var infoResponse =  Encoding.UTF8.GetBytes( _sInfoResponse );
          
          response.StatusCode = (int)HttpStatusCode.OK;
          response.ContentEncoding = Encoding.UTF8;
          response.ContentType = "application/json; charset=utf-8";
          //response.ContentLength64 = infoResponse.Length;
          response.SendChunked = true;

          response.OutputStream.Write( infoResponse,0,infoResponse.Length );
          response.OutputStream.Close( );
        }
        if( request.Url.ToString( ).Contains( "/JSON|" ) )
        {
          Receive( );

          var response = context.Response;
          var entryResponse =  Encoding.UTF8.GetBytes( _sEntryListResponse );

          response.StatusCode = (int)HttpStatusCode.OK;
          response.ContentEncoding = Encoding.UTF8;
          response.ContentType = "application/json; charset=utf-8";
          //response.ContentLength64 = entryResponse.Length;
          response.SendChunked = true;

          response.OutputStream.Write( entryResponse,0,entryResponse.Length );
          response.OutputStream.Close( );
        }
      }
      Receive( );
    }
    catch( Exception ex ) 
    {
    }
  }
}