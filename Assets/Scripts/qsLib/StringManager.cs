/*
Unity Quickstart Library (qsLib)
http://www.andreivictor.net/uqsl/
Last Updated 09/04/2015

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
using System.Text;
using System.IO;
using QsLib;

/// <summary>
/// String manager class handles the proper retrieval of equivalent strings
/// per language via a key.
/// Usage:
/// 1) In inspector, make sure the language files are correctly set and present.
/// 2) Make sure the language is set via ".language = " property.
/// 3) On other areas of code, using GetString( id, out realString ) to get the
///    appropriate language string of the id.
/// </summary>
public class StringManager : Singleton< StringManager >
{
	// Set the language of the string table.
	public Language language
	{
		get { return m_selectedLanguage; }
		set
		{
			if( m_selectedLanguage == value )
			{
				return;
			}

			SubscriptionManager.Instance.NotifySubscribers( "StringManager.OnLanguageChanged", value );
			LoadNewLanguage( value );
		}
	}

	// Retrieve the real string specified by the id from the string table based on the
	// current language set.
	public bool GetString( string id, out string realString )
	{
		realString = null;
		if( m_stringTable == null )
		{
			return false;
		}

        Dictionary< string, string > row = null;
        if( !m_stringTable.GetDataRow( id, out row ) )
        {
            return false;
        }

		return row.TryGetValue( "value", out realString );
	}

	protected void LoadNewLanguage( Language language )
	{
		string path = GetPathByLanguage( language );
		CsvReader reader = new CsvReader();
		if( reader.Open( path ) )
		{
			//TODO: Optimize by reading only by file reading at a time, or add a cached string list
			m_stringTable = reader.ReadAllToMemory();
			reader.Close();
			m_selectedLanguage = language;
		}
		else
		{
			DebugUtil.LogError( string.Format( "StringManager: Could not find {0}", path ) );
		}
	}

	protected string GetPathByLanguage( Language language )
	{
		string filename = "";
		switch( language )
		{
		case Language.German:
			filename = m_GermanFilename;
			break;
		case Language.Japanese:
			filename = m_JapaneseFilename;
			break;
		case Language.Chinese:
			filename = m_ChineseFilename;
			break;
		case Language.Russian:
			filename = m_RussianFilename;
			break;
		default:
			filename = m_EnglishFilename;
			break;
		}
		return Path.Combine( AppDir.DataPath, filename );
	}

	protected Language GetLanguageFromSystem()
	{
		Language lang = Language.English;
		switch( Application.systemLanguage )
		{
		case SystemLanguage.German:
			lang = Language.German;
			break;
		case SystemLanguage.Japanese:
			lang = Language.Japanese;
			break;
		case SystemLanguage.Chinese:
			lang = Language.Chinese;
			break;
		case SystemLanguage.Russian:
			lang = Language.Russian;
			break;
		default:
			lang = Language.English;
			break;
		}
		return lang;
	}

	protected StringManager() {} // Singleton

	protected void Start()
	{
		LoadNewLanguage( m_DetectLanguageFromSystem ? GetLanguageFromSystem() : m_selectedLanguage );
	}

	public enum Language
	{
		English     = 0,
		German,
		Japanese,
		Chinese,
		Russian
	}

	[SerializeField] protected Language m_selectedLanguage       = Language.English;   // Select the desired language to be set.
	[SerializeField] protected bool m_DetectLanguageFromSystem   = false;              // Enable the autodetection of language.
	[SerializeField] protected string m_EnglishFilename          = "en.csv";           // filename (not path) of the english language string table.
	[SerializeField] protected string m_GermanFilename           = "de.csv";           // filename (not path) of the german language string table.
	[SerializeField] protected string m_JapaneseFilename         = "ja.csv";           // filename (not path) of the japanese language string table.
	[SerializeField] protected string m_ChineseFilename          = "ch.csv";           // filename (not path) of the chinese language string table.
	[SerializeField] protected string m_RussianFilename          = "ru.csv";           // filename (not path) of the russian language string table.

	private CsvDictionary m_stringTable         = null;
}
