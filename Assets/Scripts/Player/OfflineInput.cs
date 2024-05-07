using System.Threading;
using Cysharp.Threading.Tasks.Linq;
using Kit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
	public class OfflineInput: MonoBehaviour
	{
		public Player Player { get; protected set; }

		private CompositeDisposable disposables = new CompositeDisposable();
		private CancellationTokenSource cancelSource = new CancellationTokenSource();

		private bool fireInput = false;

		private void Awake()
		{
			Player = GetComponent<Player>();
			Hook();
		}

		private void Start()
		{
			Player.Initialize();
		}

		private void Hook()
		{
			Player.State = new OfflinePlayerState(Player);

			disposables.Add(MessageBroker.Instance
										 .Receive<AmmoEmptied>()
										 .Where(msg => msg.Player == Player)
										 .Subscribe(msg => OnAmmoEmptied()));
		}

		private void FixedUpdate()
		{
			Player.UpdateMovement();
			Player.ApplyGravity();
			Player.UpdateAnimator();

			if (fireInput && Player.CanFire)
			{
				Player.Fire();
				if (!Player.CurrentWeapon.Autofire)
					fireInput = false;
			}
		}

		private void OnMove(InputValue value)
		{
			Player.SetHorizontalInput(value.Get<Vector2>());
		}

		private void OnLook(InputValue value)
		{
			Player.Look(value.Get<Vector2>());
		}

		private void OnSprint(InputValue value)
		{
			Player.State.IsSprinting = value.isPressed;
		}

		private void OnJump()
		{
			if (Player.CanJump)
			{
				Player.Jump();
				Player.PerformJumpAnimation();
			}
		}

		private void OnAim(InputValue value)
		{
			bool aiming = value.isPressed;
			Player.State.IsAiming = aiming;
			Player.Aim(aiming);
		}

		private void OnFire(InputValue value)
		{
			fireInput = value.isPressed;
		}

		private void OnReload()
		{
			if (Player.CanReload)
				Player.Reload();
		}

		private void OnInteract()
		{
			if (Player.CanInteract)
				Player.Interact();
		}

		private void OnAmmoEmptied()
		{
			ControlHelper.Delay(() => Player.CanReload, () => Player.Reload(), cancelSource.Token);
		}

		private void OnDestroy()
		{
			Unhook();
		}

		private void Unhook()
		{
			disposables.Dispose();
		}
	}
}