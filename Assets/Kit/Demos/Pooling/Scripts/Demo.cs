using System.Collections.Generic;
using Kit;
using Kit.UI.Message;
using UnityEngine;

namespace Demos.Pooling
{
	public class Demo: Singleton<Demo>
	{
		public AudioClip Music;
		public List<Enemy> Enemies { get; } = new List<Enemy>();

		private void Start()
		{
			AudioManager.PlayMusic(Music);
		}

		public void AddEnemy(Enemy enemy)
		{
			Enemies.Add(enemy);
		}

		public void RemoveEnemy(Enemy enemy)
		{
			Enemies.Remove(enemy);
			if (Enemies.Count <= 0)
				End();
		}

		public void End()
		{
			AudioManager.StopMusic();
			MessageWindow.Show("Demo", "You win. Meh.");
		}
	}
}