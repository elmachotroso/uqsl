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

package net.andreivictor.uqsl.adnaudiobridge;

import android.app.Activity;
import android.content.Context;
import android.content.res.AssetFileDescriptor;
import android.media.AudioManager;
import android.media.SoundPool;
import android.util.Log;
import java.io.IOException;
import java.util.Vector;
import java.util.Hashtable;

/// AdnAudioBridge is a special Android sound effects class that act as the API
/// for bridging native Audio to Unity as an Android JNI plugin. The uses the
/// native sound pool technology of the Android platform, which eliminates the
/// delay in the sound playback in device. Note that this technology is only
/// meant for short sounds and has a hard-coded 1MB limit. It can deal with
/// almost any uncompressed/compressed sound formats.
/// Notes:
/// 1. You would normally need one instance of this
/// 2. Use the AdnAudioBridge.cs C# class in Unity to interface with this class.
/// Limitations:
/// 1. This uses only a single AudioTrack in the audio platform at can only deal
///    with a limited number of streams overall in one pool.
/// 2. This can only track up to the latest maxStreams instances of the same
///    sound for changing volume or stopping.
/// Usage:
/// 1. Instance the class.
/// 2. loadSounds
/// 3. play, stop, whatever the loaded sounds
/// 4. unloadSounds when no longer needed. (if you don't, you get memory leak)
public class AdnAudioBridge
{
	// Instances the Audio Bridge. You only need one.
	// maxStreams - the amount of streams each specific sound can track.
	// maxPoolStreams - the total amount of sounds possible simultaneously playing.
	// activity - the android application context that you must pass in.
	public AdnAudioBridge( int maxStreams, int maxPoolStreams, Activity activity )
	{
		m_maxStreams = maxStreams;
		m_activity = activity;
		m_soundPool = new SoundPool( maxPoolStreams, AudioManager.STREAM_MUSIC, 0 );
		m_soundPool.setOnLoadCompleteListener(
			new SoundPool.OnLoadCompleteListener()
			{
				public void onLoadComplete( SoundPool soundPool, int sampleId, int status )
				{
					BufferedAudio buffer = m_sounds.get( sampleId );
					if( buffer != null )
					{
						// The buffer will only be usable if the sound is fully loaded
						// and ready.
						buffer.signalReady();
					}
				}
			}
			);
	}
	
	// Plays a sound using the specified sound id, with the volume set (maximum of 1.0)
	// and returns the stream id.
	public int playSound( int soundId, float volume )
	{
		if( ( !m_sounds.containsKey( soundId ) ) || ( soundId == 0 ) )
		{
			Log.e( "AdnAudioBridge Java", "File has not been loaded!" );
			return -1;
		}
		
		BufferedAudio buffer = m_sounds.get( soundId );
		if( buffer == null )
		{
		    Log.e( "AdnAudioBridge Java", "Buffer non-existent for " + soundId );
		    return -1;
		}

		return buffer.playSound( volume );
	}
	
	// Loads the sound specified by the path and name, soundFile, then returns
	// the soundId associated with that soundFile.
	public int loadSound( String soundFile )
	{
	    BufferedAudio buffer = new BufferedAudio( soundFile, m_maxStreams, m_activity, m_soundPool );
		if( buffer == null )
		{
			return -1;
		}
        
		int soundId = buffer.getSoundId();
		m_sounds.put( soundId, buffer );

		return soundId;
	}
	
	// Unloads the loaded sound from the sound pool. You must unload any loaded
	// sound when you don't need them.
	public void unloadSound( int soundId )
	{
		if( ( !m_sounds.containsKey( soundId ) ) || ( soundId == 0 ) )
		{
			Log.e( "AdnAudioBridge Java", "File has not been loaded!" );
			return;
		}
		
		BufferedAudio buffer = m_sounds.get( soundId );
		if( buffer == null )
		{
		    Log.e( "AdnAudioBridge Java", "Buffer non-existent for " + soundId );
		    return;
		}
		
		buffer.unloadSound();
		m_sounds.remove( soundId );
	}
	
