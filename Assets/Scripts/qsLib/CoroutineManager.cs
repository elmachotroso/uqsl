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

/// <summary>
/// This manager handles all couroutines by wrapping them. The coroutines
/// should be accessible via this manager and can be suspended and terminated
/// at any time the corresponding calls are made by the programmer.
/// STUB TBA
/// </summary>
public class CoroutineManager : Singleton< CoroutineManager >
{
	protected CoroutineManager() {} // Singleton
}
