// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEngine;

[RequireComponent( typeof( Camera ) )]
[AddComponentMenu( "Image Effects/Amplify Motion" )]
public class AmplifyMotionEffect : AmplifyMotionEffectBase
{
	public static new AmplifyMotionEffect FirstInstance { get { return ( AmplifyMotionEffect ) AmplifyMotionEffectBase.FirstInstance; } }
	public static new AmplifyMotionEffect Instance { get { return ( AmplifyMotionEffect ) AmplifyMotionEffectBase.Instance; } }
}
