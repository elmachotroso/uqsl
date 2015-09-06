/*
Unity Quickstart Library (qsLib)
http://www.andreivictor.net/uqsl/
Last Updated: 09/01/2015

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
using QsLib;

/// <summary>
/// The scene manager lets you change scenes via script and you can optionally
/// command to do a clean up when a level is being loaded. You can also make
/// use of a transition effect to change from one scene to another. In addition,
/// a delegate function can be executed after clean up scene and before fading in.
/// </summary>
public class SceneManager : Singleton< SceneManager >
{
    // Loads the scene and can optionally use transition and/or add clean up to it.
    public void LoadScene( string scene, bool useTransition = true, bool invokeCleanup = false,
    	BetweenTransitionMethod method = null )
    {
        m_IsLoadInvoked = true;
        m_SceneToLoad = scene;
        m_UseTransition = useTransition;
        m_InvokeCleanup = invokeCleanup;
        m_transitionMethod = method;
    }
    
    // Retrieve the transition color set.
    public Color GetTransitionColor()
    {
        if( m_Fader )
        {
            return m_Fader.GetTransitionColor();
        }
        
        GameObject cam = GameObject.Find( m_CameraNameToFind );
        if( cam )
        {
            Camera camComp = cam.GetComponent< Camera >() as Camera;
            if( camComp )
            {
                return camComp.backgroundColor;
            }
        }
        
        return Color.clear;
    }

    // Returns true if the scene is using transitions.
    public bool GetIsUsingTransition()
    {
        return m_UseTransition;
    }

	// Returns the name or id of the scene loaded now.
	public string GetSceneLoaded()
	{
		return m_SceneLoaded;
	}

	// Returns the name or id of the scene loaded before the current one.
	public string GetPreviousSceneLoaded()
	{
		return m_PreviousSceneLoaded;
	}
	
    protected void NotificationCallback( string message, object param )
    {
		if( message == "CleanupScene.OnCleanupComplete" )
		{
			Application.LoadLevel( m_SceneToLoad );
		}
    }

    protected SceneManager() {} // singleton
    
    protected void Start()
    {
        m_Fader = Fader.Instance;
        if( !m_Fader )
        {
            DebugUtil.LogError( "Fader instance is required." );
            this.enabled = false;
        }

		SubscriptionManager.Instance.AddSubscriber( NotificationCallback );
    }
    
    protected void Update()
    {
        if( m_IsLoadInvoked )
        {
            m_IsLoadInvoked = false;
            
            if( !m_UseTransition )
            {
                if( m_InvokeCleanup )
                {
					Application.LoadLevel( m_NameOfCleanupScene );
                }
                else
                {
                    Application.LoadLevel( m_SceneToLoad );
                }
                
				if( m_transitionMethod != null )
				{
					m_transitionMethod();
					m_transitionMethod = null;
				}
            }
            else
            {
                m_Fader.SetTransitionCallback( FadeFinishCallback );
                m_Fader.FadeOut();
            }
        }
    }

	protected new void OnDestroy()
	{
		SubscriptionManager.Instance.RemoveSubscriber( NotificationCallback );
		base.OnDestroy();
	}
    
    protected void OnLevelWasLoaded( int level )
    {
        if( Application.loadedLevelName != m_NameOfCleanupScene ) // if not cleanup scene
        {
			m_PreviousSceneLoaded = m_SceneLoaded;
			m_SceneLoaded = m_SceneToLoad;

            if( m_UseTransition )
            {
                m_Fader.SetTransitionCallback( FadeFinishCallback );
                m_Fader.FadeIn();
            }
        }
    }

    private void FadeFinishCallback( Fader.CallbackStates state, bool isFadeIn )
    {
        if( !isFadeIn )
        {
            if( m_InvokeCleanup )
            {
                Application.LoadLevel( m_NameOfCleanupScene );
            }
            else
            {
                Application.LoadLevel( m_SceneToLoad );
            }
            
			if( m_transitionMethod != null )
			{
				m_transitionMethod();
				m_transitionMethod = null;
			}
        }        
    }

    public delegate void BetweenTransitionMethod();

	[SerializeField] private string m_NameOfCleanupScene    = "Cleanup";     // The id or name of the scene with clean up procedures.
    [SerializeField] private string m_CameraNameToFind      = "Main Camera"; // The name of the GameObject with the camera component to manipulate.
    private string m_SceneToLoad                            = "";
	private string m_SceneLoaded                            = "";
	private string m_PreviousSceneLoaded                    = "";
    private bool m_UseTransition                            = true;
    private bool m_InvokeCleanup                            = false;
    private bool m_IsLoadInvoked                            = false;
    private Fader m_Fader                                   = null;
    private BetweenTransitionMethod m_transitionMethod      = null;
}
