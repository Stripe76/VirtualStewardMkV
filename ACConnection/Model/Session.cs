namespace ACConnection.Model;

public class Session
{
  public int Id { get; set; }
  public SessionType Type { get; set; }
  public virtual string? Name { get; set; }
  public virtual int Time { get; set; }
  public virtual int Laps { get; set; }

  public override string ToString( )
  {
    return $"""

              Id: {Id}
              Type: {Type}
              Name: {Name}
              Name: {Time}
              Laps: {Laps}
            """;
  }
}