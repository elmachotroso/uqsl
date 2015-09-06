/*
Unity Quickstart Library (qsLib)
http://www.andreivictor.net/uqsl/
Last Updated: 09/05/2015

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
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using QsLib;

/// <summary>
/// Save manager is a comprehensive game state save/load utility. It lets you
/// store data as key-value pairs in memory at runtime and provides the options 
/// to save or load them to/from a binary file. Aside from that, this Save
/// manager provides the following features that PlayerPrefs could not.
/// 1. Setting and retrieval of value types and reference types in memory.
/// 2. Optionally set an encryption/decryption algorithm via .encrypter and
///    .decrypter fields.
/// 3. Optionally set a checksum algorithm via .checksumer.
/// 4. Optionally set a data consolidater via .consolidater to determine save
///    upgrades.
/// 5. A special backup file for the last known good save file to load.
///
/// Important Notes:
/// Notifier Events to consider:
/// 1. SaveManager.OnLoadRequest - executes when the .Load() method is called but
///    before loading the actual file.
/// 2. SaveManager.OnSaveRequest - executes when the .Save() method is called but
///    before writing the actual file. This is actually the best event to respond to
///    by saving your individual data via Set().
/// 3. SaveManager.OnNewDataLoaded - This event is when all the loading process is
///    done. Usually, this is the perfect time to Get() all the data and put it into
///    your classes/variables.
/// </summary>
public class SaveManager : Singleton< SaveManager >
{
    // Retrieve value-type data with given key. This returns true if the operation
    // has actually retrieved data from an existing Key. Depending on the type,
    // The output value would be equivalent to 0, 0.0f, false, or null.
    public bool Get< TValue >( string key, ref TValue output ) where TValue : struct
    {
        return m_data.Get( key, ref output );
    }

    // Retrieve reference-type data with given key. This returns true if the operation
    // has actually retrieved data from an existing Key. This will turn the output
    // value to null.
    public bool GetObject< T >( string key, out T output ) where T : class
    {
        return m_data.GetObject( key, out output );
    }

    // Set the input value to the specified key. If an existing key exists, it is
    // overwritten.
    public void Set< T >( string key, T input )
    {
        m_data.Set( key, input );
    }

    // Returns true if the specified key exists. Otherwise, false.
    public bool HasKey( string key )
    {
        return m_data.HasKey( key );
    }

    // Clears all key-value pairs in the SaveManager.
    public void Clear()
    {
        m_data.Clear();
    }

    // Tells the SaveManager to start performing load procedure and read a saved or
    // backup file. A SaveManager.OnLoadRequest is notfied by the notifier before
    // performing load procedure.
    public void Load()
    {
        m_notifier.NotifySubscribers( "SaveManager.OnLoadRequest" );
        if( !PerformReading() )
        {
            DebugUtil.LogError( string.Format( "SaveManager: {0}", m_lastError ) );
        }
    }

    // Tells the SaveManager to start performing save procedure and write a saved or
    // backup file. A SaveManager.OnSaveRequest is notfied by the notifier before
    // performing load procedure.
    public void Save()
    {
        m_notifier.NotifySubscribers( "SaveManager.OnSaveRequest" );
        if( !PerformWriting() )
        {
            DebugUtil.LogError( string.Format( "SaveManager: {0}", m_lastError ) );
        }
    }

    // Returns the last error message encountered by the SaveManager.
    public string GetLastError()
    {
        return m_lastError;
    }

    // Assign a consolidater method to execute when loading. Consolidater is a
    // procedure wherein you try to merge two data of different versions to form
    // an entirely new dataset.
    public ConsolidateMethod consolidater
    {
        get { return m_consolidater; }
        set { m_consolidater = value; }
    }

    // Assign an encryption algorithm when writing a file.
    public EncryptionAlgorithm encrypter
    {
        get { return m_encrypter; }
        set { m_encrypter = value; }
    }

    // Assign a decryption algorithm when reading a file.
    public DecryptionAlgorithm decrypter
    {
        get { return m_decrypter; }
        set { m_decrypter = value; }
    }

    // Assign a checksum algorithm when comparing files.
    public ChecksumAlgorithm checksum
    {
        get { return m_checksumer; }
        set { m_checksumer = value; }
    }

    // Use this to subscribe to SaveManager notifications.
    public Notifier notifier
    {
        get { return m_notifier; }
    }

    protected void Awake()
    {
#if UNITY_IOS
        System.Environment.SetEnvironmentVariable( "MONO_REFLECTION_SERIALIZER", "yes" );
#endif //UNITY_IOS
    }
    
    protected SaveManager() {} // Singleton
    
    private bool PerformWriting()
    {
        if( m_busy )
        {
            m_lastError = "Cannot call write when the file is in use!";
            return false;
        }
        
        m_busy = true;
        
        // Fill up header.
        Set( "version_major", ApplicationSettings.Instance.info.version_major );
        Set( "version_minor", ApplicationSettings.Instance.info.version_minor );
        Set( "version_rev", ApplicationSettings.Instance.info.version_rev );
        Set( "nonfresh_save", true );
        
        byte[] rawData = null;

#if UNITY_IOS
        Hashtable hashedData = new Hashtable( m_data );
        ConvertObjectToBytes( ref hashedData, out rawData );
#else
        ConvertObjectToBytes( ref m_data, out rawData );
#endif //UNITY_IOS
        
        rawData = DoEncryption( ref rawData );
        
        // Create folders, create backups, and write the data.
        string fullPath = Path.Combine( AppDir.SavePath, m_saveFilename );
        AppDir.CreatePathIfNeeded( fullPath );
        
        File.WriteAllBytes( fullPath, rawData );
        DebugUtil.Log( string.Format( "Data saved as: {0}", fullPath ) ); 
        
        m_busy = false;
        
        return true;
    }
    
    private bool PerformReading()
    {
        if( m_busy )
        {
            m_lastError = "Cannot call read when the file is in use!";
            return false;
        }
        
        m_busy = true;
        
        bool isUsingBackup = false;
        
        string fullPath = Path.Combine( AppDir.SavePath, m_saveFilename );
        
        if( !File.Exists( fullPath ) )
        {
            // load up last known working save file instead.
            fullPath = string.Format( "{0}{1}", fullPath, ".bak" );
            
            if( File.Exists( fullPath ) )
            {
                isUsingBackup = true;
            }
            else
            {
                // Note: Leave it be... if there's no file to load then it means
                // it is a new game entirely.
                //m_lastError = "Save files does not exist!";
                m_busy = false;
                return true;
            }
        }
        
        byte[] rawData = File.ReadAllBytes( fullPath );
        if( rawData == null )
        {
            m_lastError = "File does not contain data.";
            m_busy = false;
            return false;
        }
        
        rawData = DoDecryption( ref rawData );

        DataTable loadedData = new DataTable();

#if UNITY_IOS
        Hashtable hashedLoadedData = new Hashtable();
        ConvertBytesToObject( ref rawData, out hashedLoadedData );
        ConvertHashtableToDataTable( hashedLoadedData, ref loadedData );
#else
        ConvertBytesToObject( ref rawData, out loadedData );
#endif // UNITY_IOS
        
        DataTable finalData = new DataTable();
        ConsolidateData( ref m_data, ref loadedData, ref finalData );

        // Clear the current contents of m_data
        Clear();

        // Then put in the final consolidated data to m_data.
        m_data = finalData;

        m_notifier.NotifySubscribers( "SaveManager.OnNewDataLoaded", finalData );
        
        // This file is now known to be a good save file, and we treat it as
        // latest known working save file (backup).
        if( !isUsingBackup )
        {
            // Create a backup
            string backupFile = string.Format( "{0}{1}", fullPath, ".bak" );
            
            // Design decision: No need for checksum check here as saving operation
            // would be faster and straight-forward than spending cycles on
            // opening and extracting data from the backup file AND THEN
            // computing for checksum.
            File.Copy( fullPath, backupFile, true );
        }
        
        DebugUtil.Log( string.Format( "Data loaded from: {0}", fullPath ) );
        
        DebugUtil.Log( finalData.Print() );
        
        m_busy = false;
        
        return true;
    }

#if UNITY_IOS
    private DataTable ConvertHashtableToDataTable( Hashtable table, ref DataTable dictionary )
    {
        if( table != null )
        {
            foreach( string key in table.Keys )
            {
                dictionary.Add( key, table[ key ] );
            }
        }

        return dictionary;
    }
#endif //UNITY_IOS
    
    private void ConvertObjectToBytes< T >( ref T obj, out byte[] byteBuffer )
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream mStream = new MemoryStream();
        bf.Serialize( mStream, obj );
        byteBuffer = mStream.ToArray();
    }
    
    private void ConvertBytesToObject< T >( ref byte[] byteBuffer, out T obj ) where T : class
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream mStream = new MemoryStream( byteBuffer );
        obj = bf.Deserialize( mStream ) as T;
    }
    
    private void ConsolidateData( ref DataTable left, ref DataTable right, ref DataTable output )
    {
        if( m_consolidater == null )
        {
            // We need to make sure the loaded data (assumed to be on the right)
            // is not an accidental save from a fresh save data. We don't want
            // to overwrite the old working data for this.
            bool isNotFresh = false;
            right.Get( "nonfresh_save", ref isNotFresh );
            if( !isNotFresh )
            {
                DebugUtil.Log( "Ignoring invalid 'right' data" );
                output = left;
                return;
            }
            
            DebugUtil.Log( "'right' data is valid'" );
            
            ApplicationSettings.AppInfo infoLeft = CreateAppInfo( ref left );
            ApplicationSettings.AppInfo infoRight = CreateAppInfo( ref right );
            
            output = left;
            if( infoRight.IsEqualVersion( infoLeft ) || infoRight.IsHigherVersionThan( infoLeft ) )
            {
                DebugUtil.Log( "'right' is the better version." );
                output = right;
            }
            else
            {
                DebugUtil.Log( "'left' is the better version." );
            }
            
            return;
        }
        
        m_consolidater( ref left, ref right, ref output );
    }
    
    private byte[] DoEncryption( ref byte[] data )
    {
        if( m_encrypter == null )
        {
            return data;
        }
        
        return m_encrypter( ref data );
    }
    
    private byte[] DoDecryption( ref byte[] data )
    {
        if( m_decrypter == null )
        {
            return data;
        }
        
        return m_decrypter( ref data );
    }
    
    private string GetChecksum( ref byte[] data )
    {
        if( m_checksumer == null )
        {
            return null;
        }
        
        return m_checksumer( ref data );
    }
    
    private ApplicationSettings.AppInfo CreateAppInfo( ref DataTable table )
    {
        ApplicationSettings.AppInfo info = new ApplicationSettings.AppInfo();
        
        table.Get( "version_major", ref info.version_major  );
        table.Get( "version_minor", ref info.version_minor );
        table.Get( "version_rev", ref info.version_rev );
        
        return info;
    }

    // Consolidator method format. Datas from the left and right DataTable should create a valid DataTable
    // on the output.
    public delegate void ConsolidateMethod( ref DataTable left, ref DataTable right, ref DataTable output );

    // Encryption algorithm should take in valid data and encrypt it and returns the encrypted byte array.
    public delegate byte[] EncryptionAlgorithm( ref byte[] data );

    // Decryption algorithm should take in encrypted data and decrypt it and returns the valid data via byte array.
    public delegate byte[] DecryptionAlgorithm( ref byte[] data );

    // Checkum algorithm should return a checksum string based on the given data.
    public delegate string ChecksumAlgorithm( ref byte[] data );
    
    [SerializeField] protected string m_saveFilename    = "";                   // The filename of the save file.
    
    protected Notifier m_notifier                       = new Notifier();
    protected DataTable m_data                          = new DataTable();
    protected bool m_busy                               = false;
    protected ConsolidateMethod m_consolidater          = null;
    protected EncryptionAlgorithm m_encrypter           = null;
    protected DecryptionAlgorithm m_decrypter           = null;
    protected ChecksumAlgorithm m_checksumer            = null;
    private string m_lastError                          = "";

    /// <summary>
    /// DataTable is simply an alias of a serializable Dictionary< string, object >.
    /// </summary>
    [System.Serializable]
    public class DataTable : Dictionary< string, System.Object >
    {
        public DataTable() : base()
        {
        }
        
        protected DataTable( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
        
        public bool Get< TValue >( string key, ref TValue output ) where TValue : struct
        {
            if( !HasKey( key ) )
            {
                return false;
            }
            
            output = (TValue) this[ key ];
            return true;
        }
        
        public bool GetObject< T >( string key, out T output ) where T : class
        {
            if( !HasKey( key ) )
            {
                output = null;
                return false;
            }
            
            output = this[ key ] as T;
            return true;
        }
        
        public void Set< T >( string key, T input )
        {
            if( HasKey( key ) )
            {
                this[ key ] = input;
                return;
            }
            
            this.Add( key, input );
        }
        
        public bool HasKey( string key )
        {
            return this.ContainsKey( key );
        }
        
        public string Print()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "Total items in table: {0}", this.Count );
            foreach( KeyValuePair< string, object > pair in this )
            {
                sb.AppendFormat( "\nKey: {0}, Value: {1}", pair.Key, pair.Value );
            }
            
            return sb.ToString();
        }
    }
}

#region Test script
/*
using UnityEngine;
using System.Collections;
using QsLib;

public class TestScript : MonoBehaviour
{
    protected void Start()
    {
        if( m_Load )
        {
            SaveManager.Instance.Load();
            
            bool isAndrei = false;
            if( !SaveManager.Instance.Get( "is andrei", ref isAndrei ) || !isAndrei )
            {
                DebugUtil.LogError( "is andrei is wrong." );
            }
            
            int zeroInt = int.MaxValue;
            if( !SaveManager.Instance.Get( "zero int", ref zeroInt ) || zeroInt != 0 )
            {
                DebugUtil.LogError( "zero int is wrong." );
            }
            
            string myName = "none";
            if( !SaveManager.Instance.GetObject( "my name", out myName ) || myName != "andrei o victor" )
            {
                DebugUtil.LogError( "my name is wrong." );
            }
            
            float epsilon = 0.0f;
            if( !SaveManager.Instance.Get( "float epsilon", ref epsilon ) || epsilon != float.Epsilon )
            {
                DebugUtil.LogError( "float epsilon is wrong." );
            }
            
            DebugUtil.LogWarning( "Test passed!" );
        }
        
        if( m_Save )
        {
            SaveManager.Instance.Set( "is andrei", true );
            SaveManager.Instance.Set( "zero int", 0 );
            SaveManager.Instance.Set( "my name", "andrei o victor" );
            SaveManager.Instance.Set( "float epsilon", float.Epsilon );
            SaveManager.Instance.Save();
        }
    }
    
    [SerializeField] protected bool m_Load      = true; // Enables load procedure.
    [SerializeField] protected bool m_Save      = true; // Enables save procedure.
}
*/
#endregion
