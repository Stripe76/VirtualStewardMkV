using ACLibrary.Data;
using ACLibrary.Replays;
//using Framework.Helpers;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Framework.Helpers;
using VirtualSteward.Datasources.ViewModels;
//using VirtualSteward.Features.DataTemplates.Classes;
using VirtualSteward.ViewModels;

namespace VirtualSteward.Datasources;

public abstract class CarDatasource
{
  public abstract int Length { get; }

  public virtual string GetFieldValue( uint frame,string field )
  {
    throw new NotImplementedException();
  }
  public virtual void SetFieldValue( uint frame,string field,string value )
  {
    throw new NotImplementedException( );
  }

  public virtual ReplayTail? GetTailData( )
  {
    return null;
  }
  public virtual ReplayCarLap[]? GetCarLaps( )
  {
    return null;
  }
  public virtual ReplayCarData? GetSaveData( uint frame )
  {
    return null;
  }

  public abstract VMCarData? GetCarData( uint frame );
  public abstract VMServerData? GetServerData( uint frame,VMServerData? serverData = null );
  public abstract VMCarPosition? GetPositionAndRotation( uint frame );

  public abstract uint GetLapTime( uint frame );

  public abstract VMCarData? GetCurrentCarData( );
}

public class EmptyDatasource : CarDatasource
{
  public override int Length => 0;

  public override VMCarData? GetCarData( uint frame )
  {
    return null;
  }
  public override VMServerData? GetServerData( uint frame,VMServerData? serverData = null )
  {
    return null;
  }
  public override VMCarPosition? GetPositionAndRotation( uint frame )
  {
    return null;
  }

  public override VMCarData? GetCurrentCarData( )
  {
    return null;
  }

  public override uint GetLapTime( uint frame )
  {
    return 0;
  }
}

public class ReplayFileDatasource : CarDatasource
{
  private readonly VMCarData _carData = new ( );
  private readonly VMServerData _localServerData = new ( );
  private readonly VMCarPosition _carPosition = new ( );

  private readonly ReplayCarData[] _replayData;
  private readonly ReplayCarLap[] _carLaps;
  private readonly ReplayTail _carTail;

  public override int Length => _replayData.Length;

  public ReplayFileDatasource( ReplayCar replayCar,ReplayTail tailData )
  {
    _replayData = replayCar.Data;
    _carLaps = replayCar.Laps;
    _carTail = tailData;
  }

  public override string GetFieldValue( uint frame,string field )
  {
    ACCarFrame replayData = _replayData[MapFrame( frame )].Frame;
    object? obj = ResolvePath( replayData,field );
    if( obj != null )
      return obj.ToString( )?.Replace(',','.')??"";
    return "";
  }
  public override void SetFieldValue( uint frame,string field,string value )
  {
    //ACCarFrame replayData = _replayData[MapFrame( frame )].Frame;

    SetValueByPath( ref _replayData[MapFrame( frame )].Frame,field,value );
    /*
    object? obj = ResolvePath( replayData,field );
    if( obj != null )
    {
      object? convertedValue = Convert.ChangeType( value,obj.GetType( ) );

      obj = convertedValue;
    }
    */
  }

  public override ReplayTail? GetTailData( )
  {
    return _carTail;
  }
  public override ReplayCarLap[]? GetCarLaps( )
  {
    return _carLaps;
  }
  public override ReplayCarData? GetSaveData( uint frame )
  {
    return _replayData[MapFrame( frame )];
  }

