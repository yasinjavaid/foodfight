using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Items;
using Kit;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Networking.Bolt
{
	public class NetworkPlayer: EntityEventListener<IPlayerState>, INetworkPlayer
	{
		public Player Player { get; private set; }
		public MonoBehaviour Entity { get; private set; }

		private Vector2 lookInput;
		private Vector2 moveInput;
		private bool sprintInput;
		private bool aimInput;
		private bool jumpInput;
		private bool fireInput;
		private bool reloadInput;
		private bool interactInput;

		private CompositeDisposable disposables = new CompositeDisposable();
		private CancellationTokenSource cancelSource = new CancellationTokenSource();

		private void Awake()
		{
			Player = GetComponent<Player>();
			Entity = entity;
			
		}

		public override void Attached()
		{
			if (IsLocal)
			{
				Player.State = new LocalPlayerState(this);
			}
			else if (IsServer)
			{
				Player.State = new ServerPlayerState(this);
				Player.OnHit += OnPlayerHit;
			}
			else // Observers
			{
				Player.State = new ObserverPlayerState(this);

				// Commands are only executed for Server and the Local Client, so while movement is synced on observers through Transform,
				// animation is not.
				ControlHelper.EachFrame(Player.UpdateAnimator, cancelSource.Token, PlayerLoopTiming.FixedUpdate);
			}

			Player.Initialize();

			state.SetTransforms(state.Transform, transform, Player.Animator.transform);
			state.SetAnimator(Player.Animator);
			state.OnFire += LocalFire;
			state.OnReload += LocalReload;
			state.OnInteract += LocalInteract;
			state.AddCallback(nameof(state.Ammo), OnAmmoChanged);
		}

#if CLIENT_BUILD
		public override void ControlGained()
		{
			GameManager.Instance.GainControl(Player);
		}

		private void OnLook(InputValue value)
		{
			// Setting input events to true/non-zero when they occur and false/zero in SimulateOwner is *necessary*.
			// Input is polled per Update and SimulateOwner is called per FixedUpdate, so for each simulation step, input can be polled
			// multiple times but processed only once. You want the most proper of the inputs to be processed between each simulation step.

			Vector2 input = value.Get<Vector2>();
			if (input != Vector2.zero)
				lookInput = input;
		}

		private void OnMove(InputValue value)
		{
			moveInput = value.Get<Vector2>();
		}

		private void OnSprint(InputValue value)
		{
			sprintInput = value.isPressed;
		}

		private void OnJump()
		{
			jumpInput = true;
		}

		private void OnAim(InputValue value)
		{
			aimInput = value.isPressed;
		}

		private void OnFire(InputValue value)
		{
			fireInput = value.isPressed;
		}

		private void OnReload()
		{
			reloadInput = true;
		}

		private void OnInteract()
		{
			interactInput = true;
		}
#endif

		private void OnAmmoChanged()
		{
			MessageBroker.Instance.Publish(new AmmoChanged() { Player = Player });
			if (!Player.HasAmmo)
			{
				MessageBroker.Instance.Publish(new AmmoEmptied() { Player = Player });
				if (IsServer)
					ControlHelper.Delay(() => Player.CanReload, state.Reload, cancelSource.Token);
			}
		}

		private void OnPlayerHit(Item item)
		{
			if (Player.IsDead)
				PlayerDied.Post(entity, EntityTargets.Everyone);
		}

		public override void OnEvent(PlayerDied ev)
		{
			Player.Die();
			if (IsLocal)
				entity.Freeze(true);
		}

		public override void SimulateController()
		{
			IPlayerMovementInput moveCommand = PlayerMovement.Create();
			moveCommand.LookInput = lookInput;
			moveCommand.MoveInput = moveInput;
			moveCommand.JumpInput = jumpInput;
			moveCommand.SprintInput = sprintInput;
			moveCommand.AimInput = aimInput;
			moveCommand.AimLocation = Player.LocalAimLocation;
			entity.QueueInput(moveCommand);

			if (fireInput && Player.CanFire)
			{
				entity.QueueInput(PlayerFire.Create());
				if (!Player.CurrentWeapon.Autofire)
					fireInput = false;
			}

			if (reloadInput)
				entity.QueueInput(PlayerReload.Create());

			if (interactInput)
				entity.QueueInput(PlayerInteract.Create());

			lookInput = Vector2.zero;
			jumpInput = false;
			reloadInput = false;
			interactInput = false;
		}

		public override void ExecuteCommand(Command command, bool reset)
		{
			switch (command)
			{
				case PlayerMovement moveCommand:
					HandlePlayerCommand(moveCommand, reset);
					break;

				case PlayerFire fireCommand:
					HandleFireCommand(fireCommand, reset);
					break;

				case PlayerReload reloadCommand:
					HandleReloadCommand(reloadCommand, reset);
					break;

				case PlayerInteract interactCommand:
					HandleInteractCommand(interactCommand, reset);
					break;
			}
		}

		private void HandlePlayerCommand(PlayerMovement command, bool reset)
		{
			if (reset)
			{
				// Impose the Server state on the Local Client whenever possible
				Vector3 rotation = Player.Angles.SetY(command.Result.Rotation);
				Player.Teleport(command.Result.Position, rotation);
				Player.State.Movement = command.Result.Movement;
				Player.State.IsGrounded = command.Result.IsGrounded;
				Player.State.IsSprinting = state.IsSprinting;
				Player.State.IsAiming = state.IsAiming;
				Player.State.AimLocation = state.AimLocation;
			}
			else
			{
				bool isFirst = command.IsFirstExecution;

				// Calculate the new state on the Local Client and Server
				HandleLook(command.Input.LookInput);
				HandleAiming(command.Input.AimInput, command.Input.AimLocation, isFirst);
				HandleJump(command.Input.JumpInput, isFirst);
				HandleMovement(command.Input.MoveInput, command.Input.SprintInput, isFirst);

				command.Result.Position = Player.Position;
				command.Result.Rotation = Player.Angles.y;
				command.Result.Movement = Player.State.Movement;
				command.Result.IsGrounded = Player.State.IsGrounded;
			}
		}

		private void HandleLook(Vector2 look)
		{
			Player.Look(look);
		}

		private void HandleAiming(bool aiming, Vector3 location, bool isFirst)
		{
			Player.State.IsAiming = aiming;
			Player.State.AimLocation = location;

			if (isFirst && IsLocal)
				Player.Aim(aiming);
		}

		private void HandleJump(bool jump, bool isFirst)
		{
			if (!jump)
				return;

			if (!Player.CanJump)
				return;

			Player.Jump();
			if (isFirst)
				Player.PerformJumpAnimation();
		}

		private void HandleMovement(Vector2 move, bool sprint, bool isFirst)
		{
			Player.SetHorizontalInput(move);
			Player.State.IsSprinting = sprint;

			Player.UpdateMovement();

			// Need to set IsGrounded before so that the player doesn't teleport back to ground because of the condition in ApplyGravity
			Player.State.IsGrounded = Player.LocalIsGrounded;
			Player.ApplyGravity();

			if (isFirst)
				Player.UpdateAnimator();
		}

		private void HandleFireCommand(PlayerFire command, bool reset)
		{
			if (reset)
			{
			}
			else
			{
				if (!command.IsFirstExecution)
					return;

				NetworkFire();
			}
		}

		private void NetworkFire()
		{
			if (!IsServer)
				return;

			if (Player.CanFire)
				state.Fire();
		}

		private void LocalFire()
		{
			Player.Fire();
		}

		private void HandleReloadCommand(PlayerReload command, bool reset)
		{
			if (reset)
			{
			}
			else
			{
				if (!command.IsFirstExecution)
					return;

				NetworkReload();
			}
		}

		private void NetworkReload()
		{
			if (!IsServer)
				return;

			if (Player.CanReload)
				state.Reload();
		}

		private void LocalReload()
		{
			Player.Reload();
		}

		private void HandleInteractCommand(PlayerInteract command, bool reset)
		{
			if (reset)
			{
			}
			else
			{
				if (!command.IsFirstExecution)
					return;

				NetworkInteract();
			}
		}

		private void NetworkInteract()
		{
			if (!IsServer)
				return;

			if (Player.CanInteract)
				state.Interact();
		}

		private void LocalInteract()
		{
			Player.Interact();
		}

		private void OnDestroy()
		{
			disposables.Dispose();
			cancelSource.Cancel();
			cancelSource.Dispose();
		}

		public bool IsLocal => entity.HasControl;
		public bool IsServer => entity.IsOwner;
		public bool IsObserver => !IsLocal && !IsServer;
	}
}