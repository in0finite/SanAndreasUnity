// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu( "" )]
[RequireComponent( typeof( Camera ) )]
sealed public class AmplifyMotionPostProcess : MonoBehaviour
{
	private AmplifyMotionEffectBase m_instance = null;
	public AmplifyMotionEffectBase Instance { get { return m_instance; } set { m_instance = value; } }

	void OnRenderImage( RenderTexture source, RenderTexture destination )
	{
		if ( m_instance != null )
			m_instance.PostProcess( source, destination );
	}
}
