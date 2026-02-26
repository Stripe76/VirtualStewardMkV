using System;
using System.Collections.Generic;

namespace VirtualSteward.Classes;

internal class IsWorking : IDisposable
{
    private readonly Tasks _task = Tasks.None;

    private static readonly List<Tasks> _tasks = [];

    public enum Tasks
    {
        None,
        ReplayFileLoading,
        ReplaysListLoading,
        TracklinesLoading,
        TrackCheckpointsLoading,
        CarsInfosLoading,
        TracksInfosLoading,
        TrafficPresetsLoading,
        TrafficCreation,
        SelectedCarsListUpdating,
    }

    public IsWorking( Tasks task, bool throwException = true )
    {
        lock( _tasks )
        {
            if( _tasks.Contains( task ) )
            {
                if( throwException )
                    throw new TaskAlreadyRunning( task );
            }
            else
            {
                _tasks.Add( _task = task );
            }
        }
    }

    public void Dispose( )
    {
        if( _task != Tasks.None )
        {
            lock( _tasks )
                _tasks.Remove( _task );
        }
    }

    public static bool Now( Tasks task )
    { 
        return _tasks.Contains( task ); 
    }
}

internal class TaskAlreadyRunning( IsWorking.Tasks task ) : Exception
{
    public IsWorking.Tasks Task = task;
}