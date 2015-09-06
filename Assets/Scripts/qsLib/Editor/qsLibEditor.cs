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
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using QsLib;

/// <summary>
/// This is a collection of functionality related to Editor actions and tools in Unity.
/// </summary>
public class QsLibEditor : MonoBehaviour
{
    // Temporary location that is ignored to be part of the final binary.
    // Note: basically any folder with a '.' as a first character of its name.
	private static string s_TemporaryLocation = "Assets/.ignoreAssets";
	
    // A quick file replacement for mp3 formats to ogg.
	[MenuItem( "Tools/qsLib/Assets/Sounds/Switch to OggVorbis (.ogg)" )]
	public static void SwitchMp3ToOgg()
	{
		if( s_isBusy )
		{
			DebugUtil.Log( "Has unfinished information please wait for it first." );
			return;
		}
		
		s_isBusy = true;
		
		DoSwitch( ".mp3", ".ogg" );
		
		s_isBusy = false;
	}
	
    // A quick file replacement for ogg formats to mp3.
	[MenuItem( "Tools/qsLib/Assets/Sounds/Switch to MP3 (.mp3)" )]
	public static void SwitchOggToMp3()
	{
		if( s_isBusy )
		{
			DebugUtil.Log( "Has unfinished information please wait for it first." );
			return;
		}
		
		s_isBusy = true;
		
		DoSwitch( ".ogg", ".mp3" );
		
		s_isBusy = false;
	}
	
	// A custom build procedure to build a game on the Windows Standalone platform.
	// UNITY PRO ONLY!
	[MenuItem( "Tools/qsLib/Builds/Build Windows" )]
	public static void BuildWindowsStandAlone()
	{
		string binary = PlayerSettings.productName + ".exe";
		string path = EditorUtility.SaveFolderPanel( "Where to create the build binary?", "",
			PlayerSettings.productName );
		if( path.Length < 1 || path == "" )
		{
			// this means cancel
			DebugUtil.Log( "Build was canceled." );
			return;
		}
		
		if( !Directory.Exists( path ) )
		{
			Directory.CreateDirectory( path );
		}
		DebugUtil.Log( "Building on folder: " + path );
		
		string where = Path.Combine( path, binary );
		
		BuildGameOnPlatform( where, BuildTarget.StandaloneWindows, BuildOptions.None );
	}
	
	private static void BuildGameOnPlatform( string where, BuildTarget buildTarget, BuildOptions buildOptions )
	{
		string[] levels = GetLevelsForBuild();
		
		// Build player.
		BuildPipeline.BuildPlayer( levels, where, buildTarget, buildOptions );
	}
	
	private static string[] GetLevelsForBuild()
	{
		List< string > levels = new List< string >();
		foreach( EditorBuildSettingsScene scene in EditorBuildSettings.scenes )
		{
			if( scene.enabled )
			{
				levels.Add( scene.path );
			}
		}
		return levels.ToArray();
	}
	
	private static void DoSwitch( string oldformat, string newformat )
	{
		// C:/Folder/AppName/Assets/Temp
		string tempFolder = Combine( AppPath(), s_TemporaryLocation );
		
		// If temp folder is not present, create it.
		if( !Directory.Exists( tempFolder ) )
		{
			DebugUtil.Log ( tempFolder + " was not found. It will be created." );
			Directory.CreateDirectory ( tempFolder );
		}
		
		// C:/Folder/AppName
		string targetFolder = AppPath();
		
		string[] files = GetAssetsOfType( oldformat );
		foreach( string file in files )
		{			
			// ignore files inside temporary location
			if( file.Contains( s_TemporaryLocation ) )
			{
				continue;
			}
			
			// Check if there is old format already in target folder
			bool hasNewFormat = File.Exists( Combine( targetFolder, Path.ChangeExtension( file, newformat ) ) );
			
			// move old format to temp folder
			if( !Directory.Exists( Combine( tempFolder, Path.GetDirectoryName( file ) ) ) )
			{
				Directory.CreateDirectory( Combine( tempFolder, Path.GetDirectoryName( file ) ) );
			}
			
			// move the old format files and their metas.
			string fullTargetPathAndFile = Combine( targetFolder, file );
			string fullTempPathAndFile = Combine( tempFolder, file );
			FileUtil.MoveFileOrDirectory( fullTargetPathAndFile, fullTempPathAndFile );
			FileUtil.MoveFileOrDirectory( fullTargetPathAndFile + ".meta",
			                             fullTempPathAndFile + ".meta" );
			DebugUtil.Log( fullTargetPathAndFile + " -> " + fullTempPathAndFile );
			
			// if no new format exists, try to get it from tempfolder
			if( !hasNewFormat )
			{
				string fullTempPathAndFileNew = Combine( tempFolder, Path.ChangeExtension( file, newformat ) );
				if( !File.Exists( fullTempPathAndFileNew ) )
				{
					DebugUtil.LogWarning( "Cannot copy missing " + fullTempPathAndFileNew );
					continue;
				}
				
				string fullTargetPathAndFileNew = Combine( targetFolder, Path.ChangeExtension( file, newformat ) );
				
				FileUtil.MoveFileOrDirectory( fullTempPathAndFileNew, fullTargetPathAndFileNew );
				FileUtil.MoveFileOrDirectory( fullTempPathAndFileNew + ".meta", fullTargetPathAndFileNew + ".meta" );
				DebugUtil.Log( fullTempPathAndFileNew + " -> " + fullTargetPathAndFileNew );
			}
		}
		
		AssetDatabase.Refresh();
		DebugUtil.Log( oldformat + " switch to " + newformat + " operation done." );
	}
	
	private static string[] GetAssetsOfType( string fileExtension )
	{
		List< string > tempObjects = new List< string >();
		DirectoryInfo directory = new DirectoryInfo( Application.dataPath );
		FileInfo[] goFileInfo = directory.GetFiles( "*" + fileExtension, SearchOption.AllDirectories );
		int i = 0;
		int goFileInfoLength = goFileInfo.Length;
		FileInfo tempGoFileInfo;
		string tempFilePath;
		for( ; i < goFileInfoLength; i++ )
		{
			tempGoFileInfo = goFileInfo[i];
			if (tempGoFileInfo == null)
			{
				continue;
			}
			
			tempFilePath = tempGoFileInfo.FullName;
			tempFilePath = tempFilePath.Replace( @"\", "/" ).Replace( Application.dataPath, "Assets" );
			tempObjects.Add(tempFilePath);
		}
		return tempObjects.ToArray();
	}
	
	private static string Combine( string string1, string string2 )
	{
		return Path.Combine( string1, string2 ).Replace( '\\', '/' );
	}
	
	private static string AppPath()
	{
		return Application.dataPath.Replace( "/Assets", "" );
	}
	
	private static string TrimDataPath( string path )
	{
		return path.Replace( Application.dataPath, "" );
	}
	
	private static bool s_isBusy		= false;
}
