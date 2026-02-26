using System.Diagnostics;
using Avalonia;
using VirtualSteward.Features.TrackMap.Controls;

namespace VirtualSteward.Features.TrackMap.EditingTools;

public class MapMoveEdit : EditingTool
{
  private MapDisplay _mapDisplay;

  private Point ptStartOffet = new ( 0, 0 );
  private Point ptStartCapture = new ( 0, 0 );

  //public GUIActivePlayer? ActivePlayer = null;

  public MapMoveEdit( MapDisplay mapDisplay )
  {
    _mapDisplay = mapDisplay;
  }

  public override bool LeftMouseDown( Point screenPos,Point trackPos )
  {
    ptStartOffet = _mapDisplay.Offset;
    ptStartCapture = screenPos;

    return true;
  }
  public override bool LeftMouseMove( Point screenPos,Point trackPos )
  {
    double x = ptStartOffet.X + (screenPos.X - ptStartCapture.X);
    double y = ptStartOffet.Y + (screenPos.Y - ptStartCapture.Y);

    _mapDisplay.Offset = new Point( x,y );
    /*
    if( ActivePlayer != null && ActivePlayer.FollowPlayer != null )
      ActivePlayer.FollowPlayer.IsFollow = false;
    */
    return base.LeftMouseMove( screenPos,trackPos );
  }

  public override bool RightMouseDown( Point screenPos,Point trackPos )
  {
    double x = ptStartOffet.X - (ptStartCapture.X - screenPos.X);
    double y = ptStartOffet.Y - (ptStartCapture.Y - screenPos.Y);

    _mapDisplay.SelectedPoint = new Point( x,y );

    return base.RightMouseDown( screenPos,trackPos );
  }
}
