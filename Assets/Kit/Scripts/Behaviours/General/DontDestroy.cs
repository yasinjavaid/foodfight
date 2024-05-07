﻿using UnityEngine;

namespace Kit.Behaviours
{
	/// <summary>Marks the <see cref="GameObject" /> to be persistent across scenes.</summary>
	public class DontDestroy: MonoBehaviour
	{
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}
	}
}