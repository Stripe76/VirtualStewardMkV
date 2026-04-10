using System;
using System.Collections.Generic;

using ACLibrary.Replays;
using Avalonia.Platform.Storage;
using VirtualSteward.Datasources;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Tracklines.ViewModels;

namespace VirtualSteward.Features.ReplayExport.Exports.ACReplay;

public class ACReplayExport( ) : BaseExport( "ACReplay","As AC replay file" )
{
  public override string FilesExtension { get; } = ".acreplay";
  public override List<FilePickerFileType> FilesFilter { get; } = [new FilePickerFileType( "AC replay files" ) { Patterns = ["*.acreplay"] },new FilePickerFileType( "All files" ) { Patterns = ["*.*"] }];

  public override void ExportReplay( string filename,VMReplay replay,VMTracklineFile? tracklineFile,IList<VMPlayer> players,uint startFrame,uint endFrame,IProgress<float>? progress = null )
  {
    Replay acReplay = new( CreateTrackObjects( replay.TrackObjects ),CreatePlayersCars( players,startFrame,endFrame ) )
    {
      Weather = replay.Weather,
      TrackID = replay.TrackID,
      TrackVariantID = replay.TrackVariantID,
      ReplayFrequency = replay.ReplayFrequency,
      TrackObjectsNumber = replay.TrackObjects.TrackObjectsNumber,
      TailDataRecords = replay.TailDataRecords,
      TailDataVersion = replay.TailDataVersion,
      TailData = CreateTailData( players ),
    };
    acReplay.SaveReplay( filename,progress );
  }

  private static TrackObject[] CreateTrackObjects( VMTrackObjects trackObjects )
  {
    int count = trackObjects.Length;
    TrackObject[] objects = new TrackObject[count];

    for( uint i = 0; i < count; i++ )
    {
      objects[i] = trackObjects.GetSaveData( i );
    }
    return objects;
  }
  private static ReplayCar[] CreatePlayersCars( IList<VMPlayer> players,uint startFrame,uint endFrame )
  {
    ReplayCar[] cars = new ReplayCar[players.Count];

    int count = players.Count;
    for( int i = 0; i < count; i++ )
    {
      VMPlayer player = players[i];
      CarDatasource datasource = player.Datasource;

      //if( datasources.TryGetValue( player,out CarDatasource? datasource ) )
      {
        cars[i] = new ReplayCar( GetReplayCarData( datasource,startFrame,endFrame ),GetReplayCarLaps( player ) )
        {
          CarID = player.PlayerInfo.CarInfo.CarID,
          CarSkinID = player.PlayerInfo.CarSkinInfo.SkinID,
          PlayerName = player.PlayerInfo.PlayerName,
          PlayerTeam = player.PlayerInfo.PlayerTeam,
          PlayerNation = player.PlayerInfo.PlayerNation,
          NumberOfWings = player.PlayerInfo.CarInfo.NumberOfWings,
        };
      }
    }

    return cars;
  }
  private static ReplayTail[] CreateTailData( IList<VMPlayer> players )
  {
    var tailData = new ReplayTail[players.Count];
    int count = players.Count;
    for( int i = 0; i < count; i++ )
    {
      tailData[i] = players[i].Datasource.GetTailData( );
    }
    return tailData;
  }
  private static ReplayCarLap[] GetReplayCarLaps( VMPlayer player )
  {
    return player.Datasource.GetCarLaps( );
  }
  private static ReplayCarData[] GetReplayCarData( CarDatasource datasource,uint startFrame,uint endFrame )
  {
    uint count = endFrame - startFrame;

    ReplayCarData[] data = new ReplayCarData[count];
    for( uint i = 0; i < count; i++ )
    {
      data[i] = datasource.GetSaveData( startFrame + i );
#if DEBUG
      unchecked
      {
        //data[i].Frame.Status = (ushort)(0x1 << (ushort)(i / 300));
      }
#endif
    }
    return data;
  }
}