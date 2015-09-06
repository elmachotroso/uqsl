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
using System.Collections;
using QsLib;


/// <summary>
/// In 2D, a trick to do some sort of 3D layering ability is to use Parallax scrolling.
/// This system enables that trick.
/// Parallax scene manages gameobjects with ParallaxObject scripts attached. They are
/// assigned in layers array and propagates the scroll velocity to these parallax
/// objects.
/// Usage: Assign gameobjects with ParallaxObject scripts in the Layers array and then
/// call the SetScrollVelocity to see the scroll applied to the parallax objects.
/// </summary>
public class ParallaxScene : Singleton< ParallaxScene >
{
    // Sets the scroll velocity of the ParallaxScene and to its ParallaxObjects.
    // 0.0f velocity is a stop, > 0.0f is rightwards, and < 0.0f is leftward.
    public void SetScrollVelocity( float velocity )
    {
       m_Velocity = velocity;
    }

	// Sets the vertical scroll velocity of the ParallaxScene and to its ParallaxObjects.
	// 0.0f velocity is a stop, > 0.0f is upwards, and < 0.0f is downward.
	public void SetVerticalScrollVelocity( float verticalVelocity )
	{
		m_VerticalVelocity = verticalVelocity;
	}
    
    // Manually scroll the parallax object by x and y distances.
    public void Scroll( float xdistance, float ydistance )
    {
        for( int index = 0; index < m_Layers.Length; ++index )
        {
            LayerInfo layer = m_Layers[ index ];
            ParallaxObject script = layer.GetParallaxObject();
            if( script != null )
            {
                script.Scroll( xdistance, ydistance );
            }
        }
    }

    protected ParallaxScene() {} // for singleton
    
    protected void Start()
    {
        CheckValidLayers();
        SetScrollVelocity( m_Velocity );
    }
    
    protected void Update()
    {
        if( m_Layers == null )
        {
            enabled = false;
            return;
        }
        
        for( int index = 0; index < m_Layers.Length; ++index )
        {
            LayerInfo layer = m_Layers[ index ];
            ParallaxObject script = layer.GetParallaxObject();
            if( script != null )
            {
                script.SetScrollVelocity( m_Velocity );
				script.SetVerticalScrollVelocity( m_VerticalVelocity );
            }
        }
    }
    
    private bool CheckValidLayers()
    {
        if( m_Layers == null )
        {
            enabled = false;
            return false;
        }
        
        for( int index = 0; index < m_Layers.Length; ++index )
        {
            LayerInfo layer = m_Layers[ index ];
            if( layer.m_ObjectToRender != null )
            {
                if( !layer.AcquireParallaxObject() )
                {
                    DebugUtil.LogError( "GameObjects of ParallaxScene must have ParallaxObject script as components!" );
                    enabled = false;
                    return false;
                }
            }
        }
        
        return true;
    }
    
    [SerializeField] public LayerInfo[] m_Layers            = null;    // Layers containing the parallax object pointers.
	[SerializeField] private float m_Velocity               = 0.0f;    // Velocity of the parallax scene.
	[SerializeField] private float m_VerticalVelocity       = 0.0f;    // Vertical Velocity of the parallax scene.
	
	/// <summary>
	/// A simple structure to accomodate a list of parallax objects associated with
	/// the ParallaxScene.
	/// </summary>
	[System.Serializable]
	public class LayerInfo
	{
		public ParallaxObject GetParallaxObject()
		{
			return m_Script;
		}
		
		public bool AcquireParallaxObject()
		{
			if( m_ObjectToRender == null )
			{
				return false;
			}
			
			m_Script = m_ObjectToRender.GetComponent< ParallaxObject >() as ParallaxObject;
			if( m_Script == null )
			{
				return false;
			}
			
			return true;
		}
		
		public GameObject m_ObjectToRender        = null;
		private ParallaxObject m_Script           = null;
	}
}
