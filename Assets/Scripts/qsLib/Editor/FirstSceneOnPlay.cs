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

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using QsLib;
using System.IO;
using System.Text;

/// <summary>
/// FirstSceneOnPlay is a handy Editor script that lets you use the hotkey Command-0 (mac)
/// or Ctrl+0 (PC) to play and execute the first scene on your build settings. Pressing the
/// hotkey again while the Editor is in play mode will get you back to the scene you were
/// previously working.
/// </summary>
[InitializeOnLoad]
public class FirstSceneOnPlay
{
    // Actual code to do the play/unplay feature.
    [MenuItem("Tools/qsLib/Play First Scene %0")]
    public static void PlayFromPrelaunchScene()
    {
        if ( EditorApplication.isPlaying == true )
        {
            string scene = GetEditorScene();
            if( scene != null && scene.Length > 0 )
            {
                EditorApplication.OpenScene( scene );
            }
            EditorApplication.isPlaying = false;
            return;
        }

        SetEditorScene( EditorApplication.currentScene );

        string[] scenes = GetListOfScenes();

        if( scenes.Length == 0 )
        {
            Debug.LogError( "There are no scenes available." );
            return;
        }

        EditorApplication.SaveCurrentSceneIfUserWantsTo();
        EditorApplication.OpenScene( scenes[ 0 ] );
        EditorApplication.isPlaying = true;
    }

    // A helper function to retrieve the list of scenes enabled in the Editor.
    public static string[] GetListOfScenes()
    {
        List< string > scenePaths = new List< string >();
        foreach ( EditorBuildSettingsScene scene in EditorBuildSettings.scenes )
        {
            if( scene.enabled )
            {
                scenePaths.Add( scene.path );
            }
        }

        return scenePaths.ToArray();
    }

    // A helper to write down to a file the scene path specified.
    protected static void SetEditorScene( string scenePath )
    {
        if( scenePath == null || scenePath.Length == 0 )
        {
            return;
        }

        string fileToWrite = Path.Combine( AppDir.TempPath, "EditorScene.bin" );

        FileStream fs = File.Create( fileToWrite );
        try
        {
            byte[] bytes = Encoding.Unicode.GetBytes( scenePath );
            fs.Write( bytes, 0, bytes.Length );
        }
        catch( IOException ioe )
        {
            DebugUtil.LogError( string.Format( "Problem writing onto {0}\n{1}", fileToWrite, ioe.ToString() ) );
        }
        finally
        {
            fs.Close();
        }
    }

    // A helper to retrieve the content of the file written via SetEditorScene.
    protected static string GetEditorScene()
    {
        string readString = "";
        string fileToRead = Path.Combine( AppDir.TempPath, "EditorScene.bin" );
        FileStream fs = File.Open( fileToRead, FileMode.Open, FileAccess.Read );
        try
        {
            int length = (int) fs.Length;
            byte[] bytes = new byte[ length ];

            int count = 0;
            int sum = 0;
            
            // read until Read method returns 0 (end of the stream has been reached)
            while( ( count = fs.Read( bytes, sum, length - sum ) ) > 0 )
            {
                sum += count;  // sum is a buffer offset for next reading
            }

            readString = Encoding.Unicode.GetString( bytes );
        }
        catch( IOException ioe )
        {
            DebugUtil.LogError( "Problem reading from " + fileToRead );
        }
        finally
        {
            fs.Close();
        }

        return readString;
    }
}