	// Change the volume of the specified soundId. This affects all known streams
	// of this sound.
	public void setVolume( int soundId, float volume )
	{
		if( ( !m_sounds.containsKey( soundId ) ) || ( soundId == 0 ) )
		{
			Log.e( "AdnAudioBridge Java", "File has not been loaded!" );
			return;
		}
		
		BufferedAudio buffer = m_sounds.get( soundId );
		if( buffer == null )
		{
		    Log.e( "AdnAudioBridge Java", "Buffer non-existent for " + soundId );
		    return;
		}
		
		buffer.setVolume( volume );
	}
	
	// Change the volume of the specified streamId of the soundId.
	public void setVolume( int soundId, int streamId, float volume )
	{
		if( ( !m_sounds.containsKey( soundId ) ) || ( soundId == 0 ) )
		{
			Log.e( "AdnAudioBridge Java", "File has not been loaded!" );
			return;
		}
		
		BufferedAudio buffer = m_sounds.get( soundId );
		if( buffer == null )
		{
		    Log.e( "AdnAudioBridge Java", "Buffer non-existent for " + soundId );
		    return;
		}
		
		buffer.setVolume( streamId, volume );
	}
	
	// Stop the sound of the specified soundId. This affects all streams known
	// particular to this soundId.
	public void stopSound( int soundId )
	{
		if( ( !m_sounds.containsKey( soundId ) ) || ( soundId == 0 ) )
		{
			Log.e( "AdnAudioBridge Java", "File has not been loaded!" );
			return;
		}
		
		BufferedAudio buffer = m_sounds.get( soundId );
		if( buffer == null )
		{
		    Log.e( "AdnAudioBridge Java", "Buffer non-existent for " + soundId );
		    return;
		}
		
		buffer.stop();
	}
	
	// Stop the sound of the specified streamId of the soundId.
	public void stopSound( int soundId, int streamId )
	{
		if( ( !m_sounds.containsKey( soundId ) ) || ( soundId == 0 ) )
		{
			Log.e( "AdnAudioBridge Java", "File has not been loaded!" );
			return;
		}
		
		BufferedAudio buffer = m_sounds.get( soundId );
		if( buffer == null )
		{
		    Log.e( "AdnAudioBridge Java", "Buffer non-existent for " + soundId );
		    return;
		}
		
		buffer.stop( streamId );
	}
	
	// The class that handles the per-sound aspect of the audio bridge.
	// This wraps the actual sound pool API calls and does the tracking of the
	// stream ids. Each unique sound is one BufferedAudio instance. This does
	// not create a new sound pool. It uses a already made one.
    public class BufferedAudio
	{
		// Constructor (pass in the Android activity context, and the sound pool instance)
		// This also automatically loads the sound specified by soundFile.
		public BufferedAudio( String soundFile, int maxStreams, Activity activity, SoundPool pool )
		{
			m_maxStreams = maxStreams;
			m_soundPool = pool;
			m_activity = activity;
			
			if( m_soundPool == null )
			{
				Log.e( "AdnAudioBridge Java", "BufferedAudio needs a sound pool!" );
				return;
			}
			
			m_soundId = loadSound( soundFile );
		}
		
		// Retrieve the sound id for this buffer.
		public int getSoundId()
		{
		    return m_soundId;
		}
		
		// Make this buffer ready to accept sound playback commands.
		public void signalReady()
		{
			m_ready = true;
		}
		
		// Makes this buffer play the sound with specified volume.
		public int playSound( float volume )
		{
			if( !m_ready ) 
			{
				Log.e( "AdnAudioBridge Java", "Desired sound is not yet ready: " + m_soundId );
				return -1;
			}
			
			final float fVolume = volume;

			m_activity.runOnUiThread(
				new Runnable()
				{
					public void run()
					{
						 play( fVolume );
					}
				}
				);
				
			return m_lastStreamId;
		}