  public override VMCarData? GetCarData( uint frame )
  {
    ACCarFrame replayData = _replayData[MapFrame( frame )].Frame;

    _carData.Position.X = replayData.BodyTranslation.X;
    _carData.Position.Y = replayData.BodyTranslation.Z;
    _carData.Position.Z = replayData.BodyTranslation.Y;

    _carData.Rotation.X = (float)replayData.BodyOrientation.X;
    _carData.Rotation.Y = (float)replayData.BodyOrientation.Y;
    _carData.Rotation.Z = (float)replayData.BodyOrientation.Z;

    _carData.SteeringWheel = (float)replayData.Steer;

    _carData.GasPedal = replayData.Gas / 255f;
    _carData.BrakePedal = replayData.Brake / 255f;

    _carData.RPMs = (float)replayData.EngineRpm;
    _carData.Gear = replayData.Gear;
    _carData.Fuel = replayData.Fuel;

    return _carData;
  }
  public override VMServerData? GetServerData( uint frame,VMServerData? serverData = null )
  {
    ACCarFrame replayData = _replayData[MapFrame( frame )].Frame;

    serverData ??= _localServerData;// = new VMServerData( );

    serverData.Position.X = replayData.BodyTranslation.X;
    serverData.Position.Y = replayData.BodyTranslation.Y;
    serverData.Position.Z = replayData.BodyTranslation.Z;

    serverData.Rotation.X = (float)replayData.BodyOrientation.X;
    serverData.Rotation.Y = (float)replayData.BodyOrientation.Y;
    serverData.Rotation.Z = (float)replayData.BodyOrientation.Z;

    serverData.Velocity.X = (float)replayData.Velocity.X;
    serverData.Velocity.Y = (float)replayData.Velocity.Y;
    serverData.Velocity.Z = (float)replayData.Velocity.Z;

    serverData.SteeringWheel = (float)replayData.Steer;
    //serverData.WheelsAngle = NormalizeAngle( (float)replayData.BodyOrientation.X ) - NormalizeAngle( (float)replayData.TyreOrientation[0].X );

    float body = Mathematics.Degrees( (float)replayData.BodyOrientation.X );
    float wheel = Mathematics.Degrees( (float)replayData.SusOrientationFL.X );

    if( Math.Sign( body ) == Math.Sign( wheel ) )
    {
      serverData.WheelsAngle = body-wheel;
    }
    else
    {
      if( body < 0 )
      {
        if( body > -90 )
          serverData.WheelsAngle = -(wheel-body);
        else
          serverData.WheelsAngle = (180+body) + (180-wheel);
      }
      else
      {
        if( body > 90 )
          serverData.WheelsAngle = -((180 - body) + (180 + wheel));
        else
          serverData.WheelsAngle = body - wheel;
      }
    }
    serverData.FLAngular = (float)replayData.WheelAngularSpeedFL;
    serverData.FRAngular = (float)replayData.WheelAngularSpeedFR;
    serverData.RLAngular = (float)replayData.WheelAngularSpeedRL;
    serverData.RRAngular = (float)replayData.WheelAngularSpeedRR;

    serverData.GasPedal = replayData.Gas;
    serverData.BrakePedal = replayData.Brake;

    serverData.RPMs = (ushort)replayData.EngineRpm;
    serverData.Gear = replayData.Gear;
    serverData.Fuel = replayData.Fuel;

    serverData.Flags = 0;
    serverData.Flags |= ((replayData.Status & 0x04) != 0) ? VMServerData.StatusFlags.LightsOn : 0; // Lights
    serverData.Flags |= (replayData.Brake > 0) ? VMServerData.StatusFlags.BrakeLightsOn : 0; // Brake
#if DEBUG
    serverData.BodyOrientation = replayData.BodyOrientation;
    //serverData.SusOrientation = replayData.SusOrientation;
    //serverData.TyreOrientation = replayData.TyreOrientation;
#endif
    return serverData;
  }
  public override VMCarPosition? GetPositionAndRotation( uint frame )
  {
    ACCarFrame replayData = _replayData[MapFrame( frame )].Frame;

    _carPosition.Position.X = replayData.BodyTranslation.X;
    _carPosition.Position.Y = replayData.BodyTranslation.Z;
    _carPosition.Position.Z = replayData.BodyTranslation.Y;

    _carPosition.Rotation.X = (float)replayData.BodyOrientation.X;
    _carPosition.Rotation.Y = (float)replayData.BodyOrientation.Y;
    _carPosition.Rotation.Z = (float)replayData.BodyOrientation.Z;

    _carPosition.BrakePedal = replayData.Brake;

    _carPosition.LapTime = replayData.LapTime;
    _carPosition.LastLapTime = replayData.LastLap;

    return _carPosition;
  }

  public override uint GetLapTime( uint frame )
  {
    ACCarFrame replayData = _replayData[MapFrame( frame )].Frame;

    return replayData.LapTime;
  }

  public override VMCarData? GetCurrentCarData( )
  {
    return _carData;
  }

  private long MapFrame( uint frame )
  {
    return Math.Clamp( frame,0,_replayData.Length-1 );
  }

  private static object? ResolvePath( object obj,string path )
  {
    if( obj == null )
      return null;

