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
/// Cleanup scene is a script that handles cleanup of memory. This is originally
/// intended as a hack to "force clean" the memory from one scene to another
/// scene, which is seen as effective. This works best in conjunction with SceneManager.
///
/// Additional Notes: You can actually extend the setup of this scene to your liking. You
/// design the scene to display a GUI or color for instances where clean up will take a
/// long while. (e.g. Now Loading, Please Wait, etc.)
///
/// Usage: Make an new empty scene and attach this to a gameobject. Whenever you
/// exit out from a scene, load this scene before loading the next desired scene. This
/// requires SceneManager to work and this uses the OnCleanupComplete callback
/// to signal completion.
/// </summary>
public class CleanupScene : MonoBehaviour
{
    protected void Awake()
    {
        m_SceneMgr = SceneManager.Instance;
        if( !m_SceneMgr )
        {
            DebugUtil.LogError( "Cleanup requires a scene manager." );
            this.enabled = false;
            return;
        }
        
        Camera camera = GetComponent< Camera >() as Camera;
        if( camera )
        {
            camera.backgroundColor = m_SceneMgr.GetTransitionColor();
        }
    }
    
    protected void Start()
    {
        DebugUtil.Log( "Starting garbage collection." );
        System.GC.Collect();
        m_IsCallbackInvoked = false;
    }
    
    protected void Update()
    {
        // Gc.WaitForFullGCComplete() is not available in Mono?
        //if( System.GC.WaitForFullGCComplete() == System.GCNotificationStatus.Succeeded )
        {
            if( !m_IsCallbackInvoked )
            {
                DebugUtil.Log( "Garbage collection completed." );
				SubscriptionManager.Instance.NotifySubscribers( "CleanupScene.OnCleanupComplete" );
            }
            m_IsCallbackInvoked = true;
        }
    }
    
    private SceneManager m_SceneMgr     = null;
    private bool m_IsCallbackInvoked    = false;
}