		// Unload the sound loaded in this buffer.
		public void unloadSound()
		{
			if( m_soundPool == null )
			{
				Log.e( "AdnAudioBridge Java", "SoundPool is null." );
				return;
			}
			
			if( !m_soundPool.unload( m_soundId ) )
			{
				Log.e( "AdnAudioBridge Java", "File has not been loaded!" );
			}
		}
		
		// Change the volume of the sound in this buffer. This affect all tracked
		// streams.
		public void setVolume( float volume )
		{
			if( m_soundPool == null )
			{
				Log.e( "AdnAudioBridge Java", "SoundPool is null." );
				return;
			}
			
			if( !m_ready ) 
			{
				Log.e( "AdnAudioBridge Java", "Desired sound is not yet ready: " + m_soundId );
				return;
			}
			
			for( int i = 0; i < m_streams.size(); ++i )
			{
				m_soundPool.setVolume( m_streams.get( i ), volume, volume );
			}
		}
		
		// Change the volume of a specific stream of this buffer.
		public void setVolume( int streamId, float volume )
		{
			if( m_soundPool == null )
			{
				Log.e( "AdnAudioBridge Java", "SoundPool is null." );
				return;
			}
			
			m_soundPool.setVolume( streamId, volume, volume );
		}
		
		// Stop all streams in this buffer.
		public void stop()
		{
			if( m_soundPool == null )
			{
				Log.e( "AdnAudioBridge Java", "SoundPool is null." );
				return;
			}
			
			if( !m_ready ) 
			{
				Log.e( "AdnAudioBridge Java", "Desired sound is not yet ready: " + m_soundId );
				return;
			}
			
			for( int i = 0; i < m_streams.size(); ++i )
			{
				m_soundPool.pause( m_streams.get( i ) );
			}
		}
		
		// Stops a specified stream in this buffer.
		public void stop( int streamId )
		{
			if( m_soundPool == null )
			{
				Log.e( "AdnAudioBridge Java", "SoundPool is null." );
				return;
			}
			
			if( !m_ready ) 
			{
				Log.e( "AdnAudioBridge Java", "Desired sound is not yet ready: " + m_soundId );
				return;
			}
			
			m_soundPool.pause( streamId );
		}
		
		private int play( float volume )
		{
			if( m_soundPool == null )
			{
				Log.e( "AdnAudioBridge Java", "SoundPool is null." );
				return -1;
			}
			
			if( !m_ready ) 
			{
				Log.e( "AdnAudioBridge Java", "Desired sound is not yet ready: " + m_soundId );
				return -1;
			}
			
			m_lastStreamId = m_soundPool.play( m_soundId, volume, volume, 1, 0, 1f );
			m_streams.add( m_lastStreamId );
			if( m_streams.size() > m_maxStreams )
			{
				// TODO: do we need to stop the stream first?
			    m_streams.remove( 0 );
			}
			
			return m_lastStreamId;
		}
		
		private int loadSound( String soundName )
		{
			if( m_soundPool == null )
			{
				Log.e( "AdnAudioBridge Java", "SoundPool is null." );
				return -1;
			}
			
			AssetFileDescriptor afd = null;

			try
			{
				afd = m_activity.getAssets().openFd( soundName );
			}
			catch( IOException e )
			{
				Log.e( "AdnAudioBridge Java", "File does not exist: " + soundName + ".\n" + e.toString() );
				return -1;
			}

			return m_soundPool.load( afd, 1 );
		}
		
		private int m_soundId				    = -1;
		private int m_maxStreams				= 0;
		private int m_lastStreamId				= -1;
		private boolean m_ready					= false;
		private SoundPool m_soundPool           = null;
		private Activity m_activity 	        = null;
		private Vector< Integer > m_streams     = new Vector< Integer >();
	}
	
	private int m_maxStreams								= 0;
	private SoundPool m_soundPool           				= null;
	private Activity m_activity 	        				= null;
	private Hashtable< Integer, BufferedAudio > m_sounds    = new Hashtable< Integer, BufferedAudio >();
}