    var parts = path.Split('.');
    foreach( var part in parts )
    {
      var match = Regex.Match(part, @"(\w+)(\[(\d+)\])?");

      if( !match.Success )
        return null;

      var propName = match.Groups[1].Value;
      if( propName == "Degree" )
      {
        if( obj is System.Half h )
          return Mathematics.Degrees( (float)h );
        if( obj is System.Single s )
          return Mathematics.Degrees( (float)s );
      }
      else
      {
        var indexer = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : (int?)null;

        var field = obj.GetType().GetField(propName);
        if( field != null )
        {
          obj = field.GetValue( obj );

          if( indexer.HasValue && obj is IEnumerable enumerable )
          {
            obj = enumerable.Cast<object>( ).ElementAtOrDefault( indexer.Value );
          }
        }
        else
        {
          var prop = obj.GetType( ).GetProperty( propName );
          if( prop == null )
            return null;

          obj = prop.GetValue( obj );

          //if( indexer.HasValue && obj is IEnumerable enumerable )
          if( indexer.HasValue && obj is Array array )
          {
            obj = array.Cast<object>( ).ElementAtOrDefault( indexer.Value );
          }
        }
      }
    }
    return obj;
  }

  private static void SetValueByPath( ref ACCarFrame frame,string path,string rawValue )
  {
    var parts = path.Split('.');

#if DEBUG
    if( path == "Lights" )
    {
      int c = 0;
    }
#endif
    if( parts.Length == 2 )
    {
      var field = typeof( ACCarFrame ).GetField( parts[0] );
      if( field != null )
      {
        var fieldField = field.FieldType.GetField( parts[1] );
        if( fieldField != null )
        {
          object? value = ConvertValue( rawValue,fieldField.FieldType );
          if( value != null )
          {
            /*
            if( path == "BodyTranslation.X" )
            {
              float offset = (float)value - frame.BodyTranslation.X;

              frame.TyreTranslationFL.X += offset;
              frame.TyreTranslationFR.X += offset;
              frame.TyreTranslationRL.X += offset;
              frame.TyreTranslationRR.X += offset;
            }
            else if( path == "BodyTranslation.Y" )
            {
              float offset = (float)value - frame.BodyTranslation.Y;

              frame.TyreTranslationFL.Y += offset;
              frame.TyreTranslationFR.Y += offset;
              frame.TyreTranslationRL.Y += offset;
              frame.TyreTranslationRR.Y += offset;
            }
            else if( path == "BodyTranslation.Z" )
            {
              float offset = (float)value - frame.BodyTranslation.Z;

              frame.TyreTranslationFL.Z += offset;
              frame.TyreTranslationFR.Z += offset;
              frame.TyreTranslationRL.Z += offset;
              frame.TyreTranslationRR.Z += offset;
            }
            */
            if( field.FieldType.IsValueType )
            {
              var tr = TypedReference.MakeTypedReference( frame,[field] );
              //var trParent = System.TypedReference.MakeTypedReference( frame,[] );

              fieldField.SetValueDirect( tr,value );
              field.SetValueDirect( __makeref( frame ),TypedReference.ToObject( tr ) );
              //field.SetValue( frame,TypedReference.ToObject( tr ) );
            }
            else
            {
              fieldField.SetValue( field,value );
            }
          }
        }
      }
      else
      {
        var fieldProp = typeof( ACCarFrame ).GetProperty( parts[0] );
        if( fieldProp != null )
        {
          object? value = ConvertValue( rawValue,fieldProp.PropertyType );
          if( value != null )
          {
            fieldProp.SetValue( field,value );
          }
        }
      }
    }
    else
    {
      var field = typeof( ACCarFrame ).GetField( path );
      if( field != null )
      {
        object? value = ConvertValue( rawValue,field.FieldType );
        if( value != null )
        {
          if( field.FieldType.IsValueType )
            field.SetValueDirect( __makeref(frame),value );
          else
            field.SetValue( frame,value );
        }
      }
      else
      {
        var prop = typeof( ACCarFrame ).GetProperty( path );
        if( prop != null )
        {
          object? value = ConvertValue( rawValue,prop.PropertyType );
          if( value != null )
            prop.SetValue( frame,value );
        }
      }
    }
  }

  private static object? ConvertValue( string raw,Type targetType )
  {
    if( targetType == typeof( string ) )
      return raw;

    if( targetType.IsEnum )
      return Enum.Parse( targetType,raw );

    if( targetType == typeof( int ) )
      return int.Parse( raw );

    if( targetType == typeof( Half ) )
      return Half.Parse( raw,CultureInfo.InvariantCulture );

    if( targetType == typeof( float ) || targetType == typeof( Single ) )
      return Single.Parse( raw,CultureInfo.InvariantCulture );

    if( targetType == typeof( double ) )
      return double.Parse( raw,CultureInfo.InvariantCulture );

    if( targetType == typeof( bool ) )
      return bool.Parse( raw );

    // Per tipi complessi puoi estendere qui
    return Convert.ChangeType( raw,targetType );
  }

}
