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
using System.Collections.Generic;
using QsLib;

/// <summary>
/// Subscription manager reduces dependency by being the middle-man of messages or events
/// to be passed around to systems. This is simply a single, global notifier for all to have
/// instant access.
/// 
/// Usage: See Notifier class usage.
/// 
/// Warning: It is highly recommended to remove a subscriber appropriately especially when
/// the object the function belongs to is no longer present. You don't want the method to be
/// executed on illogicaly. To remove a subscriber, simply use the RemoveSubscriber method.
/// </summary>
public class SubscriptionManager : Singleton< SubscriptionManager >
{
	// Add a NotifiableMethod delegate as a subscriber to be executed as callback when a
	// message is received.
	public void AddSubscriber( Notifier.NotifiableMethod sub )
	{
		m_notifier.AddSubscriber( sub );
	}

	// Remove a NotifiableMethod delegate from getting executed whenever a message is
	// received.
	public void RemoveSubscriber( Notifier.NotifiableMethod sub )
	{
		m_notifier.RemoveSubscriber( sub );
	}

	// For systems requiring to invoke the callback of interested subscribers, pass a
	// unique message and an optional parameters for all subscribers to receive.
	public void NotifySubscribers( string message, object param = null )
	{
		m_notifier.NotifySubscribers( message, param );
	}

	protected SubscriptionManager() {} // Singleton

	private Notifier m_notifier = new Notifier();
}
