using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Framework.Bindables;
using Framework.Helpers;
using Framework.UI.Values;
using VirtualSteward.Datasources;
using VirtualSteward.Datasources.ViewModels;
using VirtualSteward.Features.PlayersList.ViewModels;
using VirtualSteward.Features.ReplayLoading.ViewModels;
using VirtualSteward.Features.Tracklines.ViewModels;

namespace VirtualSteward.Features.ReplayExport.Exports.SVGFile;

public class SVGFileExport( ) : BaseExport( "SVGFile","As SVG map file" )
{
  public BaseBool _includeLegend = new BaseBool( "InlcudeLegend","Include legend" );

  public object? Parameter => _includeLegend;

  public override bool ShowTimelineExport { get; } = false;

  public override string FilesExtension { get; } = ".svg";
  public override List<FilePickerFileType> FilesFilter { get; } = [new FilePickerFileType( "SVG files" ) { Patterns = ["*.svg"] },new FilePickerFileType( "All files" ) { Patterns = ["*.*"] }];

  public override void ExportReplay( string filename,VMReplay replay,VMTracklineFile? tracklineFile,IList<VMPlayer> players,uint startFrame,uint endFrame,IProgress<float>? progress = null )
  {
    int nXOffset = 0,nYOffset = 0,nWidth = 0,nHeight = 0,nMaxWidth = 24000,nMaxHeight = 24000;

    foreach( VMPlayer p in players )
    {
      foreach( VMPlayerLap lap in p.Laps )
      {
        if( lap.IsSelected )
        {
          PointCollection arPoints = [];
          if( ExportPlayerLine( p,lap,arPoints,nMaxWidth,nMaxHeight ) )
          {
            foreach( Point pt in arPoints )
            {
              if( pt.X < nXOffset )
                nXOffset = (int)pt.X;
              if( pt.Y < nYOffset )
                nYOffset = (int)pt.Y;

              if( -nXOffset + pt.X > nWidth )
                nWidth = (int)pt.X + -nXOffset;
              if( -nYOffset + pt.Y > nHeight )
                nHeight = (int)pt.Y + -nYOffset;
            }
          }
        }
      }
    }
    nXOffset = -nXOffset;
    nYOffset = -nYOffset;

    nXOffset += nXOffset / 20;
    nYOffset += nYOffset / 20;

    nWidth += nWidth / 20;
    nHeight += nHeight / 20;

    if( _includeLegend )
    {
      nXOffset += 250;
      nWidth += 250;
    }
    if( nWidth > 0 && nHeight > 0 )
    {
      XNamespace aw = "http://www.w3.org/2000/svg";
      XElement svg = new XElement( aw + "svg",new XAttribute( "version","1.1" ),new XAttribute( "width",nWidth ),new XAttribute( "height",nHeight ) );

      StringBuilder sbStyle = new( );
      sbStyle.AppendLine( ".small { font: bold 11px sans-serif; }" );
      sbStyle.AppendLine( ".braking { stroke:white; stroke-width:1; stroke-dasharray: 2; fill:none; }" );
      sbStyle.AppendLine( $"\t/* Track lines */" );
      sbStyle.AppendLine( $"\t.lineTrack {{ stroke:#{String.Format( "{0:x2}{1:x2}{2:x2}",0,0,0 )}; stroke-width:1; fill:none; }}" );

      foreach( VMPlayer p in players )
      {
        //if( p.IsSelected )
        {
          sbStyle.AppendLine( $"\t/* {p.PlayerInfo.PlayerName} */" );
          sbStyle.AppendLine(
            $"\t.line{p.PlayerID} {{ stroke:#{p.LineStyle.Color.Color.R:x2}{p.LineStyle.Color.Color.G:x2}{p.LineStyle.Color.Color.B:x2}; stroke-width:1; fill:none; }}" );
        }
      }
      svg.Add( new XElement( aw + "style",sbStyle.ToString( ) ) );

      if( _includeLegend )
      {
        int nPlayerX = 50;
        int nPlayerY = 20;
        foreach( VMPlayer p in players )
        {
          //if( p.IsSelected )
          {
            XElement pPlayerLine = new XElement( aw + "line",
              new XAttribute( "x1",5 ),
              new XAttribute( "y1",nPlayerY - 5 ),
              new XAttribute( "x2",nPlayerX - 10 ),
              new XAttribute( "y2",nPlayerY - 5 ),
              new XAttribute( "style","stroke-width:4;" ),
              new XAttribute( "class",$"line{p.PlayerID}" )
            );
            svg.Add( pPlayerLine );

            XElement pPlayer = new XElement( aw + "text",
              new XAttribute( "class","small" ),
              new XAttribute( "x",nPlayerX ),
              new XAttribute( "y",nPlayerY ),
              p.PlayerInfo.PlayerName
            );
            svg.Add( pPlayer );

            nPlayerY += 20;
          }
        }
      }
      {
        PointCollection arPoints = [];
        if( ExportTrackLines( tracklineFile,arPoints,nMaxWidth,nMaxHeight ) )
        {
          StringBuilder sb = new StringBuilder( );
          foreach( Point pt in arPoints )
          {
            sb.Append( $"{nXOffset + pt.X:0.00}".Replace( ',','.' ) );
            sb.Append( ',' );
            sb.Append( $"{nYOffset + pt.Y:0.00} ".Replace( ',','.' ) );
          }
          XElement poly = new XElement( aw + "polyline",
            new XAttribute( "class",$"lineTrack" ),
            new XAttribute( "stroke-width","1" ),
            new XAttribute( "fill","none" )
          );

          poly.Add( new XAttribute( "points",sb.ToString( ) ) );

          svg.Add( poly );
        }
      }
      for( int i = players.Count - 1; i >= 0; i-- )
      {
        VMPlayer p = players[i];

        //if( p != null && p.IsSelected )
        {
          foreach( VMPlayerLap lap in p.Laps )
          {
            if( lap.IsSelected )
            {
              IImmutableSolidColorBrush lapColor = lap.LineStyle.Color;
              {
                PointCollection arPoints = [];
                if( ExportPlayerLine( p,lap,arPoints,nMaxWidth,nMaxHeight ) )
                {
                  StringBuilder sb = new StringBuilder( );
                  foreach( Point pt in arPoints )
                  {
                    sb.Append( $"{nXOffset + pt.X:0.00}".Replace( ',','.' ) );
                    sb.Append( ',' );
                    sb.Append( $"{nYOffset + pt.Y:0.00} ".Replace( ',','.' ) );
                  }
                  XElement poly = new XElement( aw + "polyline",
                    new XAttribute( "class",$"line{p.PlayerID}" ),
                    new XAttribute( "stroke-width","1" ),
                    new XAttribute( "fill","none" )
                  );

                  if( !Equals( lapColor,p.LineStyle.Color ) )
                    poly.Add( new XAttribute( "style","stroke:#" + $"{lapColor.Color.R:x2}{lapColor.Color.G:x2}{lapColor.Color.B:x2}" ) );
                  poly.Add( new XAttribute( "points",sb.ToString( ) ) );

                  svg.Add( poly );
                }
              }
              List<PointCollection> arLines = [];
              if( ExportPlayerBrakingLine( p,lap,arLines,nMaxWidth,nMaxHeight ) )
              {
                foreach( PointCollection arPoints in arLines )
                {
                  StringBuilder sb = new StringBuilder( );
                  foreach( Point pt in arPoints )
                  {
                    sb.Append( $"{nXOffset + pt.X:0.00}".Replace( ',','.' ) );
                    sb.Append( ',' );
                    sb.Append( $"{nYOffset + pt.Y:0.00} ".Replace( ',','.' ) );
                  }
                  XElement poly = new XElement( aw + "polyline",
                    new XAttribute( "class","braking" ),
                    new XAttribute( "points",sb.ToString( ) )
                  );

                  svg.Add( poly );
                }
              }
            }
          }
        }
      }
      System.IO.File.WriteAllText( filename,svg.ToString( ) );
    }
  }

