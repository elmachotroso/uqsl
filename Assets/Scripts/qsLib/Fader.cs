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
using QsLib;

/// <summary>
/// Fader is a utility to create full screen fading in and out effect.
/// Usage: Attach this to a persistent gameobject and use the API below.
/// </summary>
[RequireComponent( typeof( GUITexture ) )]
public class Fader : Singleton< Fader >
{
    // The callback states available. An "Interrupted" callback state
    // means the intended transition was interrupted.
    public enum CallbackStates
    {
        Normal        = 0,
        Interrupted
    }
    
    // A delegate for functions to be called when transition state happens.
    // Use SetTransitionCallback to assign the delegate.
    public delegate void TransitionCallback( CallbackStates state, bool isFadeIn );

    // Sets the callback for transition finish.
    public Fader SetTransitionCallback( TransitionCallback finishCallback )
    {
        m_TransitionFinishCb = finishCallback;
        return this;
    }
    
    // Fade in the screen with the default transition speed and color.
    public void FadeIn()
    {
        FadeIn( m_GlobalTransitionSpeed, m_GlobalTransitionColor );
    }
    
    // Fade in with a desired transition speed desired.
    public void FadeIn( float speed )
    {
        FadeIn( speed, m_GlobalTransitionColor );
    }
    
    // Fade in with a desired transition speed and color.
    public void FadeIn( float speed, Color color )
    {
        if( !m_guiTexture )
        {
            return;
        }
        
        // handle interrupted / spam calls
        if( m_IsFading )
        {
            if( m_TransitionFinishCb != null )
            {
                m_TransitionFinishCb( CallbackStates.Interrupted, ( m_FadeDirection < 0.0f ) );
            }
        }
        
        // fade in scene - alpha out quad
        m_IsFading = true;
        m_FadeDirection = -1.0f;
        m_TransitionSpeed = speed;
        m_TransitionColor = color;
    }
    
    // Fade out with the default transition speed and color.
    public void FadeOut()
    {
        FadeOut( m_GlobalTransitionSpeed, m_GlobalTransitionColor );
    }
    
    // Fade out with the desired transition speed.
    public void FadeOut( float speed )
    {
        FadeOut( speed, m_GlobalTransitionColor );
    }
    
    // Fade out with the desired transition speed and color.
    public void FadeOut( float speed, Color color )
    {
        if( !m_guiTexture )
        {
            return;
        }
        
        // fade out scene - alpha in quad
        m_IsFading = true;
        m_FadeDirection = 1.0f;
        m_TransitionSpeed = speed;
        m_TransitionColor = color;
        m_guiTexture.enabled = true;
    }
    
    // Retrieve the transition color set.
    public Color GetTransitionColor()
    {
        return m_TransitionColor;
    }
    
    protected Fader() {} // singleton
    
    protected void Awake()
    {
        m_TransitionColor = m_GlobalTransitionColor;
        m_TransitionSpeed = m_GlobalTransitionSpeed;

        m_guiTexture = GetComponent< GUITexture >() as GUITexture;
        if( !m_guiTexture )
        {
			DebugUtil.LogError( "This script requires a guiTexture component." );
			enabled = false;
			return;
        }
        
		m_guiTexture.pixelInset = new Rect( 0, 0, Screen.width, Screen.height );
		m_guiTexture.color = new Color( m_TransitionColor.r, m_TransitionColor.g, m_TransitionColor.b, m_Alpha );
		m_guiTexture.enabled = false;
    }
    
    protected void Update()
    {
        if( !m_guiTexture )
        {
            return;
        }
        
        if( m_IsFading )
        {
            m_Alpha += m_FadeDirection * m_TransitionSpeed * Time.deltaTime;
            m_Alpha = Mathf.Clamp01( m_Alpha );
            m_guiTexture.color = new Color( m_TransitionColor.r, m_TransitionColor.g, m_TransitionColor.b, m_Alpha );
            
            if( m_FadeDirection < 0.0f )
            {
                if( m_Alpha < 0.05f )
                {
                    if( m_TransitionFinishCb != null )
                    {
                        m_TransitionFinishCb( CallbackStates.Normal, true );
                    }
                    
                    m_IsFading = false;
                    m_guiTexture.enabled = false;
                }
            }
            else if( m_FadeDirection > 0.0f )
            {
                if( m_Alpha > 0.95f )
                {
                    if( m_TransitionFinishCb != null )
                    {
                        m_TransitionFinishCb( CallbackStates.Normal, false );
                    }
                    
                    m_IsFading = false;
                }
            }
        }
    }

    public Color m_GlobalTransitionColor               = Color.black;  // Default transition color
    public float m_GlobalTransitionSpeed               = 1.0f;         // Default transition speed
    private Color m_TransitionColor                    = Color.black;  // Holds user specified transition color.
    private float m_TransitionSpeed                    = 1.0f;         // Holds user specified transition speed.
    private bool m_IsFading                            = false;        // Fades out or not.
    private float m_FadeDirection                      = 1.0f;         // A multiplier that controls the change of alpha.
    private float m_Alpha                              = 0.0f;         // The current alpha value.
    private TransitionCallback m_TransitionFinishCb    = null;         // Holds functions to call when transition sequences are finished.
    private GUITexture m_guiTexture                    = null;         // Pointer to guiTexture to simulate fades.
}