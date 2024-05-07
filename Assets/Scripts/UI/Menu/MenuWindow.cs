using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Menu
{
	public class MenuWindow: MonoBehaviour
	{
		public Text TitleText;
		public GameObject ModButton;

		private void Awake()
		{
			if (TitleText != null)
				TitleText.text = Application.productName;

#if !MODDING
			if (ModButton != null)
				ModButton.SetActive(false);
#endif
		}
	}
}