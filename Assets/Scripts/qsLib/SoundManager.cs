/*
Unity Quickstart Library (qsLib)
http://www.andreivictor.net/uqsl/
Last Updated: 09/04/2015

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
using QsLib;

/// <summary>
/// Sound manager is a powerful audio class that handles the playback of sound effects (sfx) and
/// background music (bgm) in a unified and organized manner. The aim is to simplify and optimize
/// the use of audio resources and playback all throughout the game.
/// The optimization strategies of the sound manager is the pooling of pre-determined audio data
/// and pre-load them into memory. These audio data are singular but shared to conserve memory.
/// Also, playback is optimized by using a limited number of buffers. For background music (bgm),
/// two buffers used to seamlessly transition playback from one music to another. As for sound
/// effects (sfx), an n-amount of (default is 3) buffers are reserved for each sound effect
/// audio data to prevent bad playback whenever sound effects are spammed.
/// Some few API for volume control and playback effect are also exposed.
/// Usage:
/// 1. Attached SoundManager to a application-wide GameObject.
/// 2. Apply the desired settings on the SoundManager component.
/// 3. Use SoundManager::PrepareSound to the audio file you want to use
/// 4. Use SoundManager PlaySfx or PlayBgm or other playback API.
/// 5. Use SoundManager::UnprepareSound when done using the specific audio file.
///
/// Notes:
/// 1. Android Audio Bridge (AdnAudioBridge) is a JNI-based plugin to help address
///    lag playback on Android devices.
/// 2. SfxOverBgm feature is where the BGM sound playback volume is changed (usually
///    muted) to isolate Sound effects playback. After the sound effect playback, the
///    original Bgm volume is restored.
///
/// Limitations:
/// 1. Cannot simulate sounds in 3D space since the audio sources will always be from the
///    location of the GameObject of the SoundManager.
/// </summary>
public class SoundManager : Singleton< SoundManager >
{
	// A helper function that creates and returns a playlist from a given string array of sound ids.
	public List< BgmPlayListTrack > CreatePlaylist( string[] ids, float crossfadeTime = 0.0f )
	{
		if( ids == null )
		{
			return null;
		}
		
		List< BgmPlayListTrack > playlist = new List< BgmPlayListTrack >();
		foreach( string id in ids )
		{
			if( m_library.ContainsKey( id ) )
			{
				BgmPlayListTrack track = new BgmPlayListTrack();
				track.m_clip = m_library[ id ].AudioClipFile; 
				track.m_crossfadeTime = crossfadeTime;
				playlist.Add( track );
			}
		}
		
		return playlist;
	}

    // Set the master volume for both bgm and sfx. [0.0, 1.0]
    public void SetVolume( float value )
    {
        value = Math.Clamp0( value );
        m_MasterVolume = value;
    }

    // Set the bgm volume relative to the master volume. [0.0, 1.0]
    public void SetBgmVolume( float value )
    {
        value = Math.Clamp0( value );
        m_BgmVolume = value;
    }

    // Set the sfx volume relative to the master volume. [0.0, 1.0f]
    public void SetSfxVolume( float value )
    {
        value = Math.Clamp0(value);
        m_SfxVolume = value;
    }
    
    // Retrieve the master volume value.
    public float GetVolume()
    {
        return m_MasterVolume;
    }

	// Retrieve the raw bgm volume.
	public float GetBgmVolume()
	{
		return m_BgmVolume;
	}

	// Retrieve the raw sfx volume.
	public float GetSfxVolume()
	{
		return m_SfxVolume;
	}

    // Retrieve the final bgm volume relative to the master volume.
    public float GetBgmVolumeFinal()
    {
        return Math.Clamp0( m_BgmVolume * m_SfxOverBgmVolume * m_MasterVolume );
    }

    // Retrieve the final sfx volume relative to the sfx volume.
    public float GetSfxVolumeFinal()
    {
        return Math.Clamp0( m_SfxVolume * m_MasterVolume );
    }

    // Retrieve the GameObject where the audio sources are placed.
    public GameObject SoundSourcesObject
    {
        get
        {
            return m_child;
        }
    }

    // Returns true if the AndroidAudioBridge is going to be used for Sfx.
    public bool IsUsingAndroidAudioBridgeForSfx()
    {
        return m_UseAndroidAudioBridgeForSfx;
    }
    
    // Prepares/loads the desired sound in memory. The id is the string name used to identify their corresponding
    // audio files. IsBgm is wheter the files are to be used as bgm. is3D and stream are flags to identify
    // how the audio files will be used.
    public void PrepareSound( string[] ids, string[] files, bool isBgm, bool is3D, bool stream )
    {
        if( ids.Length != files.Length )
        {
            DebugUtil.LogError( "PrepareSound: Arrays of ids and files are not of the same size." );
            return;
        }
        
        for( int index = 0; index < ids.Length; ++index )
        {
            PrepareSound( ids[ index ], files[ index ], isBgm, is3D, stream );
        }
    }

    // Prepares/loads the desired sound in memory. The id is the string name used to identify their corresponding
    // audio files. IsBgm is wheter the files are to be used as bgm. is3D and stream are flags to identify
    // how the audio files will be used.
    public void PrepareSound( string id, string file, bool isBgm, bool is3D, bool stream )
    {
        if( m_library.ContainsKey( id ) )
        {
            DebugUtil.LogWarning( string.Format( "PrepareFile {0} is already loaded in the library. Increasing reference count instead.", id ) );
            m_library[ id ].m_refCount++;
            return;
        }

        string soundFile = GetSoundFileForPlatform( file );

        if( Application.platform != RuntimePlatform.Android && !File.Exists( soundFile ) )
        {
            DebugUtil.LogError( string.Format( "File {0} is illegal or non-existent.", soundFile ) );
        }

        AudioFileHandler handler = new AudioFileHandler( soundFile, isBgm, m_SfxBufferSize );
        m_library.Add( id, handler );
        StartCoroutine( LoadAudioClipFromFile( handler, is3D, stream ) );
    }
    
    // The unprepare sound is to unload from memory the audio file that corresponds to given id(s).
    public void UnprepareSound( string[] ids )
    {
        foreach( string id in ids )
        {
            UnprepareSound( id );
        }
    }

    // The unprepare sound is to unload from memory the audio file that corresponds to given id(s).
    public void UnprepareSound( string id )
    {
        if( m_library.ContainsKey( id ) )
        {
            AudioFileHandler handler = m_library[ id ];
            if( handler.m_refCount <= 1 )
            {
                Stop( id );
                if( Application.platform == RuntimePlatform.Android && m_UseAndroidAudioBridgeForSfx && !handler.m_isBgm )
                {
                    AdnAudioBridge.UnloadSound( handler.m_AudioBridgeId );
                }
                m_library.Remove( id );
            }
            else
            {
                m_library[ id ].m_refCount--;
            }
        }
        else
        {
            DebugUtil.LogWarning( string.Format( "UnprepareSound {0} doesn't exist to unload.", id ) );
        }
    }
    
    // Returns true if the specified id exists and is fully loaded (PrepareSound is completely finished).
    public bool IsReadyForPlayback( string id )
    {
		if( !m_library.ContainsKey( id ) )
		{
			DebugUtil.LogWarning( string.Format( "IsReadyForPlayback: {0} doesn't exist.", id ) );
			return false;
		}
		
		AudioFileHandler handler = m_library[ id ];
		return handler.m_isReadyForPlayback;
    }

    // Plays a prepared sfx of the specified id.
    public void PlaySfx( string id, float delay = 0.0f, bool loop = false, bool isSfxOverBgm = false )
    {
        if( m_library == null )
        {
            DebugUtil.LogError( "Cannot play sfx without library!" );
            return;
        }

        if( m_library.ContainsKey( id ) )
        {
            AudioFileHandler handler = m_library[ id ];
            if( handler == null || handler.m_isBgm )
            {
                DebugUtil.LogError( "Handler null or is not sfx: " + handler.m_fileInfo );
                return;
            }
            
			if( !handler.m_isReadyForPlayback )
			{
				DebugUtil.LogWarning(
				    string.Format( "PlaySfx: Cannot play {0} because it is not fully loaded yet. Try again later.",
				    id ) );
			}

            bool isAdnBridgeSfx = Application.platform == RuntimePlatform.Android && m_UseAndroidAudioBridgeForSfx && !handler.m_isBgm;

            if( !isAdnBridgeSfx && handler.m_sfx == null )
            {
                DebugUtil.LogError( "PlaySfx m_sfx of handler is null!" );
                return;
            }

            if( isSfxOverBgm )
            {
                float clipLength = handler.m_audioLength;
                float sfxOverBgmNewValue = Time.time + clipLength + delay;
                if( sfxOverBgmNewValue > m_SfxOverBGmVolumeEndTime )
                {
                    m_SfxOverBGmVolumeEndTime = sfxOverBgmNewValue;
                }
            }

            if( isAdnBridgeSfx )
            {
                handler.m_LastAudioBridgeStream = AdnAudioBridge.PlaySound( handler.m_AudioBridgeId, GetSfxVolumeFinal() );
            }
            else
            {
                handler.m_sfx.Play( delay, loop );
            }
        }
        else
        {
        	DebugUtil.LogWarning( string.Format( "PlaySfx: No such id {0} exists in SoundManager.", id ) );
        }
    }
    
    // Plays a bgm playlist from the collection of ids. 
    public void PlayBgmPlaylist( string[] ids, bool loop = true, float crossfadeTime = 0.0f )
    {
        List< BgmPlayListTrack > playlist = CreatePlaylist( ids, crossfadeTime );
        
        if( playlist != null && playlist.Count > 0 )
        {
            m_bgm.PlayPlaylist( playlist, loop );
        }
    }
    
    // Plays a bgm playlist from the collection of bgm playlist tracks. 
    public void PlayBgmPlaylist( List< BgmPlayListTrack > playlist, bool loop = true )
    {
        m_bgm.PlayPlaylist( playlist, loop );
    }

    // Plays a single bgm of the specified id.
    public void PlayBgm( string id, bool loop = true, float crossfadeTime = 0.0f )
    {
        if( m_library.ContainsKey( id ) )
        {
        	AudioFileHandler handler = m_library[ id ];
        	if( handler.m_isReadyForPlayback )
        	{
				m_bgm.Play( handler.AudioClipFile, loop, crossfadeTime );
        	}
            else
            {
            	DebugUtil.LogWarning(
            		string.Format( "PlayBgm: Cannot play {0} because it is not fully loaded yet. Try again later.",
            		id ) );
            }
        }
		else
		{
			DebugUtil.LogWarning( string.Format( "PlayBgm: No such id {0} exists in SoundManager.", id ) );
		}
    }
    
    // Stops playback of any bgm if any.
    public void StopBgm()
    {
        m_bgm.Stop();
    }

    // Stops playback of any audio source with the specified id.
    public void Stop( string id )
    {
        if( m_library.ContainsKey( id )  )
        {
            AudioFileHandler handler = m_library[ id ];
            if( handler != null )
            {
                if( !handler.m_isBgm )
                {
                    if( Application.platform == RuntimePlatform.Android && m_UseAndroidAudioBridgeForSfx )
                    {
                        AdnAudioBridge.StopSound( handler.m_AudioBridgeId );
                    }
                    else if( handler.m_sfx != null )
                    {
                        m_library[ id ].m_sfx.Stop();
                    }
                }
                else
                {
                    m_bgm.Stop( handler.AudioClipFile );
                }
            }
        }
    }

    // Returns true if the specified audio clip of the specified id is playing.
    public bool IsPlaying( string id )
    {
        if( m_library.ContainsKey( id ) )
        {
            AudioFileHandler handler = m_library[ id ];
            if( handler != null )
            {
                if( !handler.m_isBgm && handler.m_sfx != null )
                {
                    return m_library[ id ].m_sfx.IsPlaying();
                }
                else if( handler.m_isBgm )
                {
                    return m_bgm.IsBgmPlaying( handler.AudioClipFile );
                }
            }
        }

        return false;
    }
    
    // Returns true if the indicated playlist is currently playing. Note that this
    // temporarily constructs a BGM playlist based on the ids and crossfade time
    // indicated. This means that the id and crossfade time is part of the matching.
    // A different crossfade time is not considered the same playlist. It is
    // recommended to use the other IsPlayingBgmPlaylist function over this one.
    public bool IsPlayingBgmPlaylist( string[] ids, float crossfadeTime = 0.0f )
    {
    	List< BgmPlayListTrack > playlist = CreatePlaylist( ids, crossfadeTime );
    	return playlist != null && IsPlayingBgmPlaylist( ref playlist );
    }
    
    // Returns true if the specified playlist is the current playlist playing.
    public bool IsPlayingBgmPlaylist( ref List< BgmPlayListTrack > playlist )
    {
    	return m_bgm != null && m_bgm.IsBgmPlaylistPlaying( ref playlist );
    }

    protected SoundManager() { } // Singleton

    protected void Awake()
    {
        // make child gameobject
        m_child = new GameObject( "SoundManager: Sound Sources" );
        if (m_child == null)
        {
            DebugUtil.LogError( "Could not create child object for the SoundManager." );
            enabled = false;
        }
        ObjectHelpers.CopyTransform(gameObject.transform, m_child.transform);
        m_child.transform.parent = gameObject.transform;
        
        m_library = new Dictionary< string, AudioFileHandler >();
        m_bgm = new BgmPlayer();

        if( m_UseAndroidAudioBridgeForSfx )
        {
            AdnAudioBridge.Initialize( m_SfxBufferSize );
        }
    }

    protected void Start()
    {
        SaveManager.Instance.notifier.AddSubscriber( LoadSaveCallback );
    }

    protected void Update()
    {
        float timeNow = Time.time;
        if( timeNow > m_SfxOverBGmVolumeEndTime )
        {
            m_SfxOverBgmVolume += Time.deltaTime * m_SfxOverBgmRate;
        }
        else
        {
            m_SfxOverBgmVolume -= Time.deltaTime * m_SfxOverBgmRate;
        }
        m_SfxOverBgmVolume = Mathf.Clamp( m_SfxOverBgmVolume, m_SfxOverBgmMinVolume, 1.0f );

        ApplySfxVolumeChanges();
        
        if( m_bgm != null )
        {
            m_bgm.Update();
        }
    }

    protected void OnDestroy()
    {
        SaveManager.Instance.notifier.RemoveSubscriber( LoadSaveCallback );
    }

    protected void LoadSaveCallback( string message, object param )
    {
        SaveManager saveMgr = SaveManager.Instance;
        if( message == "SaveManager.OnSaveRequest" )
        {
            saveMgr.Set( m_bgmSettingVariableName, GetBgmVolume() );
            saveMgr.Set( m_sfxSettingVariableName, GetSfxVolume() );
        }
        else if( message == "SaveManager.OnLoadRequest" )
        {
            // Do nothing! This is just a request to load BEFORE loading!
        }
        else if( message == "SaveManager.OnNewDataLoaded" )
        {
            float bgmVolume = 0.0f;
            float sfxVolume = 0.0f;

            saveMgr.Get( m_bgmSettingVariableName, ref bgmVolume );
            saveMgr.Get( m_sfxSettingVariableName, ref sfxVolume );

            SetBgmVolume( bgmVolume );
            SetSfxVolume( sfxVolume );
        }
    }

    private IEnumerator LoadAudioClipFromFile( AudioFileHandler handler, bool is3D, bool stream )
    {
        string[] parts = handler.m_fileInfo.Split('\\');

        string path = "";
        string adnPath = "";
        if( Application.platform == RuntimePlatform.Android )
        {
            path = handler.m_fileInfo;
            adnPath = handler.m_fileInfo;
            if( m_UseAndroidAudioBridgeForSfx && !handler.m_isBgm )
            {
                // Note: For AdnAudioBridge, we only need the path relative to the StreamingAssets folder.

                // remove streaming path at the beginning
                if( adnPath.Contains( AppDir.StreamPath ) )
                {
                    adnPath = adnPath.Remove( 0, AppDir.StreamPath.Length );
                }

                // remove / or \ at the beginning if any.
                if( adnPath.StartsWith( "/" ) || adnPath.StartsWith( "\\" ) )
                {
                    adnPath = adnPath.Remove( 0, 1 );
                }
            }
        }
        else
        {
            path = "file://" + handler.m_fileInfo;
        }

        if( Application.platform == RuntimePlatform.Android && m_UseAndroidAudioBridgeForSfx && !handler.m_isBgm )
        {
            handler.m_AudioBridgeId = AdnAudioBridge.LoadSound( adnPath );
        }

        // Note: We still load audio clip even if we use AdnAudioBridge because we need to get the length of the sound.
        // We will unload it later once we receive this information.
        WWW www = new WWW( path );

        while( www.progress < 1 || !www.audioClip.isReadyToPlay )
        {
            yield return www;
        }

        AudioClip clip = www.GetAudioClip( is3D, stream );
        if( clip == null )
        {
            DebugUtil.LogError( string.Format( "Failed to load AudioClip: {0}", path ) );
        }
        else
        {
            DebugUtil.Log( string.Format( "Successfully loaded AudioClip: {0}", path ) );
            clip.name = parts[ parts.Length - 1 ];
            handler.AudioClipFile = clip;
            handler.m_audioLength = clip.length;
            handler.m_isReadyForPlayback = true;

            if( !handler.m_isBgm )
            {
                if( Application.platform == RuntimePlatform.Android && m_UseAndroidAudioBridgeForSfx )
                {
                    // NOTE: We unload the audio clip file from memory because we won't need it when using AdnAudioBridge
                    handler.AudioClipFile = null;
                }
                else
                {
                    handler.PrepareSfxBuffers( GetSfxVolumeFinal() );
                }
            }
        }
    }

    private void ApplySfxVolumeChanges()
    {
        foreach( AudioFileHandler handle in m_library.Values )
        {
            if( handle != null )
            {
                if( !handle.m_isBgm )
                {
                    if( Application.platform == RuntimePlatform.Android && m_UseAndroidAudioBridgeForSfx )
                    {
                        AdnAudioBridge.SetVolume( handle.m_AudioBridgeId, GetSfxVolumeFinal() );
                    }
                    else if( handle.m_sfx != null )
                    {
                        handle.m_sfx.SetVolume( GetSfxVolumeFinal() );
                    }
                }
            }
        }
    }

    private string GetSoundFileForPlatform( string file )
    {
        #if UNITY_ANDROID || UNITY_IPHONE
            return Path.ChangeExtension( file, "mp3" );
        #elif UNITY_STANDALONE || UNITY_WEBPLAYER
            return Path.ChangeExtension( file, "ogg" );
        #else
            return file;
        #endif
    }

    [SerializeField] protected int m_SfxBufferSize                              = 3;        // The number of buffers to create for sfx playback.
    [SerializeField] protected float m_SfxOverBgmMinVolume                      = 0.3f;     // The minimum volume to tone down the BGM when an sfx over bgm is played.
    [SerializeField] protected float m_SfxOverBgmRate                           = 1.0f;     // How fast is the change of BGM volume when sfx over bgm is played.
    [SerializeField] protected bool m_UseAndroidAudioBridgeForSfx               = true;     // (Android only) enable to use the native audio playback for 0 delay playback!
   
    private float m_MasterVolume                                                = 1.0f;
    private float m_BgmVolume                                                   = 1.0f;
    private float m_SfxOverBgmVolume                                            = 1.0f;
    private float m_SfxOverBGmVolumeEndTime                                     = 0.0f;
    private float m_SfxVolume                                                   = 1.0f;
    private Dictionary< string, AudioFileHandler > m_library                    = null;
    private BgmPlayer m_bgm                                                     = null;
    private GameObject m_child                                                  = null;
        
    private string m_bgmSettingVariableName                                     = "bgmVolume";
    private string m_sfxSettingVariableName                                     = "sfxVolume";
    
	/// <summary>
	/// This a structure for a single playlist track. Note that a collection of this class is considered a playlist.
	/// </summary>
	public class BgmPlayListTrack
	{		
		public AudioClip m_clip          = null; // The actual audio clip to be used.
		public float m_crossfadeTime     = 0.0f; // The amount of time needed to perform the crossfading.
	}
    
	/// <summary>
	/// A helper class for the SoundManager. This class does the managing and playback of music. It contains two
	/// audio sources to simulate the next and current playing track. A crossfader controls the volume of both
	/// audio sources.
	/// </summary>
	public class BgmPlayer
	{
		// This allows to play a whole playlist. It can be loop the whole playlist or not.
		public void PlayPlaylist( List< BgmPlayListTrack > playlist, bool loop = true )
		{
			m_playlist = playlist;
			m_trackNumber = -1;
			m_isLooping = loop;
			PlayNextTrack();
		}
		
		// This plays a single music clip for playback. (This is implemented as playing
		// a playlist with a single track inside)
		public void Play( AudioClip clip, bool loop = true, float crossfadeTime = 0.0f )
		{
			// create single track playlist
			BgmPlayListTrack track = new BgmPlayListTrack();
			track.m_clip = clip;
			track.m_crossfadeTime = crossfadeTime;
			List< BgmPlayListTrack > playlist = new List< BgmPlayListTrack >();
			playlist.Add( track );
			m_playlist = playlist;
			m_trackNumber = -1;
			m_isLooping = loop;
			PlayNextTrack();
		}
		
		// This stops the playback of the bgm player.
		public void Stop()
		{
			m_isStopped = true;
			m_LeftSource.Stop();
			m_RightSource.Stop();
		}
		
		// Stops playback of given audio clip.
		public void Stop( AudioClip clip )
		{
			if( m_LeftSource.clip == clip && m_LeftSource.isPlaying )
			{
				m_LeftSource.Stop();
				m_LeftSource.clip = null;
			}
			
			if( m_RightSource.clip == clip && m_RightSource.isPlaying )
			{
				m_RightSource.Stop();
				m_RightSource.clip = null;
			}
		}
		
		// Returns true if the specified audio clip is being played.
		public bool IsBgmPlaying( AudioClip clip )
		{
			if( m_LeftSource && m_LeftSource.isPlaying && m_LeftSource.clip == clip )
			{
				return true;
			}
			
			if( m_RightSource && m_RightSource.isPlaying && m_RightSource.clip == clip )
			{
				return true;
			}
			
			return false;
		}
		
		// Returns true when the provided playlist is currently playing.
		public bool IsBgmPlaylistPlaying( ref List< BgmPlayListTrack > playlist )
		{
			if( !IsPlaylistIdentical( ref playlist, true ) )
			{
				return false;
			}
			
			if( playlist.Count == m_playlist.Count )
			{
				foreach( BgmPlayListTrack track in playlist )
				{
					if( IsBgmPlaying( track.m_clip ) )
					{
						return true;
					}
				}	
			}
						
			return false;
		}
		
		// Constructor
		public BgmPlayer()
		{
			GameObject go = SoundManager.Instance.SoundSourcesObject;
			m_LeftSource = go.AddComponent< AudioSource >();
			m_LeftSource.playOnAwake = false;
			m_LeftSource.volume = 1.0f;
			m_RightSource = go.AddComponent< AudioSource >();
			m_RightSource.playOnAwake = false;
			m_RightSource.volume = 0.0f;
		}
		
		// General update method invoked by the class using BgmPlayer.
		public void Update()
		{
			if( m_isStopped )
			{
				return;
			}    
			
			if( m_hasChangeRequest )
			{
				float dt = Time.deltaTime;
				m_BgmCrossfader += dt / m_CrossfadeTime;
				if( m_BgmCrossfader > 1.0f )
				{
					m_BgmCrossfader = 1.0f;
				}
			}
			
			float bgmFinalVolume = Math.Clamp0( SoundManager.Instance.GetBgmVolumeFinal() );
			
			m_RightSource.volume = m_BgmCrossfader * bgmFinalVolume;
			m_LeftSource.volume = ( 1.0f - m_BgmCrossfader ) * bgmFinalVolume;
			
			
			if( m_LeftSource.volume == 0.0f && m_LeftSource.isPlaying )
			{
				m_LeftSource.Stop();
			}
			else if( m_LeftSource.volume > 0.0f && !m_LeftSource.isPlaying )
			{
				m_LeftSource.Play();
			}
			
			if( m_RightSource.volume == 0.0f && m_RightSource.isPlaying )
			{
				m_RightSource.Stop();
			}
			else if( m_RightSource.volume > 0.0f && !m_RightSource.isPlaying )
			{
				m_RightSource.Play();
			}
			
			// reached transition
			if( m_BgmCrossfader >= 1.0f )
			{
				AudioSource temp = m_RightSource;
				m_RightSource = m_LeftSource;
				m_LeftSource = temp;
				m_BgmCrossfader = 0.0f;
				m_hasChangeRequest = false;
			}
			
			if( !m_isTrackAboutToEnd && m_isTrackAboutToEnd != IsTrackAboutToEnd() )
			{
				DebugUtil.Log( "Bgm track is about to end, playing next track." );
				PlayNextTrack();
			}
			m_isTrackAboutToEnd = IsTrackAboutToEnd();
		}
		
		private bool IsTrackAboutToEnd()
		{
			if( m_playlist.Count == 0 )
			{
				return false;
			}
			
			int nextTrack = m_trackNumber + 1;
			if( m_isLooping && nextTrack >= m_playlist.Count )
			{
				nextTrack = 0;
			}
			
			float nextTrackCrossfadeTime = 0.0f;
			if( nextTrack < m_playlist.Count )
			{
				nextTrackCrossfadeTime = m_playlist[ nextTrack ].m_crossfadeTime;
			}
			
			if( m_trackNumber >= m_playlist.Count )
			{
				return false;
			}
			
			if( m_playlist[ m_trackNumber ] == null || m_playlist[ m_trackNumber ].m_clip == null )
			{
				DebugUtil.LogWarning( string.Format( "The tracknumber {0} has null clips!!!", m_trackNumber ) );
				return true;
			}
			
			AudioClip clip = m_playlist[ m_trackNumber ].m_clip;
			
			return ( clip.length - nextTrackCrossfadeTime ) >= m_LeftSource.time;
		}
		
		private void PlayNextTrack()
		{
			if( !m_isLooping && m_trackNumber >= m_playlist.Count )
			{
				return;
			}
			
			m_trackNumber++;
			if( m_isLooping && m_trackNumber >= m_playlist.Count )
			{
				m_trackNumber = 0;
			}
			
			if( m_trackNumber < m_playlist.Count )
			{
				ActualPlay( m_playlist[ m_trackNumber ] );
			}
		}
		
		private void SwapSources()
		{
			AudioSource temp = m_RightSource;
			m_RightSource = m_LeftSource;
			m_LeftSource = temp;
		}
		
		private void ActualPlay( BgmPlayListTrack track )
		{
			m_RightSource.clip = track.m_clip;
			m_RightSource.loop = false; // we do not use looping feature of sound source, instead we manually replay the bgm
			m_CrossfadeTime = track.m_crossfadeTime;
			m_BgmCrossfader = 0.0f;
			m_isStopped = false;
			m_hasChangeRequest = true;
		}
		
		private bool IsPlaylistIdentical( ref List< BgmPlayListTrack > playlist, bool contentwiseCheck = false )
		{
			if( !contentwiseCheck )
			{
				return playlist == m_playlist;
			}
			
			if( playlist.Count != m_playlist.Count )
			{
				return false;
			}
			
			for( int i = 0; i < playlist.Count; ++i )
			{
				BgmPlayListTrack track = playlist[ i ];
				if( track.m_clip != m_playlist[ i ].m_clip || track.m_crossfadeTime != m_playlist[ i ].m_crossfadeTime )
				{
					return false;
				}
			}
			
			return true;
		}
		
		private AudioSource m_LeftSource             = null;
		private AudioSource m_RightSource            = null;
		private float m_BgmCrossfader                = 0.0f; // 0.0 mean current source, 1.0 means next source
		private float m_CrossfadeTime                = 2.0f;
		private bool m_hasChangeRequest              = false;
		private bool m_isLooping                     = false;
		private bool m_isTrackAboutToEnd             = false;
		private bool m_isStopped                     = true;
		private int m_trackNumber                    = -1;
		private List< BgmPlayListTrack > m_playlist  = new List< BgmPlayListTrack >();
	}
	
	/// <summary>
	/// A helper class for the SoundManager. This class does the managing and playback of sound effects within
	/// n-buffers (audio sources), which helps prevent smooth sounding playback of spammed sound playback.
	/// </summary>
	public class SfxPlayer
	{
		// Constructor with audio clip and specified amount of buffers.
		public SfxPlayer( AudioClip clip, int numBuffer )
		{
			if( clip != null )
			{
				m_AudioBuffers = new AudioSource[ numBuffer ];
				GameObject go = SoundManager.Instance.SoundSourcesObject;
				for( int i = 0; i < m_AudioBuffers.Length; ++i )
				{
					AudioSource source = go.AddComponent< AudioSource >();
					if( source == null )
					{
						DebugUtil.LogError( "Unable to create " + i + "th AudioSourceBuffer for " + clip.name );
						continue;
					}
					source.clip = clip;
					source.playOnAwake = false;
					source.volume = 1.0f;
					m_AudioBuffers[ i ] = source;
				}
			}
			else
			{
				DebugUtil.LogError( "Null clip error!" );
			}
		}
		
		// Plays the sound effect with a delay specified and if it lops.
		public void Play( float delay = 0.0f, bool loop = false )
		{
			if( m_AudioBuffers.Length > 0 )
			{
				AudioSource source = m_AudioBuffers[ m_NextBufferToUse ];
				if( source && source.clip && source.clip.isReadyToPlay )
				{
					source.Stop();
					source.loop = loop;
					source.PlayDelayed( delay );
				}
				m_NextBufferToUse = ( m_NextBufferToUse + 1 ) % m_AudioBuffers.Length;
			}
		}
		
		// Stops playback of all sound effects.
		public void Stop()
		{
			foreach( AudioSource source in m_AudioBuffers )
			{
				if( source != null )
				{
					source.Stop();
				}
			}
		}
		
		// Sets the volume of the sound effects.
		public void SetVolume( float volume )
		{
			volume = Math.Clamp0( volume );
			foreach( AudioSource source in m_AudioBuffers )
			{
				if( source != null )
				{
					source.volume = volume;
				}
			}
		}
		
		// Returns true if any of the audio sources are playing.
		public bool IsPlaying()
		{
			bool areSourcesPlaying = false;
			
			foreach( AudioSource source in m_AudioBuffers )
			{
				if( source != null && source.isPlaying )
				{
					return true;
				}
			}
			
			return areSourcesPlaying;
		}
		
		private int m_NextBufferToUse        = 0;
		private AudioSource[] m_AudioBuffers = null;
	}
	
	/// <summary>
	/// This is a helper class that becomes a handle for an individual audio file. A library of audio files
	/// is the conjunction of one or more of these. It is inteded that this will be implemented using the flyweight
	/// design pattern.
	/// <c/summary>
	public class AudioFileHandler
	{
		// Constructor providing the file information for the audio file and whether it is a bgm and how
		// many sfx buffers would be needed.
		public AudioFileHandler( string fileInfo, bool isBgm, int sfxBufferCount = 1 )
		{
			m_fileInfo = fileInfo;
			m_isBgm = isBgm;
			m_refCount = 1;
			m_SfxBufferSize = sfxBufferCount;
			m_isReadyForPlayback = false;
		}
		
		// Returns true if this handler has an audio clip.
		public bool HasAudioClipFile
		{
			get
			{
				return m_hasAudioClip;
			}
		}
		
		// Retrieve or set the audio clip
		public AudioClip AudioClipFile
		{
			get
			{
				return m_audioClip;
			}
			
			set
			{
				m_hasAudioClip = value != null;
				m_audioClip = value;
			}
		}
		
		// Prepares (instantiates) n amount of audio sources based on the buffer size specified.
		public void PrepareSfxBuffers( float volume )
		{
			if( m_sfx != null )
			{
				m_sfx.Stop();
				m_sfx = null;
			}
			
			m_sfx = new SfxPlayer( m_audioClip, m_SfxBufferSize );
			m_sfx.SetVolume( volume );
		}
		
		public int m_refCount               = 0;        // Takes note of the reference count.
		public string m_fileInfo            = "";       // The file information of the audio file.
		public bool m_isBgm                 = false;    // Flag to know if its a bgm or not.
		public SfxPlayer m_sfx              = null;     // Pointer to the sfx player if it becomes one.
		public float m_audioLength          = 0.0f;     // The length of the playback of the audio file.
		public int m_AudioBridgeId          = -1;       // AdnAudioBridge only. The soundId of this file.
		public int m_LastAudioBridgeStream  = -1;       // The last streaming id created for this sound.
		public bool m_isReadyForPlayback	= false;
		private int m_SfxBufferSize         = 1;
		private bool m_hasAudioClip         = false;
		private AudioClip m_audioClip       = null;
	}
	
}