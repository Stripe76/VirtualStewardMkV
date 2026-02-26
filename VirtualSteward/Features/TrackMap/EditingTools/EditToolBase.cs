using Avalonia;

namespace VirtualSteward.Features.TrackMap.EditingTools;

public class EditingTool
{
  public virtual void MouseMove( Point screenPos,Point trackPos )
  {

  }

  public virtual bool LeftMouseDown( Point screenPos, Point trackPos )
  {
    return false;
  }
  public virtual void LeftMouseUp( Point screenPos,Point trackPos )
  {

  }
  public virtual bool LeftMouseMove( Point screenPos,Point trackPos )
  {
    return false;
  }

  public virtual bool RightMouseDown( Point screenPos,Point trackPos )
  {
    return false;
  }
  public virtual void RightMouseUp( Point screenPos,Point trackPos )
  {

  }
  public virtual bool RightMouseMove( Point screenPos,Point trackPos )
  {
    return false;
  }
}
