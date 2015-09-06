/*
Unity Quickstart Library (qsLib)
http://www.andreivictor.net/uqsl/
v0.0.1

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
/// A ParallaxObject with the ability to manipulate the texture offset of the material it has.
/// Note: The game object should be able to render a Texture (not a sprite). You should set the
/// texture to repeating/tiling to make this work.
/// Usage: Attach this script to a gameobject with a material that supports texture offset and
/// add that gameobject to the ParallaxScene's list of layers.
/// </summary>
public class ParallaxObject : MonoBehaviour
{
    // Set the scroll velocity (to make it scroll a certain direction at a certain rate).
    public void SetScrollVelocity( float scrollVelocity )
    {
        m_scrollVelocity = scrollVelocity;
    }

    // Set the vertical scroll velocity (to make it vertically scroll a certain direction at a certain rate).
	public void SetVerticalScrollVelocity( float verticalScrollVelocity )
	{
		m_scrollVerticalVelocity = verticalScrollVelocity;
	}
    
    // Manually scroll the parallax object by x and y distances.
    public void Scroll( float xdistance, float ydistance )
    {
        if( !m_renderer )
        {
            return;
        }
        
        Vector2 pos = m_renderer.material.mainTextureOffset;
        pos.x += xdistance * m_ScrollFactor;
        pos.y += ydistance * m_VerticalScrollFactor;
        
        if( pos.x > 1.0f )
        {
            pos.x = pos.x - ((int) pos.x );
        }
        
        if( pos.x < 0.0f )
        {
            pos.x = pos.x + ((int) pos.x ) + 1.0f;
        }
        
        if( pos.y > 1.0f )
        {
            pos.y = pos.y - ((int) pos.y );
        }
        
        if( pos.y < 0.0f )
        {
            pos.y = pos.y + ((int) pos.y ) + 1.0f;
        }
        
        m_renderer.material.mainTextureOffset = pos;
    }
    
    protected void Update()
    {
		float dt = Timers.Instance.Game.GetDeltaTime();
        if( !m_renderer )
        {
            return;
        }

        Vector2 pos = m_renderer.material.mainTextureOffset;
        pos.x += m_scrollVelocity * m_ScrollFactor * dt;
		pos.y += m_scrollVerticalVelocity * m_VerticalScrollFactor * dt;
        
        if( pos.x > 1.0f )
        {
            pos.x = pos.x - ((int) pos.x );
        }
        
        if( pos.x < 0.0f )
        {
            pos.x = pos.x + ((int) pos.x ) + 1.0f;
        }

		if( pos.y > 1.0f )
		{
			pos.y = pos.y - ((int) pos.y );
		}
		
		if( pos.y < 0.0f )
		{
			pos.y = pos.y + ((int) pos.y ) + 1.0f;
		}
        
        m_renderer.material.mainTextureOffset = pos;    
    }

    protected void Start()
    {
        m_renderer = GetComponent< Renderer >() as Renderer;
        if( !m_renderer )
        {
            DebugUtil.LogError( "Renderer not found for this GameObject." );
            enabled = false;
            return;
        }
    }
    
    [SerializeField] protected float m_ScrollFactor         = 1.0f;	// How much of the avatar velocity is considered?
	[SerializeField] protected float m_VerticalScrollFactor = 1.0f; // How much of the avatar vertical velocity is considered?
    private float m_scrollVelocity         					= 0.0f;
	private float m_scrollVerticalVelocity 					= 0.0f;
    private Renderer m_renderer            					= null;
}
