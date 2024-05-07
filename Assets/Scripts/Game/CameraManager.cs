using Cinemachine;
using Game.UI.HUD;
using Kit;
using UnityEngine;

namespace Game
{
	public class CameraManager: Singleton<CameraManager>
	{
		public CinemachineVirtualCamera CameraMove;
		public CinemachineVirtualCamera CameraAim;
		public Reticule Reticule;
		public CinemachineBrain Brain { get; private set; }

		private void Awake()
		{
			Brain = GetComponent<CinemachineBrain>();
		}

		public void Attach(Player player)
		{
			Brain.enabled = true;
			CameraMove.Follow = player.CameraAnchor;
			CameraAim.Follow = player.CameraAnchor;
		}

		public void Detach()
		{
			Brain.enabled = false;
			CameraMove.Follow = null;
			CameraAim.Follow = null;
		}

		public void Switch(bool aim)
		{
			CameraAim.Priority = aim ? 1 : 0;
			CameraMove.Priority = aim ? 0 : 1;
		}

		public Vector3 LookAt => Brain.CurrentCameraState.ReferenceLookAt;
	}
}