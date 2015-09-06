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
using System.Collections;

/// <summary>
/// Class Component to override some of global settings.
/// </summary>
public class ApplicationSettings : Singleton< ApplicationSettings >
{
    public AppInfo info
    {
        get { return m_appInfo; }
    }
    
    protected void Start()
    {
    	// Time-related
        Application.targetFrameRate = m_timeSets.targetFrameRate;
        Time.timeScale = m_timeSets.timeScale;
        
        // Physics-related
        Physics.gravity = m_physicsSets.gravity;
    }
    
    protected ApplicationSettings() {} // Singleton

    [SerializeField] protected AppInfo m_appInfo                = new AppInfo();            // Set some identification for this app.
    [SerializeField] protected TimeSettings m_timeSets          = new TimeSettings();       // Set the desired frame rate.
    [SerializeField] protected PhysicsSettings m_physicsSets    = new PhysicsSettings();    // Settings for physics.
    
    /// <summary>
    /// Structure used to contain the version of the app. This structure is also used by the SaveManager to do save file
    /// comparison.
    /// </summary>
    [System.Serializable]
    public class AppInfo
    {
        public ulong version_major  = 0;    // major part of version
        public ulong version_minor  = 0;    // minor part of version
        public ulong version_rev    = 0;    // revision part of version
        
        // Retrieve the equivalent version string.
        public string GetVersionString()
        {
            return string.Format( "{0}.{1}.{2}", version_major, version_minor, version_rev );
        }
        
        // This method helps determine if this class' version is higher than
        // the version supplied in the parameters.
        public bool IsHigherVersionThan( AppInfo info )
        {
            return IsHigherVersionThan( info.version_major, info.version_minor, info.version_rev );
        }
        
        public bool IsHigherVersionThan( ulong major, ulong minor, ulong rev )
        {
            if( major > version_major )
            {
                return false;
            }
            
            if( minor > version_minor )
            {
                return false;
            }
            
            if( rev > version_rev )
            {
                return false;
            }
            
            if( IsEqualVersion( major, minor, rev ) )
            {
                return false;
            }
            
            return true;
        }
        
        // This method helps determine if this class' version is equal to the
        // version supplied in the parameters.
        public bool IsEqualVersion( AppInfo info )
        {
            return IsEqualVersion( info.version_major, info.version_minor, info.version_rev );
        }
        
		// This method helps determine if this class' version is equal to the
		// version supplied in the parameters.
        public bool IsEqualVersion( ulong major, ulong minor, ulong rev )
        {
            return major == version_major && minor == version_minor && rev == version_rev;
        }
    }
    
    /// <summary>
    /// A structure of Time-related settings for the app to use.
    /// Note that for IOS users, it is important to the targetFrameRate setting to override IOS's default cap to 30FPS.
    /// </summary>
    [System.Serializable]
    public class TimeSettings
    {
        public int targetFrameRate      = 60;           // The target appliaction frame rate.
        public float timeScale          = 1.0f;         // time scale of the general time manager.
		//public float fixedTimeStep      = 0.0166667f;   // Fixed time step to apply.
		//public float maxAllowedTimestep = 0.3333333f;   // Maximum allowed time step.
    }
    
    /// <summary>
    /// A structure of Physics-related settings for the app to use.
    /// </summary>
    [System.Serializable]
    public class PhysicsSettings
    {
        public Vector3 gravity          = Physics.gravity;  // The gravity vector for the whole application.
    }
}
