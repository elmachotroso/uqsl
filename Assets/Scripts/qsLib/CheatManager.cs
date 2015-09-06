/*
Unity Quickstart Library (qsLib)
http://www.andreivictor.net/uqsl/
Last Updated: 08/30/2015

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
using QsLib;

/// <summary>
/// Cheat manager is a utility that aids in debugging by having certain codes
/// activated. The cheats are activated if it is typed. Think Doom or Duke Nukem
/// cheat code typing. This only works for devices with keyboards.
/// Usage: Attach the CheatManager to a GameObject that can persist for the
/// whole life of the application.
/// </summary>
public class CheatManager : Singleton< CheatManager >
{
    // This can be used by other systems to check if a certain cheat code is
    // activated. Returns true if the cheat is already activated.
    public bool IsCheatActivated( string cheat )
    {
        if( m_cheats.ContainsKey( cheat ) )
        {
            return m_cheats[ cheat ];
        }
    
        return false;
    }
    
    protected void Awake()
    {
        if( m_cheatEntry != null && m_cheatEntry.Length > 0 )
        {
			foreach( string name in m_cheatEntry )
			{
				m_cheats.Add( name, false );
			}
        }
    }
    
    protected void Update()
    {
    	if( m_cheatEntry == null || m_cheatEntry.Length == 0 )
    	{
    		return;
    	}
    	
        foreach( KeyCode keycode in KeyCode.GetValues( typeof( KeyCode ) ) )
        {
            if( Input.GetKeyDown( keycode ) )
            {
                StringBuilder sb = new StringBuilder( m_lastStringTyped );
                sb.Append( keycode.ToString() );
                if( sb.Length > m_maxStringLength )
                {
                    int sizeToCut = sb.Length - m_maxStringLength;
                    sb.Remove( 0, sizeToCut );
                }
                m_lastStringTyped = sb.ToString();
                
                // iterate and check for cheat activation
                // if cheat was activated, clean the buffer.
                for( int i = 0; i < m_cheatEntry.Length; ++i )
                {
                    if( m_lastStringTyped.Contains( m_cheatEntry[ i ] ) )
                    {
                        OnCheatMatched( i );
                        m_lastStringTyped = "";
                        break;
                    }
                }
            }
        }
    }
    
    protected CheatManager() {} // singleton requirement
    
    private void OnCheatMatched( int cheatNumber )
    {
        if( cheatNumber >= m_cheatEntry.Length )
        {
            return;
        }
        
        m_lastCheatMatched = m_cheatEntry[ cheatNumber ];
		m_cheats[ m_lastCheatMatched ] = !m_cheats[ m_lastCheatMatched ];
		bool isCheatActivated = m_cheats[ m_lastCheatMatched ];
		DebugUtil.LogWarning( string.Format( "{0} {1}",
			m_lastCheatMatched, ( isCheatActivated ? "activated!" : "deactivated!" ) ) );
			
        SoundManager.Instance.PlaySfx( "cheat_activation" );
		SubscriptionManager.Instance.NotifySubscribers( "CheatManager.OnCheatMatched", m_lastCheatMatched );
    }
    
    [SerializeField] protected int m_maxStringLength        = 32;               // The total number of characters the character buffer can hold.
    [SerializeField] protected string m_lastCheatMatched    = "";               // The previous activated cheat.
    [SerializeField] protected string[] m_cheatEntry        = new string[] {};  // The collection of cheat codes as input in the editor.
    private string m_lastStringTyped            			= "";
	private Dictionary< string, bool > m_cheats    			= new Dictionary< string, bool >();
}