  private static bool ExportTrackLines( VMTracklineFile? tracklineFile,PointCollection points,int nMaxWidth,int nMaxHeight,int steps = 1 )
  {
    if( tracklineFile != null )
    {
      (VMTrackline? leftSide,VMTrackline? rightSide) = Tracklines.Tracklines.CreateTrackLimits( tracklineFile );
      if( leftSide != null )
      {
        var data = leftSide.Data;

        int start = 0;
        int finish = data.Count;

        for( int i = start; i < finish; i += steps )
        {
          Point pt = new( data[i].Position.X,data[i].Position.Y );

          if( Math.Abs( pt.X ) < nMaxWidth && Math.Abs( pt.Y ) < nMaxHeight )
            points.Add( pt );
        }
      }
      if( rightSide != null )
      {
        var data = rightSide.Data;

        int start = 0;
        int finish = data.Count;

        for( int i = start; i < finish; i += steps )
        {
          Point pt = new( data[i].Position.X,data[i].Position.Y );

          if( Math.Abs( pt.X ) < nMaxWidth && Math.Abs( pt.Y ) < nMaxHeight )
            points.Add( pt );
        }
      }
      return true;
    }
    return false;
  }

  private static bool ExportPlayerLine( VMPlayer player,VMPlayerLap lap,PointCollection arPoints,int nMaxWidth,int nMaxHeight,bool bCloseCap = false,uint steps = 1 )
  {
    uint start = lap.StartFrame;
    uint finish = lap.EndFrame;

    CarDatasource datasource = player.Datasource;
    if( finish - start < 24000 )
    {
      VMCarPosition carPos = new VMCarPosition( );
      VMCarPosition nextPos = new VMCarPosition( );

      for( uint i = start; i < finish; i += steps )
      {
        datasource.GetPositionAndRotation( i,carPos );

        bool bAdd = true;
        if( i < finish - 1 )
        {
          datasource.GetPositionAndRotation( i+1,nextPos );

          double dDistance = Mathematics.DistanceSqrd( carPos.Position,nextPos.Position );
          if( dDistance > 60*60 )
            bAdd = false;
        }
        Point pt = new Point( carPos.Position.X,carPos.Position.Y );

        if( bAdd && Math.Abs( pt.X ) < nMaxWidth && Math.Abs( pt.Y ) < nMaxHeight )
          arPoints.Add( pt );
      }
      if( bCloseCap && arPoints.Count > 0 )
        arPoints.Add( arPoints[0] );
    }
    return true;
  }
  private static bool ExportPlayerBrakingLine( VMPlayer player,VMPlayerLap lap,List<PointCollection> arLines,int nMaxWidth,int nMaxHeight,uint steps = 1 )
  {
    //if( vCar != null && arLines != null )
    {
      uint start = lap.StartFrame;
      uint finish = lap.EndFrame;

      CarDatasource datasource = player.Datasource;
      if( finish - start < 24000 )
      {
        PointCollection? arBreaking = null;

        VMCarPosition carPos = new VMCarPosition( );
        VMCarPosition nextPos = new VMCarPosition( );

        for( uint i = start; i < finish; i += steps )
        {
          datasource.GetPositionAndRotation( i,carPos );

          bool bAdd = true;
          if( i < finish - 1 )
          {
            datasource.GetPositionAndRotation( i+1,nextPos );

            double dDistance = Mathematics.DistanceSqrd( carPos.Position,nextPos.Position );
            if( dDistance > 60*60 )
              bAdd = false;
          }
          Point pt = new Point( carPos.Position.X,carPos.Position.Y );

          if( bAdd && Math.Abs( pt.X ) < nMaxWidth && Math.Abs( pt.Y ) < nMaxHeight )
          {
            if( carPos.BrakePedal > 0 )
            {
              arBreaking ??= new PointCollection( );
              arBreaking.Add( pt );
            }
            else
            {
              if( arBreaking is { Count: < 1000 } )
              {
                arLines.Add( arBreaking );
              }
              arBreaking = null;
            }
          }
        }
      }
      return true;
    }
    return false;
  }
}