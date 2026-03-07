using System.Text;

namespace ACConnection.Utils;

// https://github.com/ac-custom-shaders-patch/acc-extension-config/wiki/Misc-%E2%80%93-Server-extra-options
public class CSPServerExtraOptions
{
  public string WelcomeMessage { get; set; } = "";
  public string ExtraOptions { get; set; } = "";
  public string? CSPExtraOptions { get; set; }

  public CSPServerExtraOptions( )
  {
  }

  public string GenerateWelcomeMessage( )
  {
    var sb = new StringBuilder();
    sb.Append( WelcomeMessage );

    var welcomeMessage = sb.ToString();

    sb.Clear( );

    sb.AppendLine( ExtraOptions );
    sb.AppendLine( CSPExtraOptions );

    var extraOptions = sb.ToString();

    var encodedWelcomeMessage = CSPServerExtraOptionsParser.Encode(welcomeMessage, extraOptions);

    return encodedWelcomeMessage;
  }
}