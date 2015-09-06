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
using System.Collections;
using QsLib;

/// AdnAudioBridge is a special Android sound effects class that act as the API
/// for bridging native Audio to Unity as an Android JNI plugin. This uses the
/// native sound pool technology of the Android platform, which eliminates the
/// delay in the sound playback in device. Note that this technology is only
/// meant for short sounds and has a hard-coded 1MB limit. It can deal with
/// almost any uncompressed/compressed sound formats.
///
/// Notes:
/// 1. You would normally need one instance of this
/// 2. Use the AdnAudioBridge.cs C# class in Unity to interface with this class.
///
/// Limitations:
/// 1. This uses only a single AudioTrack in the audio platform at can only deal
///    with a limited number of streams overall in one pool.
/// 2. This can only track up to the latest maxStreams instances of the same
///    sound for changing volume or stopping.
///
/// Usage:
/// 1. Initialize
/// 2. loadSounds
/// 3. play, stop, whatever the loaded sounds
/// 4. unloadSounds when no longer needed. (if you don't, you get memory leak)
public class AdnAudioBridge
{
    // Initializes the class and bind with the Android platform via JNI.
    // You must always call this once whenever you needed this technology for
    // your project.
    public static void Initialize( int maxStreams )
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        m_unityActivityClass = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" );
        if( m_unityActivityClass == null )
        {
            DebugUtil.LogError( "AdnAudioBridge: Cannot bind to UnityPlayer class!" );
            return;
        }
        
        m_activityContext = m_unityActivityClass.GetStatic< AndroidJavaObject >( "currentActivity" );
        if( m_activityContext == null )
        {
            DebugUtil.LogError( "AdnAudioBridge: Cannot find UnityPlayer instance!" );
            return;
        }
        
        m_soundObject = new AndroidJavaObject( "net.andreivictor.uqsl.adnaudiobridge.AdnAudioBridge", maxStreams, 128, m_activityContext );
        if( m_soundObject == null )
        {
            DebugUtil.LogError( "AdnAudioBridge: Failed to bind with AdnAudioBridge." );
            return;
        }
        #endif //UNITY_ANDROID && !UNITY_EDITOR
    }
    
    // Plays a sound using the specified sound id, with the volume set (maximum of 1.0)
    // and returns the stream id.
    public static int PlaySound( int soundId, float volume = 1.0f )
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        return m_soundObject.Call< int >( "playSound", new object[] { soundId, volume } );
        #else
        DebugUtil.Log( "AdnAudioBridge method called but not available for this platform: " + "PlaySound" );
        return 0;
        #endif //UNITY_ANDROID && !UNITY_EDITOR
    }
    
    // Stop the sound of the specified soundId. This affects all streams known
    // particular to this soundId.
    public static void StopSound( int soundId )
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        m_soundObject.Call( "stopSound", new object[] { soundId } );
        #else
        DebugUtil.Log( "AdnAudioBridge method called but not available for this platform: " + "SetVolume" );
        #endif //UNITY_ANDROID && !UNITY_EDITOR
    }
    
    // Stop the sound of the specified streamId of the soundId.
    public static void StopSound( int soundId, int streamId )
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        m_soundObject.Call( "stopSound", new object[] { soundId, streamId } );
        #else
        DebugUtil.Log( "AdnAudioBridge method called but not available for this platform: " + "SetVolume" );
        #endif //UNITY_ANDROID && !UNITY_EDITOR
    }
    
    // Change the volume of the specified soundId. This affects all known streams
    // of this sound.
    public static void SetVolume( int soundId, float volume = 1.0f )
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        m_soundObject.Call( "setVolume", new object[] { soundId, volume } );
        #else
        DebugUtil.Log( "AdnAudioBridge method called but not available for this platform: " + "SetVolume" );
        #endif //UNITY_ANDROID && !UNITY_EDITOR
    }

    // Change the volume of the specified streamId of the soundId.
    public static void SetVolume( int soundId, int streamId, float volume = 1.0f )
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        m_soundObject.Call( "setVolume", new object[] { soundId, streamId, volume } );
        #else
        DebugUtil.Log( "AdnAudioBridge method called but not available for this platform: " + "SetVolume" );
        #endif //UNITY_ANDROID && !UNITY_EDITOR
    }
    
    // Loads the sound specified by the path and name, soundFile, then returns
    // the soundId associated with that soundFile.
    public static int LoadSound( string soundFile )
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        return m_soundObject.Call< int >( "loadSound", new object[] { soundFile } );
        #else
        DebugUtil.Log( "AdnAudioBridge method called but not available for this platform: " + "LoadSound" );
        return -1;
        #endif //UNITY_ANDROID && !UNITY_EDITOR
    }
    
    // Unloads the loaded sound from the sound pool. You must unload any loaded
    // sound when you don't need them.
    public static void UnloadSound( int soundId )
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        m_soundObject.Call( "unloadSound", new object[] { soundId } );
        #else
        DebugUtil.Log( "AdnAudioBridge method called but not available for this platform: " + "UnloadSound" );
        #endif //UNITY_ANDROID && !UNITY_EDITOR
    }

    #if UNITY_ANDROID && !UNITY_EDITOR
    protected static AndroidJavaClass m_unityActivityClass  = null;
    protected static AndroidJavaObject m_activityContext    = null;
    protected static AndroidJavaObject m_soundObject        = null;
    #endif //UNITY_ANDROID && !UNITY_EDITOR
}
