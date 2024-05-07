using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Kit;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD
{
	public class Reticule: MonoBehaviour
	{
		public float Time = 0.3f;
		public Ease Easing = Ease.Linear;

		private CompositeDisposable disposables = new CompositeDisposable();

		private Image image;
		private float onAlpha;

		private void Awake()
		{
			image = GetComponent<Image>();
			onAlpha = image.color.a;
			image.color = image.color.SetAlpha(0);

			Attach();
		}

		private void Attach()
		{
			disposables.Add(MessageBroker.Instance.Receive<GainedControl>().Subscribe(_ => Show()));
		}

		public void Show()
		{
			Show(true);
		}

		public void Hide()
		{
			Show(false);
		}

		public void Show(bool value)
		{
			image.DOFade(value ? onAlpha : 0, Time).SetEase(Easing);
		}

		private void OnDestroy()
		{
			disposables.Dispose();
		}
	}
}