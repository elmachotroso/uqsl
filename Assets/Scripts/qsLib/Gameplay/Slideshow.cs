/*
Unity Quickstart Library (qsLib)
http://www.andreivictor.net/uqsl/
Last Update: 08/30/2015

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
/// Slideshow is a component which handles the slideshow of multiple
/// images in full screen for certain number of times.
/// Usage: Attach this component to a gameobject and supply the splash screens
/// list and other options. This should work once it starts.
/// </summary>
public class Slideshow : MonoBehaviour
{
	protected void Awake()
	{
		/*
		// TODO: ScreenManager on next update
		if( ScreenManager.Instance == null )
		{
			DebugUtil.LogError( "Slideshow script requires an active ScreenManager component." );
			enabled = false;
			return;
		}
		*/
	}
	
    protected void Start()
    {
    	/*
    	// TODO: ScreenManager on next update
		float width = ScreenManager.Instance.GetTargetWidth();
		float height = ScreenManager.Instance.GetTargetHeight();
		*/
		
		float width = 1920.0f;
		float height = 1080.0f;

        m_Fader = Fader.Instance;
        
        if( m_SplashObject == null && m_SplashScreens.Length > 0 )
        {
            m_SplashObject = GameObject.CreatePrimitive( PrimitiveType.Quad );
            if( m_SplashObject != null )
            {
                m_SplashObject.name = "Splash Screen Quad";
				m_SplashObject.transform.position = transform.position;
                m_SplashObject.transform.rotation = transform.rotation;
                m_SplashObject.transform.localScale = new Vector3( width / 100.0f, height / 100.0f, 1.0f );
                m_SplashObject.transform.position = new Vector3(
                    m_SplashObject.transform.position.x,
                    m_SplashObject.transform.position.y,
                    0.0001f
                    );
                Renderer splashObjectRenderer = m_SplashObject.GetComponent< Renderer >() as Renderer;
                if( splashObjectRenderer )
                {
                    splashObjectRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    //splashObjectRenderer.castShadows = false;
                    splashObjectRenderer.receiveShadows = false;
                    splashObjectRenderer.material.color = RenderSettings.ambientLight;
                    splashObjectRenderer.material.shader = m_shaderOnQuad;
                    splashObjectRenderer.material.mainTexture = m_SplashScreens[ 0 ].m_SplashImage;
                }
            }
        }
    }
    
    protected void OnEnabled()
    {
        if( m_SplashScreens == null || m_SplashScreens.Length == 0 )
        {
        	DebugUtil.LogWarning( "Slideshow: List of splash screens are empty. Nothing to do." );
            enabled = false;
            return;
        }
        
        m_CurrentIndex = 0;
        m_NextIndex = 0;
        m_TimeAccum = 0.0f;
    }
    
    protected void Update()
    {
        // Skip key
		if( m_SkipMode != SkipModes.NoSkip && ( Input.GetKeyUp( m_SkipKey ) || Input.touchCount > 0 ) )
		{
			// We end the current slide but accumulating time equivalent to the slide's show time.
			m_TimeAccum += ( m_SplashScreens[ m_CurrentIndex ].m_TimeToShow
				- Timers.Instance.Game.GetDeltaTime() - m_TimeAccum );
				
			if( m_SkipMode == SkipModes.SkipToExit )
			{
				// if skip to exit is chosen, we just set to the final slide.
				m_CurrentIndex = m_SplashScreens.Length - 1;
			}
		}

        if( m_State == SplashScreenLifecycle.Display )
        {
            m_TimeAccum += Timers.Instance.Game.GetDeltaTime();
            if( m_TimeAccum >= m_SplashScreens[ m_CurrentIndex ].m_TimeToShow )
            {
                m_NextIndex = m_CurrentIndex + 1;
                if( m_NextIndex < m_SplashScreens.Length )
                {
                    m_Fader.SetTransitionCallback( OnFinishTransition );
                    m_Fader.FadeOut();
                    m_State = SplashScreenLifecycle.FadeOut;
                }
                else
                {
                    m_State = SplashScreenLifecycle.Exit;
                }
                
            }
        }
        
        if( m_State == SplashScreenLifecycle.FadeOutComplete )
        {
            m_CurrentIndex = m_NextIndex;
            Renderer splashObjectRenderer = m_SplashObject.GetComponent< Renderer >() as Renderer;
            if( splashObjectRenderer )
            {
                splashObjectRenderer.material.mainTexture = m_SplashScreens[ m_CurrentIndex ].m_SplashImage;
            }
            m_Fader.SetTransitionCallback( OnFinishTransition );
            m_Fader.FadeIn();
            m_State = SplashScreenLifecycle.FadeIn;
        }
        
        if( m_State == SplashScreenLifecycle.FadeInComplete )
        {
            m_TimeAccum = 0.0f;
            m_State = SplashScreenLifecycle.Display;
        }

        if( m_State == SplashScreenLifecycle.Exit )
        {
            SceneManager.Instance.LoadScene( m_sceneToLoadNext, m_useTransitionOnSceneLoad, m_useCleanupOnSceneLoad );
            enabled = false;
        }
    }
    
    protected void OnFinishTransition( Fader.CallbackStates state, bool isFadeIn )
    {
        if( isFadeIn )
        {
            m_State = SplashScreenLifecycle.FadeInComplete;
        }
        else
        {
            m_State = SplashScreenLifecycle.FadeOutComplete;
        }
    }

    protected enum SkipModes
    {
        NoSkip        = 0,
        SkipToNext,
        SkipToExit
    }
    
    protected enum SplashScreenLifecycle
    {
        Display    = 0,
        FadeOut,
        FadeOutComplete,
        FadeIn,
        FadeInComplete,
        Exit
    }

    [System.Serializable]
    protected class SplashScreenInfo
    {
        public Texture m_SplashImage                            = null;
        public float m_TimeToShow                               = 1.0f;
    }

    [SerializeField] protected SplashScreenInfo[] m_SplashScreens	= null;                  // Slides to show and their information.
    [SerializeField] protected SkipModes m_SkipMode                 = SkipModes.SkipToNext;  // What behavior is done when skipping is made.
    [SerializeField] protected KeyCode m_SkipKey                    = KeyCode.Space;         // The key that will forcefully skip a slide.
	[SerializeField] protected bool m_loadSceneOnFinish             = false;                 // Is a scene going to be loaded when the slideshow completes?
    [SerializeField] protected string m_sceneToLoadNext             = "";                    // The scene to load when the slideshow ends.
    [SerializeField] protected bool m_useTransitionOnSceneLoad      = true;                  // If transition is used when loading the next scene.
    [SerializeField] protected bool m_useCleanupOnSceneLoad         = true;                  // Use clean up procedures when loading the scene.
	[SerializeField] protected Shader m_shaderOnQuad                = null;                  // Specify what shader the quad will use.
    private SplashScreenLifecycle m_State                   		= SplashScreenLifecycle.Display;
    private int m_CurrentIndex                              		= 0;
    private int m_NextIndex                                 		= 0;
    private float m_TimeAccum                               		= 0.0f;
    private Fader m_Fader                                   		= null;
    private GameObject m_SplashObject                       		= null;
}
