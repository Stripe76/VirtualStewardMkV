using System.Collections.Generic;

using Framework.UI.Values;
using Framework.UI.Configurations;

using VirtualSteward.ACNetwork.Shared;

namespace VirtualSteward.Features.Server.Configurations;

public class CMServerOptions( ACServerSettings settings ) : Configuration( "SERVER_OPTIONS","Server options" )
{
  public readonly RangedInt ServerFrequency = new( 1,100,nameof( ServerFrequency ),"Frequency" )
  {
    Unit = "hz",
    Description = "Same as replay file or multiple is suggested",
    ValueChanged = ( value ) => { settings.ServerFrequency = value; },
    Value = 18,
  };
  public readonly MappedValueUInt MapFrequency = new( nameof( MapFrequency ),"Map update",[0,1,2,3,4,5,6,7,8,9,10,12,14,18,19] )
  {
    IsVisible = false,
    Unit = "hz",
    Description = "Update frequency of the map in Virtual Steward, doesn't affect the server",
    DisplayTexts = new SortedList<int,string>( ) { { 0,"Off" }, { 14,"As server" } },
    Separator = true,
    ValueChanged = ( value ) =>
    {
      /*
      if( value == 14 )
        server.MapFrequency = (uint)(1000 / settings.ServerFrequency);
      else if( value > 0 )
        server.MapFrequency = 1000 / value;
      else
        server.MapFrequency = 0;
      */
    },
    InputValue = 1,
  };
  public readonly RangedFloat TrackGrip = new( 85,100,nameof( TrackGrip ),"Track grip" )
  {
    Format = "0.0",
    Unit = "%",
    ValueChanged = ( value ) => { settings.TrackGrip = value / 100.0f; },
    Value = 100,
  };
  public readonly RangedFloat FuelRate = new( 0,400,nameof( FuelRate ),"Fuel rate" )
  {
    Format = "0",
    Unit = "%",
    ValueChanged = ( value ) => { settings.FuelRate = value / 100.0f; },
    Value = 0,
  };
  public readonly RangedFloat TiresWear = new( 0,400,nameof( TiresWear ),"Tires wear" )
  {
    Format = "0",
    Unit = "%",
    Separator = true,
    ValueChanged = ( value ) => { settings.TiresWear = value / 100.0f; },
    Value = 0,
  };
  public readonly BaseSwitchBool AllowWrongWay = new( nameof( AllowWrongWay ),"Wrong way driving is enabled","Wrong way driving is disabled" )
  {
    Description = "Wrong way driving",
    ValueChanged = ( value ) => { settings.AllowWrongWay = value; },
    Value = true,
  };
  public readonly BaseSwitchBool EanbleCollisions = new( nameof( EanbleCollisions ),"Collisions are enabled","Collisions are disabled" )
  {
    Description = "Cars colisions",
    Separator = true,
    ValueChanged = ( value ) => { settings.DisableCollisions = !value; },
    Value = true,
  };
  public readonly BaseSwitchBool TiresBlankets = new( nameof( TiresBlankets ),"Tires blanket enabled","Tires blanket disabled" )
  {
    Description = "Tires blanket",
    ValueChanged = ( value ) => { settings.TiresBlanket = value; },
    Value = true,
    Separator = true,
  };
  public readonly BaseSwitchBool RecalcVelocities = new( nameof( RecalcVelocities ),"Recalculate cars velocities","Don't recalculate cars velocities" )
  {
    Description = "Apparently AC replay files store wrong speed data for remote online cars, this is meant to fix that and make cars smoother on turns",
    ValueChanged = ( value ) => { settings.RecalcVelocities = value; },
    Value = true,
  };
  public readonly BaseSwitchBool ExtendedCarPhysics = new( nameof( ExtendedCarPhysics ),"Extended car physics","Extended car physics is disabled" )
  {
    Description = "Never tested, don't know if it actually works",
    ValueChanged = ( value ) => { settings.ExtendedCarPhysic = value; },
    Value = false,
  };
  public readonly BaseSwitchBool ExtendedTrackPhysics = new( nameof( ExtendedTrackPhysics ),"Extended track physics","Extended track physics is disabled" )
  {
    Description = "Never tested, don't know if it actually works",
    ValueChanged = ( value ) => { settings.ExtendedTrackPhysic = value; },
    Value = false,
    Separator = true,
  };
}