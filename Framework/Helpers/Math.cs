using System.Numerics;

namespace Framework.Helpers;

public class Mathematics
{
  public static float Degrees( float Radians )
  {
    return Radians * (180f / MathF.PI);
  }
  public static double Degrees( double Radians )
  {
    return Radians * (180f / Math.PI);
  }

  public static float Radians( float Degrees )
  {
    float f = Degrees * (MathF.PI / 180f);
    return f;
  }
  public static double Radians( double Degrees )
  {
    return Degrees * (MathF.PI / 180f);
  }

  public static Vector3 RotateX( Vector3 vector,float angle )
  {
    return new Vector3( vector.X * MathF.Cos( angle ) + vector.Y * MathF.Sin( angle ),
                        vector.Y * MathF.Cos( angle ) - vector.X * MathF.Sin( angle ),
                        vector.Z );
  }

  public static Vector3 GetPitchAndYaw( Vector3 A,Vector3 B )
  {
    // Calcola le differenze delle coordinate
    double deltaX = B.X - A.X;
    double deltaY = B.Y - A.Y;
    double deltaZ = B.Z - A.Z;

    // Calcola yaw (angolo in piano orizzontale)
    double yaw = Math.Atan2( deltaY,deltaX );

    // Calcola pitch (angolo in piano verticale)
    double distanzaXY = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    double pitch = Math.Atan2( deltaZ,distanzaXY );

    return new Vector3( (float)yaw,(float)pitch,0 );
  }

  public static int GetQuadrant( double radians )
  {
    double sin = Math.Sin( radians );
    double cos = Math.Cos( radians );

    if( sin >= 0 && cos >= 0 )
      return 1;
    if( sin >= 0 && cos <= 0 )
      return 2;
    if( sin <= 0 && cos <= 0 )
      return 3;
    if( sin <= 0 && cos >= 0 )
      return 4;
    return 1;
  }
  public static int GetQuadrant( float aX,float aY,float bX,float bY )
  {
    float dX = bX - aX;
    float dY = bY - aY;

    if( dY <= 0 && dX >= 0 )
      return 1;
    if( dY <= 0 && dX <= 0 )
      return 2;
    if( dY >= 0 && dX <= 0 )
      return 3;
    if( dY >= 0 && dX >= 0 )
      return 4;
    return 1;
  }

  public static double Distance( Vector3 a,Vector3 b )
  {
    return (a - b).Length( );
  }
  public static double Distance( double aX,double aY,double bX,double bY )
  {
    double d = Math.Sqrt( (bX - aX) * (bX - aX) + (bY - aY) * (bY - aY) );
    return d;
  }

  public static double Direction( double aX,double aY,double bX,double bY )
  {
    return -Math.Atan2( bY - aY,aX - bX );
  }

  public static float GetPositiveAngleDifference( float a,float b )
  {
    if( (a > MathF.PI / 2 && b < -MathF.PI / 2) || (a < -MathF.PI / 2 && b > MathF.PI / 2) )
      return MathF.Abs( a ) - MathF.Abs( b );
    return a - b;
  }
  public static double GetPositiveAngleDifference( double a,double b )
  {
    if( (a > Math.PI / 2 && b < -Math.PI / 2) || (a < -Math.PI / 2 && b > Math.PI / 2) )
      return Math.Abs( a ) - Math.Abs( b );
    return a - b;
  }

  public static float NormalizeAngle( float a )
  {
    if( a < -MathF.PI )
      return a + MathF.PI + MathF.PI;
    if( a > MathF.PI )
      return a - MathF.PI;
    return a;
  }
  public static double NormalizeAngle( double a )
  {
    if( a < -Math.PI )
      return a + Math.PI + Math.PI;
    if( a > Math.PI )
      return a - Math.PI;
    return a;
  }

  public static string LapTimeToString( int nTime )
  {
    return String.Format( "{0:00}:{1:00}:{2:000}",nTime / 60000,nTime / 1000 % 60,nTime % 1000 );
  }

  public static float PI2 = MathF.PI/2;

  public static double ZeroToOneLerp( int value,int total )
  {
    if( total == 0 )
      return 1;
    return ZeroToOneLerp( value / total );
  }
  public static double ZeroToZeroLerp( int value,int total )
  {
    if( total == 0 )
      return 1;
    return ZeroToZeroLerp( value / total );
  }

  public static double ZeroToOneLerp( double lerp )
  {
    return Math.Cos( Math.PI + Math.PI * lerp ) / 2 + 0.5f;
  }
  public static double ZeroToOneFastLerp( double lerp )
  {
    return Math.Sin( (Math.PI / 2) * lerp );
  }
  public static double ZeroToZeroLerp( double lerp )
  {
    //return Math.Cos( 2 * Math.PI * lerp - Math.PI ) / 2 + 0.5f;
    //maps the progress between -π/2 to π/2

    double progress = double.Lerp( -Math.PI,Math.PI,lerp );
    //returns a value between -1 and 1
    progress = Math.Cos( progress );
    //scale the sine value between 0 and 1.
    progress = (progress / 2f) + .5f;

    return progress;
  }
  public static double MinusOnePlusOne( double lerp )
  {
    //return Math.Cos( 2 * Math.PI * lerp - Math.PI ) / 2 + 0.5f;
    //maps the progress between -π/2 to π/2

    double progress = double.Lerp( -Math.PI,Math.PI,lerp );
    //returns a value between -1 and 1
    progress = Math.Sin( progress );
    //scale the sine value between 0 and 1.

    return progress;
  }

  public static float OneToZeroLerpF( float lerp )
  {
    return 1 - MathF.Sin( PI2 * lerp );
  }
}
