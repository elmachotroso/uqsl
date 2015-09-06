/*
Unity Quickstart Library (qsLib)
http://www.andreivictor.net/uqsl/
Last Updated: 09/06/2015

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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

/// <summary>
/// QsLib contains the general purpose functions, classes, and design patterns
/// usable in almost every aspect of programming and that are not found in the default Unity and C#
/// libraries.
/// </summary>
namespace QsLib
{
    /// <summary>
    /// Math/Numerical helpers.
    /// </summary>
    public class Math
    {
        // Determine if the floating point expression will result to a value near 0.0f within epsilon.
        public static bool IsWithinEpsilon( float expression, float epsilon = float.Epsilon )
        {
            return Mathf.Abs( expression ) < epsilon;
        }
        
        // An equality operator for floating point that considers epsilon.
        public static bool IsEqual( float expression, float compareTo, float epsilon = float.Epsilon )
        {
            return IsWithinEpsilon( compareTo - expression, epsilon );
        }

        // Returns a value that clamps values less than 0.0f as 0.0f;
        public static float Clamp0( float value )
        {
            return value < 0.0f ? 0.0f : value;
        }

        // Returns the clamped value of number within the min and max inclusive.
        public static int Clamp( int min, int max, int number )
        {
            if( number < min )
            {
                return min;
            }

            if( number > max )
            {
                return max;
            }

            return number;
        }
    }
    
    /// <summary>
    /// Debug util are aliases to the traditional unity debug logging, which can be filtered to
    /// which kind of debug logs are to be allowed to be displayed. In theory, disabling some
    /// debug logging while on release will help performance gains. Unfortunately, there is no
    /// C-style macros available to exploit this but for now, these will do.
    /// </summary>
    public class DebugUtil
    {
        // Normal log
        public static void Log( object message )
        {
            if( m_showDebugLogs && m_enableNormalLogs )
            {
                Debug.Log( message );
            }
        }
        
        // Warning log
        public static void LogWarning( object message )
        {
            if( m_showDebugLogs && m_enableWarnings )
            {
                Debug.LogWarning( message );
            }
        }
        
        // Error log
        public static void LogError( object message )
        {
            if( m_showDebugLogs && m_enableErrors )
            {
                Debug.LogError( message );
            }
        }
        
        // Enable or disable the invocation of all logs. This will effectively
        // disable all logging even if the individual settings are set differently.
        public static void ShowDebug( bool enable )
        {
            m_showDebugLogs = enable;
        }
        
        // Enable or disable display of normal logs.
        public static void EnableNormalLogs( bool enable )
        {
            m_enableNormalLogs = enable;
        }
        
        // Enable or disable display of warnings logs.
        public static void EnableWarnings( bool enable )
        {
            m_enableWarnings = enable;
        }
        
        // Enable or disable display of error logs.
        public static void EnableErrors( bool enable )
        {
            m_enableErrors = enable;
        }
        
        private static bool m_showDebugLogs    = true;
        private static bool m_enableNormalLogs = true;
        private static bool m_enableWarnings   = true;
        private static bool m_enableErrors     = true;
    }
    
    /// <summary>
    /// Functions to alias and simplify commonly used paths to follow a certain organization. Feel free to modify the
    /// paths below to suit your needs. Using these is definitely better than using the read-only paths provided by
    /// Unity Engine.
    /// </summary>
    public class AppDir
    {
        // Path to the writeable streaming path.
        public static string StreamPath
        {
            get { return Application.streamingAssetsPath; }
        }
        
        // Path to the writeable data path in streaming path. The purpose is to store actual data-related assets.
        public static string DataPath
        {
            get { return Path.Combine( StreamPath, "data" ); }
        }
        
        // Path to the per-user writeable path.
        public static string UserPath
        {
            get { return Application.persistentDataPath; }
        }
        
        // Path to the save sub-folder in the per-user writeable path. This is to explicitly point to where save files
        // are ideally created and stored.
        public static string SavePath
        {
            get { return Path.Combine( Application.persistentDataPath, "save" ); }
        }
        
        // Path to the writeable temporary files path where its contents can disappear at any time.
        public static string TempPath
        {
            get { return Application.temporaryCachePath; }
        }
		
		// Recreates the full directory structure given the path/filename. If the directory already exists, nothing happens.
        public static void CreatePathIfNeeded( string path )
        {
            string dir = Path.GetDirectoryName( path );
            if( !Directory.Exists( dir ) )
            {
                Directory.CreateDirectory( dir );
            }
        }
    }
    
    /// <summary>
    /// Collection of helpful data structures and functions related to objects in Unity that would be great if they
    /// existed in the first place.
    /// </summary>
    public class ObjectHelpers
    {
        // A simple structure to contain the transform information.
        public struct TransformInfo
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }
        
        // Copy a Transform to another.
        public static void CopyTransform( Transform source, Transform dest )
        {
            dest.position = source.position;
            dest.rotation = source.rotation;
            dest.localScale = source.localScale;
        }
        
        // Copy/Store a Transform's values to a TransformInfo structure.
        public static void CopyTransform( Transform source, ref TransformInfo dest )
        {
            dest.position = source.position;
            dest.rotation = source.rotation;
            dest.scale = source.localScale;
        }
        
        // Copy/Store a TransformInfo's values to a Transform structure.
        public static void CopyTransform( ref TransformInfo source, Transform dest )
        {
            dest.position = source.position;
            dest.rotation = source.rotation;
            dest.localScale = source.scale;
        }
        
        // Copy/Store a TransformInfo to another TransformInfo.
        public static void CopyTransform( ref TransformInfo source, ref TransformInfo dest )
        {
            dest.position = source.position;
            dest.rotation = source.rotation;
            dest.scale = source.scale;
        }
        
        // Returns an array of all child transforms of a specified GameObject
        public static Transform[] GetChildren( GameObject go )
        {
            int childrenCount = go.transform.childCount;
            Transform[] children = new Transform[ childrenCount ];
            
            for( int i = 0; i < childrenCount; ++i )
            {
                children[ i ] = go.transform.GetChild( i );
            }
            
            return children;
        }
        
        // Attach all child transforms to the specified GameObject parent.
        public static void AttachChildren( GameObject parent, Transform[] children )
        {
            foreach( Transform child in children )
            {
                child.SetParent( parent.transform );
            }
        }
    }
    
    /// <summary>
    /// Object pool is a resuable design pattern to create a limited amount of preallocated objects
    /// and lets you retrieve available "unused" objects and return "used" objects for later reuse.
    /// Usage: Create ObjectPool< Type > and use one of the constructors documented below.
    /// </summary>
    public class ObjectPool< T >
        where T : class
    {
        // Delegate functions for creation (InstanceFunction) and destruction (DestroyFunction) of
        // type T objects used in this ObjectPool. Clue: These act as constructor and destructor
        // of the type T objects!
        public delegate T InstanceFunction();
        public delegate void DestroyFunction( T objectToDestroy );
        
        // Retrieve an available and ready object in the object pool for use and mark it as used.
        public T GetReadyObject()
        {
            for( int i = 0; i < m_objects.Count; ++i )
            {
                PoolItem< T > item = m_objects[ i ];
                if( item.m_state == ItemLife.Ready )
                {
                    item.m_state = ItemLife.Claimed;
                    ++m_ClaimedObjects;
                    --m_ReadyObjects;
                    return item.m_object;
                }
            }
            
            return null;
        }
        
        // This will return an object back to the pool and become "unused".
        public void ReturnObject( T theObject )
        {
            if( theObject != null )
            {
                for( int i = 0; i < m_objects.Count; ++i )
                {
                    PoolItem< T > obj = m_objects[i];
                    if( obj.m_object == theObject )
                    {
                        obj.m_state = ItemLife.Ready;
                        ++m_ReadyObjects;
                        --m_ClaimedObjects;
                    }
                }
            }
        }
        
        // Returns the number of objects in the pool.
        public int GetObjectsInPoolSize()
        {
            return m_ClaimedObjects + m_ReadyObjects + m_NullObjects;
        }

        // Returns the number of objects ready in this pool.
        public int GetReadyObjectsInPoolSize()
        {
            return m_ReadyObjects;
        }

        // Returns the number of objects claimed in this pool.
        public int GetClaimedObjectsInPoolSize()
        {
            return m_ClaimedObjects;
        }
        
        // Simple constructor with expected number of objects to preallocate.
        public ObjectPool( int size ) : this()
        {
            for( int i = 0; i < size; ++i )
            {
                PoolItem< T > newItem = new PoolItem< T >();
                newItem.m_object = null;
                newItem.m_state = ItemLife.Null;
                ++m_NullObjects;            
                m_objects.Add( newItem );
            }
        }
        
        // Constructor with expected number of objects and instance and destroy functions specified.
        public ObjectPool( int size, InstanceFunction cfunc, DestroyFunction dfunc ) : this()
        {
            m_InstanceFxn = cfunc;
            m_DestroyFxn = dfunc;
            
            for( int i = 0; i < size; ++i )
            {
                PoolItem< T > newItem = new PoolItem< T >();
                newItem.m_object = m_InstanceFxn();
                if( newItem.m_object == null )
                {
                    newItem.m_state = ItemLife.Null;
                    ++m_NullObjects;
                }
                else
                {
                    newItem.m_state = ItemLife.Ready;
                    ++m_ReadyObjects;
                }
                
                m_objects.Add( newItem );
            }
        }
        
        // Returns the list of objects in the pool.
        public List< PoolItem< T > > PoolObjects
        {
            get
            {
                return m_objects;
            }
        }
        
        // Pool Item class internally used by the object pull to take hold of
        // the object and the object's state.
        public class PoolItem< T > where T : class
        {
            public T m_object          = null;
            public ItemLife m_state    = ItemLife.Null;
        }
        
        // States enumeration for the life of the object.
        public enum ItemLife
        {
            Null = 0,
            Ready,
            Claimed,
        }
        
        // Default Contstructor protected to prevent explicit use.
        protected ObjectPool()
        {
            m_objects = new List< PoolItem< T > >();
        }
        
        // This effectively nullifies the specified object.
        protected void DestroyObject( T theObject )
        {
            if( theObject != null )
            {
                for( int i = 0; i <  m_objects.Count; ++i )
                {
                    PoolItem< T > obj = m_objects[i];
                    if( obj.m_object == theObject )
                    {
                        if( obj.m_state == ItemLife.Ready )
                        {
                            ++m_NullObjects;
                            --m_ReadyObjects;
                        }
                        else if( obj.m_state == ItemLife.Claimed )
                        {
                            ++m_NullObjects;
                            --m_ClaimedObjects;
                        }
                        
                        if( m_DestroyFxn != null )
                        {
                            m_DestroyFxn( obj.m_object );
                        }                    
                        obj.m_object = null;
                        obj.m_state = ItemLife.Null;
                    }
                }
            }
        }
        
        private int m_ClaimedObjects             = 0;
        private int m_ReadyObjects               = 0;
        private int m_NullObjects                = 0;
        private InstanceFunction m_InstanceFxn   = null;
        private DestroyFunction m_DestroyFxn     = null;
        private List< PoolItem< T > > m_objects  = null;
    }
    
	/// <summary>
	/// Notifier class reduces dependency by being the middle-man of messages or events
	/// to be passed around to interested parties called "Subscribers". When a message or event
	/// is invoked via NotifySubscribers, it is sent to all subscribers in this system.
	/// 
	/// Usage: Subscribers may choose to respond or just ignore the message broadcasted.
	/// To be a subscriber, a code must implement the NotifiableMethod delegate and pass that
	/// function as parameter to the AddSubscriber method.
	/// 
	/// Warning: It is highly recommended to remove a subscriber appropriately especially when
	/// the object the function belongs to is no longer present. You don't want the method to be
	/// executed on illogicaly. To remove a subscriber, simply use the RemoveSubscriber method.
	/// </summary>
	public class Notifier
	{
		// The delegate method utilized by the SubscriptionManager to map to callbacks
		// for subscribers.
		public delegate void NotifiableMethod( string message, object param );
		
		// Add a NotifiableMethod delegate as a subscriber to be executed as callback when a
		// message is received.
		public void AddSubscriber( NotifiableMethod sub )
		{
			if( FindSubscriber( sub ) == -1 )
			{
				m_subscribers.Add( sub );
			}
		}
		
		// Remove a NotifiableMethod delegate from getting executed whenever a message is
		// received.
		public void RemoveSubscriber( NotifiableMethod sub )
		{
			if( FindSubscriber( sub ) != -1 )
			{
				m_subscribers.Remove( sub );
			}
		}
		
		// For systems requiring to invoke the callback of interested subscribers, pass a
		// unique message and an optional parameters for all subscribers to receive.
		public void NotifySubscribers( string message, object param = null )
		{
			foreach( NotifiableMethod notifyMethod in m_subscribers )
			{
				notifyMethod( message, param );
			}
		}
		
		protected int FindSubscriber( NotifiableMethod sub )
		{
			return m_subscribers.IndexOf( sub );
		}
		
		private List< NotifiableMethod > m_subscribers    = new List< NotifiableMethod > ();
	}
    
    /// <summary>
    /// FsmState is an abstract class that defines the structure for implementing a state that will
    /// be runnable in an Finite State Machine (Fsm) class. You use this encapsulate a particular case
    /// of the flow of your code.
    /// Usage:
    /// 1) Implement a concrete FsmState class (preferably a nested class, but not required) and set
    /// TParentClass a class you want your state class to access to.
    /// 2) Concrete FsmState class should call FsmState constructor.
    /// 3) Concrete FsmState should implement OnEnter, OnUpdate, and OnExit methods. OnEnter is the
    /// code section upon entering the state and is executed only once. OnExit is the code section
    /// executed once when transitioning out from the state. OnUpdate is executed per frame update
    /// during the time the state is the active state in the Fsm.
    /// 4) Instantiate concrete FsmState and add it to an instance of an Fsm class.
    /// </summary>
    public abstract class FsmState< TParentClass > where TParentClass : class
    {
        // Constructor where a reference to the fsm and parent class is passed.
        public FsmState( Fsm< TParentClass > fsm, TParentClass parent )
        {
            m_fsm = fsm;
            m_parent = parent;
        }
        
        // Quick accessor to the parent class assigned.
        public TParentClass parent
        {
            get { return m_parent; }
        }
        
        // Quick accessor to the Fsm class assigned.
        public Fsm< TParentClass > fsm
        {
            get { return m_fsm; }
        }
        
        // Abstract methods to be implemented.
        public virtual void OnEnter() {}
        public virtual void OnUpdate( float dt ) {}
        public virtual void OnExit() {}
        
        protected FsmState() {} // prevent use
        
        protected TParentClass m_parent     = null;
        protected Fsm< TParentClass > m_fsm = null;
    }
    
    /// <summary>
    /// Finite State Machine (Fsm) class handles the execution of states in a certain order.
    /// Using the states within the machine, it executes code sections for entry, update, and
    /// on exit. To move one from one state to another, a transition is used.
    /// Usage:
    /// 1) See and apply Usage of FsmState class.
    /// 2) Make sure this instance of Fsm class has its update been called every frame.
    /// 3) Use TransitionTo< NextState >() as you wish but be careful not to cause cyclic.
    /// Note: It is advisable NOT to put TransitionTo commands in OnExit().
    /// </summary>
    public class Fsm< TParentClass > where TParentClass : class
    {
        // Constructor - you must pass an instance of the one to be considered the parent class of the object.
        public Fsm( TParentClass parent )
        {
            m_parent = parent;
            m_states = new Dictionary< System.Type, FsmState< TParentClass > >();
        }
        
        // Retrieve the parent instance.
        public TParentClass Parent
        {
            get { return m_parent; }
        }
        
        // Returns true if the specified TFsmState is the current state.
        public bool IsIn< TFsmState >()
        {
            return m_state == null ? false : m_state.GetType() == typeof( TFsmState );
        }

        // Sometimes needed to determine if you're in the verge of transitioning to a new state.
        // This case is uncommon and if you do need it, you will need IsIn() method to go with it.
        public bool IsTransitioning()
        {
            return m_transitioning;
        }
        
        // Add a valid FsmState of the same parent class in this machine.
        public void AddState( FsmState< TParentClass > state )
        {
            if( !m_states.ContainsKey( state.GetType() ) )
            {
                m_states.Add( state.GetType(), state );
            }
        }
        
        // Clears all states in this machine. If callExit is true and clear is called, it will explicity call
        // OnExit of the current active state first before clearing all states.
        public void Clear( bool callExit = true )
        {
            if( callExit )
            {
                m_state.OnExit();
            }
            
            m_transitioning = false;
            m_nextState = null;
            m_state = null;
            
            m_states.Clear();
        }
        
        // Get current active FsmState object.
        public FsmState< TParentClass > GetCurrentState()
        {
            return m_state;
        }
        
        // Accessor to get the previous state the fsm was before transition.
        public FsmState< TParentClass > GetPreviousState()
        {
            return m_prevstate;
        }
        
        // Get a specific state object in the state machine.
        public FsmState< TParentClass > GetState< TFsmState >()
        {
            return m_states.ContainsKey( typeof( TFsmState ) ) ? m_states[ typeof( TFsmState ) ] : null;
        }
        
        // TransitionTo signals this machine to change states and executes the OnExit and OnEnter callbacks
        // appropriately. This method returns true if the call to this method is acceptable. Otherwise, it
        // would mean the machine is already transitioning to another state.
        public bool TransitionTo< TFsmState >()
        {
            if( m_transitioning )
            {
                return false;
            }
            
            m_transitioning = true;
            m_prevstate = m_state;
            m_states.TryGetValue( typeof( TFsmState ), out m_nextState );
            if( m_nextState == null )
            {
                DebugUtil.LogWarning( "State " + typeof( TFsmState ).ToString() + " is not in the FSM. Was this intentional?" );
            }
            
            return true;
        }
        
        // Another variation of TransitionTo above but with known state provided to transition to. Returns
        // false if the fsm is currently transitioning or the state specified does not exist.
        public bool TransitionTo< TParentClass >( FsmState< TParentClass > state ) where TParentClass : class
        {
            if( m_transitioning || state == null || !m_states.ContainsValue( state ) )
            {
                return false;
            }
            
            m_transitioning = true;
            m_prevstate = m_state;
            m_nextState = state;
            if( m_nextState == null )
            {
                DebugUtil.LogWarning( "State " + state.GetType().ToString() + " is not in the FSM. Was this intentional?" );
            }
            
            return true;
        }
        
        // Generic per-frame update that should be called every frame by the user of the Fsm instance.
        public void Update( float dt )
        {
            if( m_transitioning )
            {
                // we need to do this now before it gets overwritten by OnExit or OnEnter.
                m_transitioning = false;
                FsmState< TParentClass > nextState = m_nextState;
                
                // regardless if transition out is null or not, we call exit.
                // a transition to a null state usually means it's a terminal state.
                if( m_state != null )
                {
                    m_state.OnExit();
                }
                
                m_state = nextState;
                if( m_state != null )
                {
                    m_state.OnEnter();
                }
            }
            else if( m_state != null )
            {
                m_state.OnUpdate( dt );
            }
        }
        
        protected Dictionary< System.Type, FsmState< TParentClass > > m_states = null;
        protected FsmState< TParentClass > m_nextState                         = null;
        protected FsmState< TParentClass > m_prevstate                         = null;
        protected FsmState< TParentClass > m_state                             = null;
        protected TParentClass m_parent                                        = null;
        private bool m_transitioning                                           = false;
    }
    
    /// <summary>
    /// CSV dictionary is a class that enables reading of comma-separated and "-delimited csv files.
    /// The data read from the csv file will become an accessible 2D table using this data type.
    /// Usage: Use CsvReader class below
    /// </summary>
    public class CsvDictionary
    {
        // Constructor that prepares an empty table.
        public CsvDictionary()
        {
            m_table = new Dictionary< string, Dictionary< string, string > >();
        }
        
        // Retrieves a dictionary of the specified row name. (Key-Value pairs of columns
        // for that row) via rowData. If successful, it returns true, otherwise false.
        public bool GetDataRow( string rowName, out Dictionary< string, string > rowData )
        {
            if( !m_table.ContainsKey( rowName ) )
            {
                rowData = null;
                return false;
            }
            
            rowData = m_table[ rowName ];
            return true;
        }
        
        // Returns the data associated on the rown and column.
        public string GetData( string rowName, string columnName )
        {
            if( !m_table.ContainsKey( rowName ) || !m_table[ rowName ].ContainsKey( columnName ) )
            {
                DebugUtil.Log( "Missing keys for '" + rowName + "' or '" + columnName + "'" );
                return "";
            }
            
            return m_table[ rowName ][ columnName ];
        }
        
        // Return the whole dictionary "table".
        public Dictionary< string, Dictionary< string, string > > GetTable()
        {
            return m_table;
        }
        
        // Give how many rows present in the table.
        public int RowsCount
        {
            get { return m_rows == null ? 0 : m_rows.Count; }
        }
        
        // Give how many columns present in the table.
        public int ColumnsCount
        {
            get { return m_columns == null ? 0 : m_columns.Count; }
        }
        
        // Returns a list of the column names existing in the table.
        public List< string > Columns
        {
            get { return m_columns; }
            set { m_columns = value; }
        }
        
        // Returns a list of the row names existing in the table.
        public List< string > Rows
        {
            get { return m_rows; }
            set { m_rows = value; }
        }
        
        private List< string > m_rows                                      = null;
        private List< string > m_columns                                   = null;
        private Dictionary< string, Dictionary< string, string > > m_table = null;
    }
    
    /// <summary>
    /// Csv reader helper class to read csv files.
    /// Usage: Declare a CsvReader, then use Open method to open a csv file, then use
    /// ReadAllToMemory to store all contents of CSV in memory. Lastly, Close if no
    /// </summary>
    // comma-delimited csv reader with " delimeter
    public class CsvReader
    {
        // Open the specified csv file.
        public bool Open( string file )
        {
            bool isJar = file.Contains( "jar:" );

            if( ( !isJar && m_fstream != null ) || m_streamReader != null )
            {
                m_LastError = "Cannot open new file without closing opened stream: " + m_FileToParse;
                return false;
            }

            if( isJar )
            {
                byte[] data = null;
                LoadBytesFromFile( file, out data );
                if( data == null )
                {
                    m_LastError = "Cannot read specified file: " + file;
                    return false;
                }

                m_streamReader = new StreamReader( new MemoryStream( data ), Encoding.Unicode );
            }
            else
            {
                m_fstream = File.OpenRead( file );
                if( !m_fstream.CanRead )
                {
                    m_LastError = "Cannot read specified file: " + file;
                    return false;
                }
                m_streamReader = new StreamReader( m_fstream, Encoding.Unicode );
            }
            
            m_FileToParse = file;
            return true;
        }
        
        // Close an existing opened csv file.
        public void Close()
        {
            if( m_streamReader != null )
            {
                m_streamReader.Close();
                m_streamReader = null;
            }
            
            if( m_fstream != null )
            {
                m_fstream.Close();
                m_fstream = null;
                m_FileToParse = "";
            }
        }
        
        // Retrieve all data and store it into memory as a CsvDictionary.
        public CsvDictionary ReadAllToMemory()
        {
            CsvDictionary dic = new CsvDictionary();
            List< string > rowNames = new List< string >();
            List< string > headers = new List< string >();
            int rows = 0;
            while( !m_streamReader.EndOfStream )
            {
                string line = m_streamReader.ReadLine();
                if( !line.EndsWith( "," ) )
                {
                    line += ",";
                }
                List< string > tokens = TokenizeLine( line );
                if( tokens.Count > 0 )
                {
                    if ( rows == 0 )
                    {
                        // first row is always the headers row.
                        headers = tokens;
                    }
                    else
                    {
                        Dictionary< string, string > thisRow = new Dictionary< string, string >();
                        string rowName = tokens[0];
                        rowNames.Add( rowName );
                        for ( int column = 1; column < tokens.Count; ++column )
                        {
                            thisRow.Add( headers[ column ], tokens[ column ] );
                        }
                        dic.GetTable().Add( rowName, thisRow );
                    }
                    ++rows;
                }
                
                dic.Columns = headers;
                dic.Rows = rowNames;
            }
            
            return dic;
        }
        
        // Retrieve the last error message this CsvReader had encountered.
        public string GetLastError()
        {
            return m_LastError;
        }
        
        private List< string > TokenizeLine( string line )
        {
            List< string > tokens = new List< string >();
            bool quoted = false;
            bool escaped = false;
            StringBuilder token = new StringBuilder();
            for( int i = 0; i < line.Length; ++i )
            {
                char ch = line[ i ];
                
                if( !escaped && ch == '\\' )
                {
                    escaped = true;
                    continue;
                }
                
                if( escaped )
                {
                    switch( ch )
                    {
                        case 'n':
                            ch = '\n';
                            break;
                        default:
                            // keep the same char even if escaped.
                            break;
                    }
                }
                
                if( quoted )
                {
                    if( !escaped && ch == '"' )
                    {
                        quoted = false;
                        continue;
                    }
                    else
                    {
                        token.Append( ch );
                    }
                }
                else
                {
                    if( !escaped && ch == '"' )
                    {
                        quoted = true;
                        continue;
                    }
                    else if( ch == ',' || ( !escaped && ( ch == '\n' || ch == '\r' ) ) )
                    {
                        tokens.Add( token.ToString() );
                        token.Remove( 0, token.Length );
                    }
                    else
                    {
                        token.Append( ch );
                    }
                }
                
                if( escaped )
                {
                    escaped = false;
                }
            }
            
            return tokens;
        }

        private void LoadBytesFromFile( string file, out byte[] data )
        {
            WWW www = null;
            if( Application.platform == RuntimePlatform.Android )
            {
                www = new WWW( file );
            }
            else
            {
                www = new WWW( "file://" + file );
            }
            
            while( www.progress < 1 || !www.isDone )
            {
                //yield return www;
                if( www.error != null && www.error != "" )
                {
                    data = null;
                    DebugUtil.LogError( "CsvReader www error: " + www.error );
                    return;
                }
            }
            
            data = www.bytes;
            DebugUtil.Log( "Successfully loaded data: " + file );
        }
        
        private FileStream m_fstream         = null;
        private StreamReader m_streamReader  = null;
        private string m_FileToParse         = "";
        private string m_LastError           = "";
    }
}