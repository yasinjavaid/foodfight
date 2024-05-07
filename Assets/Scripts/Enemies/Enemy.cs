using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.AI
{
	public class Enemy: MonoBehaviour
	{
		public AsyncReactiveProperty<float> Health;
		public AsyncReactiveProperty<float> Speed;
		public AsyncReactiveProperty<float> Accuracy;
	}
}