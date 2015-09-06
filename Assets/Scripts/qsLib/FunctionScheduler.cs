/*
Unity Quickstart Library (qsLib)
http://www.andreivictor.net/uqsl/
Last Update: 09/06/2015

Copyright 2015 Andrei O. Victor

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using UnityEngine;
using System.Collections.Generic;
using QsLib;


/// <summary>
/// This class lets you coordinate timed execution of functions. It lets you store a function along
/// with the delay is observes before it executes. It is only when the Start() method is called that
/// the delay will be considered. This is useful in simple cases of timing scenes or effects.
///
/// Usage:
/// 1. Instantiate this class (and set a different timer if you need to)
/// 2. Schedule (add) several functions to execute.
/// 3. Make sure to manually call Update() per frame.
/// 4. Call Start() in your code to begin the counting the time for timed execution.
/// </summary>
public class FunctionScheduler 
{
    // Constructor with posibility of changing the timer to use.
    public FunctionScheduler( Timer otherTimer = null )
    {
        m_timer = otherTimer != null ? otherTimer : Timers.Instance.Game;
    }

    // Schedule (add) a function delegate (function), to execute at time (time) from the
    // start of this class instance.
    public void Schedule( float time, ScheduledFunction.function function )
    {
        Schedule( new ScheduledFunction( time, function ) );
    }

    // Same as scheduled (add) function above but for putting in a full entry.
    public void Schedule( ScheduledFunction entry )
    {
        if( entry != null )
        {
            m_actions.Add( entry );
        }
    }

    // Call to start to activate this scheduler to begin counting for timed executions.
    public void Start()
    {
        m_started = true;
        m_timeStarted = GetTime();
    }

    // Stop will cause the function scheduler to not call any further scheduled functions.
    // This resets the execution back at the beginning.
    public void Stop()
    {
        m_started = false;
    }

    // Clears all previously scheduled stuff.
    public void Clear()
    {
        if( m_started )
        {
            DebugUtil.LogWarning( "Called FunctionScheduler.Clear() while it is running!" );
        }

        m_actions.Clear();
    }

    // Return a reference to a notifier.
    public Notifier notifier
    {
        get { return m_notifier; }
    }

    // Call this per frame or it won't work.
    public void Update()
    {
        if( !m_started )
        {
            return;
        }

        float timeSinceStarted = GetTime() - m_timeStarted;
        for( int i = 0; i < m_actions.Count; ++i )
        {
            if( m_actions[ i ].delay <= timeSinceStarted )
            {
                if( m_actions[ i ].functionToExec != null )
                {
                    m_actions[ i ].functionToExec();
                }
            }
        }
        
        for( int i = m_actions.Count - 1; i >= 0; --i )
        {
            if( m_actions[ i ].delay <= timeSinceStarted )
            {
                m_actionsDone.Add( m_actions[ i ] );
                m_actions.RemoveAt( i );
            }
        }

        if( m_actions.Count == 0 )
        {
            m_started = false;
            m_notifier.NotifySubscribers( "ScheduledFunctionSet.OnComplete" );
            List< ScheduledFunction > temp = m_actionsDone;
            m_actionsDone = m_actions;
            m_actions = temp;
        }
    }

    private float GetTime()
    {
        if( m_timer == null )
        {
            return Time.time;
        }

        return m_timer.GetTime();
    }

    protected List< ScheduledFunction > m_actions       = new List< ScheduledFunction >();
    protected List< ScheduledFunction > m_actionsDone   = new List< ScheduledFunction >();
    protected Notifier m_notifier                       = new Notifier();
    protected Timer m_timer                             = null;
    protected float m_timeStarted                       = 0.0f;
    protected bool m_started                            = false;
    
	/// <summary>
	/// This simple structure that stores the time (delay) it will execute 
	/// the function (functionToExec) assigned.
	/// </summary>
	[System.Serializable]
	public class ScheduledFunction
	{
		// A normal parameterless void function.
		public delegate void function();
		
		// Constructor with defaults to be assigned.
		public ScheduledFunction( float time, function fn )
		{
			delay = time;
			functionToExec = fn;
		}
		
		public float delay                  = 0.0f; // the time relative to the beginning time of the scheduler.
		public function functionToExec      = null; // the function to execute when the time to execute it comes.
	}
}
