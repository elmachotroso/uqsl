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
using System.Collections.Generic;
using QsLib;

/// <summary>
/// SpriteAnimationController allows you to make use of the classic 2D sprite
/// animation using a spritesheet applied on a material. Here you define the
/// frames for a specific animation and how fast it can be.
/// </summary>
public class SpriteAnimationController : MonoBehaviour
{
    // Retrieve the current sequence number being played.
    public int GetCurrentSequence()
    {
        return m_CurrentAnimationSequence;
    }

    // Set/change the sequence being played by the name and its starting frame.
    public void SetSequence( string name, int startFrameNumber = 0 )
    {
        int id = GetSequenceId( name );
        SetSequence( id, startFrameNumber );
    }

    // Set/change the sequence being played by the id and its starting frame.
    public void SetSequence( int id, int startFrameNumber = 0 )
    {
        if( id >= 0 && id < m_Animations.Count )
        {
            m_CurrentAnimationSequence = id;
            m_CurrentAnimationFrame = startFrameNumber;
            m_FrameTimeAccum = 0.0f;
        }
    }

    // Set/change the speed of the animation sequences.
    public void SetDelayFactor( float delayFactor )
    {
        m_DelayFactor = delayFactor;
    }
    
    protected void Start()
    {
        m_renderer = gameObject.GetComponent< Renderer >() as Renderer;
        if( !SetupAndValidateSpriteSheet() )
        {
            enabled = false;
            return;
        }
    }
    
    protected void Update()
    {
        float dt = Time.deltaTime;
        m_FrameTimeAccum += dt;
        GetNextFrame();
        SetSpriteSheetOffsetByFrame(
            m_Animations[ m_CurrentAnimationSequence ].sequence[ m_CurrentAnimationFrame ].spriteFrame
            );
    }
    
    private void GetNextFrame()
    {
        // Get current frame info
        SpriteAnimationSequence sequence = m_Animations[ m_CurrentAnimationSequence ];
        SpriteFrameInfo spriteFrameInfo = sequence.sequence[ m_CurrentAnimationFrame ];
        float frameDelay = spriteFrameInfo.frameDelay;
        
        do
        {
            frameDelay = spriteFrameInfo.frameDelay * m_DelayFactor;
            if( frameDelay <= 0.0f )
            {
                frameDelay = 0.0001f;
            }
            // Just retain frame if time not expired on current frame.
            if( m_FrameTimeAccum < frameDelay )
            {
                break;
            }
            
            m_FrameTimeAccum -= frameDelay;
            
            // Look at next frame.
            int nextFrame = m_CurrentAnimationFrame + 1;
            if( nextFrame >= sequence.sequence.Count )
            {
                if( sequence.loop )
                {
                    nextFrame = 0;
                }
                else
                {
                    nextFrame = sequence.sequence.Count - 1;
                }
            }
            
            // Upate next frame as current frame.
            m_CurrentAnimationFrame = nextFrame;
            spriteFrameInfo = sequence.sequence[ m_CurrentAnimationFrame ];
            
            // keep on doing this until we find the frame that the accumulated time is not expired.
        } while( m_FrameTimeAccum >= frameDelay );
        
        return;
    }
    
    private void SetSpriteSheetOffsetByFrame( int frame )
    {
        if( frame >= 0 && frame < m_TotalFrames )
        {
            if( m_renderer )
            {
                float xoffset = m_SpriteSheetCellWidthRatio * ( frame % m_FrameColumns );
                float yoffset = 1.0f - ( m_SpriteSheetCellHeightRatio * ( ( frame / m_FrameColumns ) + 1 ) );
                m_renderer.material.mainTextureOffset = new Vector2(
                    xoffset, yoffset );
            }
        }
    }
    
    private int GetSequenceId( string name )
    {
        for( int i = 0; i < m_Animations.Count; ++i )
        {
            if( m_Animations[ i ].name.Equals( name ) )
            {
                return i;
            }
        }
        
        return -1;
    }
    
    private bool SetupAndValidateSpriteSheet()
    {
        MeshRenderer rend = GetComponent< MeshRenderer >() as MeshRenderer;
        if( rend == null || rend.material == null || rend.material.mainTexture == null )
        {
            Debug.LogError( "This script requires a renderer and a texture on a material." );
            return false;
        }
        
        if( m_FrameColumns <= 0 || m_FrameRows <= 0 )
        {
            DebugUtil.LogError( "Frame row and column counts should be greater than 0." );
            return false;
        }
        
        m_SpriteSheetWidth = rend.material.mainTexture.width;
        m_SpriteSheetHeight = rend.material.mainTexture.height;
        
        if( m_SpriteSheetWidth <= 0 || m_SpriteSheetHeight <= 0 )
        {
            DebugUtil.LogError( "Sprite Sheet Texture width and height should be greater than 0." );
            return false;
        }
        
        m_SpriteSheetCellWidth = m_SpriteSheetWidth / m_FrameColumns;
        m_SpriteSheetCellHeight = m_SpriteSheetHeight / m_FrameRows;
        m_SpriteSheetCellWidthRatio = m_SpriteSheetCellWidth / (float) m_SpriteSheetWidth;
        m_SpriteSheetCellHeightRatio = m_SpriteSheetCellHeight / (float) m_SpriteSheetHeight;
        m_TotalFrames = m_FrameColumns * m_FrameRows;
        
        rend.material.mainTextureScale = new Vector2(
            m_SpriteSheetCellWidthRatio,
            m_SpriteSheetCellHeightRatio );
        
        if( m_Animations == null || m_CurrentAnimationSequence >= m_Animations.Count )
        {
            m_CurrentAnimationSequence = m_Animations.Count - 1;
        }
        
        if( m_Animations == null || m_Animations[ m_CurrentAnimationSequence ].sequence == null
           || m_CurrentAnimationFrame >= m_Animations[ m_CurrentAnimationSequence ].sequence.Count )
        {
            m_CurrentAnimationFrame = 0;
        }
        
        return true;
    }

    [System.Serializable]
    protected struct SpriteFrameInfo
    {
        public int spriteFrame;
        public float frameDelay;
    }
    
    [System.Serializable]
    protected struct SpriteAnimationSequence
    {
        public string name;
        public bool loop;
        public List< SpriteFrameInfo > sequence;
    }

    [SerializeField] protected List< SpriteAnimationSequence > m_Animations = null;	// The list animation sequences of the sprite and its details.
    [SerializeField] protected int m_FrameColumns                           = 1;	// How many columns are there in the spritesheet assigned?
    [SerializeField] protected int m_FrameRows                              = 1;	// How many rows are there in the spritesheet assigned?
    [SerializeField] protected int m_CurrentAnimationFrame                  = 0;	// At what frame is the current animation sequence playing?
    [SerializeField] protected int m_CurrentAnimationSequence               = 0;	// What animation sequence is currently playing?
    [SerializeField] protected float m_DelayFactor                          = 1.0f; // A "timescale" factor applied on top of the delay factors of the animation sequences.
    private int m_SpriteSheetWidth                         = 1;
    private int m_SpriteSheetHeight                        = 1;
    private int m_TotalFrames                              = 0;
    private int m_SpriteSheetCellWidth                     = 1;
    private int m_SpriteSheetCellHeight                    = 1;
    private float m_SpriteSheetCellWidthRatio              = 1.0f;
    private float m_SpriteSheetCellHeightRatio             = 1.0f;
    private float m_FrameTimeAccum                         = 0.0f;
    private Renderer m_renderer                            = null;
}
