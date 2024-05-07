using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Linq;
using Game.Items;
using Game.Items.Weapons;
using Game.Items.Modifiers;
using KinematicCharacterController;
using Kit;
using Sirenix.OdinInspector;
using UnityEngine;
using NetworkPlayer = Game.Networking.Bolt.NetworkPlayer;

namespace Game
{
	public class Player: MonoBehaviour, ICharacterController
	{
		private const int BaseLayer = 0;
		private const int LayerStart = BaseLayer + 1;
		private const int LayerCount = 4;
		private const int LayerEnd = LayerCount - 1;
		private const string NoneState = "None";
		private const string AimingState = "Aiming";
		private static readonly int MoveXParam = Animator.StringToHash("MovementX");
		private static readonly int MoveZParam = Animator.StringToHash("MovementZ");
		private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");
		private static readonly int IsAimingParam = Animator.StringToHash("IsAiming");
		private static readonly int JumpParam = Animator.StringToHash("Jump");
		private static readonly int FireParam = Animator.StringToHash("Fire");
		private static readonly int ReloadParam = Animator.StringToHash("Reload");
		private static readonly int HitParam = Animator.StringToHash("Hit");
		private static readonly int DieParam = Animator.StringToHash("Die");
		private static readonly int PickupWeaponParam = Animator.StringToHash("PickupWeapon");
		private static readonly int PickupModifierParam = Animator.StringToHash("PickupModifier");

		private float speedEffectMultiplayer = 1f;
		
		public Transform CameraAnchor;
		public Animator Animator;

		[FoldoutGroup("Movement")]
		public float MoveSpeed = 5f;

		[FoldoutGroup("Movement")]
		public float SprintSpeed = 8f;

		[FoldoutGroup("Movement")]
		public float AimSpeedMultiplier = 0.6f;

		[FoldoutGroup("Movement")]
		public float SpeedSmoothing = 10.0f;

		[FoldoutGroup("Movement")]
		public float JumpHeightMove = 0.6f;

		[FoldoutGroup("Movement")]
		public float JumpHeightSprint = 0.6f;

		[FoldoutGroup("Movement")]
		public float Gravity = -9.8f;

		[FoldoutGroup("Movement")]
		public float GroundGravity = -1f;

		[FoldoutGroup("Camera")]
		public float TurnSpeed = 0.2f;

		[FoldoutGroup("Camera")]
		public float TurnMax = 10.0f;

		[FoldoutGroup("Camera")]
		[Range(-360, 360)]
		public float CameraYMin = -15;

		[FoldoutGroup("Camera")]
		[Range(-360, 360)]
		public float CameraYMax = 50;

		[FoldoutGroup("Behaviour")]
		public float MaxHealth = 100.0f;

		[FoldoutGroup("Behaviour")]
		public Weapon DefaultWeapon;

		[FoldoutGroup("Local")]
		[ShowInInspector]
		[HideInEditorMode]
		[Sirenix.OdinInspector.ReadOnly]
		public Weapon CurrentWeapon { get; protected set; }

		public Modifier CurrentModifier { get; protected set; }

		[ShowInInspector]
		[HideInEditorMode]
		[Sirenix.OdinInspector.ReadOnly]
		public PlayerState State { get; set; }

		public event Action<Item> OnHit;
		public event Action OnKilled;

		public new Transform transform { get; private set; }
		public KinematicCharacterMotor Controller { get; private set; }
		public List<IInteractable> Interactables { get; } = new List<IInteractable>();

		private Weapon pickupWeapon;
		private Modifier pickupModifier;
		private Vector3 kinematicMovement;
		private Quaternion kinematicRotation;

		private void Awake()
		{
			transform = base.transform;
			Controller = GetComponent<KinematicCharacterMotor>();
			Controller.CharacterController = this;
		}

		public void Initialize()
		{
			State.Health = MaxHealth;
			EquipDefault();
		}

		private void EquipDefault()
		{
			if (DefaultWeapon != null)
			{
				Weapon instance = (Weapon) DefaultWeapon.Spawn();
				Equip(instance);
			}
		}

		public void Look(Vector2 delta)
		{
			delta.x = Mathf.Clamp(delta.x, -TurnMax, TurnMax);
			delta.y = Mathf.Clamp(delta.y, -TurnMax, TurnMax);

			// Rotate the player body for horizontal view
			Vector3 rotation = kinematicRotation.eulerAngles;
			float angle = delta.x * TurnSpeed;
			angle = rotation.y + angle;
			rotation.y = angle;
			kinematicRotation = rotation.ToQuaternion();
			//Rotation = rotation.ToQuaternion();

			// Rotate the camera anchor for vertical view
			if (CameraAnchor == null)
				return;

			rotation = CameraAnchor.localRotation.eulerAngles;
			angle = delta.y * TurnSpeed;
			angle = rotation.x - angle;
			angle = MathHelper.ClampDeltaAngle(angle);
			angle = Mathf.Clamp(angle, CameraYMin, CameraYMax);
			rotation.x = angle;
			CameraAnchor.localRotation = rotation.ToQuaternion();
		}

		public void SetHorizontalInput(Vector2 input)
		{
			SetHorizontalMovement(input.normalized);
		}

		public void SetHorizontalMovement(Vector2 by)
		{
			State.Movement = State.Movement.SetX(by.x).SetZ(by.y);
		}

		public void Move(Vector3 movement, bool sprinting, bool aiming)
		{
			// Apply sprint factor
			float speed = sprinting ? SprintSpeed : MoveSpeed;
			speed *= speedEffectMultiplayer;
			movement.x *= speed;
			movement.z *= speed;
			
			

			// Apply aim factor
			if (aiming)
			{
				movement.x *= AimSpeedMultiplier;
				movement.z *= AimSpeedMultiplier;
			}

			//movement *= GameManager.Instance.FixedDeltaTime;
			movement = Rotation * movement;
			kinematicMovement = movement;
			//Controller.Move(movement);
		}

		public void UpdateMovement()
		{
			Move(State.Movement, State.IsSprinting, State.IsAiming);
		}

		public void UpdateAnimator()
		{
			float smoothing = SpeedSmoothing * GameManager.Instance.FixedDeltaTime;
			Vector3 animatorInput = State.Movement;

			if (State.IsSprinting)
				animatorInput *= 2;

			Animator.SetFloat(MoveXParam, Mathf.Lerp(Animator.GetFloat(MoveXParam), animatorInput.x, smoothing));
			Animator.SetFloat(MoveZParam, Mathf.Lerp(Animator.GetFloat(MoveZParam), animatorInput.z, smoothing));
			Animator.SetBool(IsGroundedParam, State.IsGrounded);
			Animator.SetBool(IsAimingParam, State.IsAiming);
		}

		public void ApplyGravity()
		{
			float verticalMovement = State.Movement.y;
			if (State.IsGrounded)
			{
				// Cap to a stable gravity on ground
				verticalMovement = GroundGravity;
			}
			else
			{
				verticalMovement += Gravity * GameManager.Instance.FixedDeltaTime;

				// Cap gravity so it doesn't increase infinitely when falling
				if (verticalMovement < Gravity)
					verticalMovement = Gravity;
			}
			State.Movement = State.Movement.SetY(verticalMovement);
		}

		public void Teleport(Vector3 position)
		{
			Position = position;
		}

		public void Teleport(Vector3 position, Quaternion rotation)
		{
			Teleport(position);
			Rotation = rotation;
		}

		public void Teleport(Vector3 position, Vector3 angles)
		{
			Teleport(position);
			Angles = angles;
		}

		private void OnTriggerEnter(Collider other)
		{
			IInteractable interactable = other.GetComponent<IInteractable>();
			Interactables.Add(interactable);
		
		}
		private void OnTriggerStay(Collider other)
		{
			if (other.CompareTag(ModifierManager.ModifierStickyTaffyTag))
			{
				var mod = other.gameObject.GetComponent<IModifier>();
				var managedSpeed = mod.playerSpeedDecrease / 100.0f;
				speedEffectMultiplayer = 1 - managedSpeed;
			}
		}
		private void OnTriggerExit(Collider other)
		{
			IInteractable interactable = other.GetComponent<IInteractable>();
			Interactables.Remove(interactable);
			switch (other.gameObject.tag)
			{
				case ModifierManager.ModifierStickyTaffyTag:
					var modifier = other.gameObject.GetComponent<IModifier>();
					if (modifier.colliderTag == "verticalPlane") //Vertical plane must save in const or same it in some new state
					{
						//	WalkPlayerOnVerticalPlane();
						//	return;
					}
					if (!GameManager.Instance.isFriendlyFire)
					{
						SpeedChangeInModifier(modifier.playerSpeedDecrease, ModifierTypes.StickyTaffy);
					}
					else if (modifier.networkId == gameObject.GetComponent<NetworkPlayer>().entity.NetworkId)//must remove getcom and use it from cached value
					{
						SpeedChangeInModifier(modifier.playerSpeedDecrease, ModifierTypes.StickyTaffy);
					}
					break;
				case ModifierManager.ModifierSlipperyButterTag:
					break;
				case ModifierManager.ModifierSpringlyMarshmallowTag:
					break;
				default:
					break;
			}
		}
		private void SpeedChangeInModifier(int speed, ModifierTypes currentMod)
		{
			var managedSpeed = speed / 100.0f;
			speedEffectMultiplayer = 1 - managedSpeed;
			// adding time values over key value pair in dic
			ModifierManager.Instance.IncreaseTimeOfModiferMultiplayer(currentMod);
			MessageBroker.Instance.Receive<OnModifierEffectLifeEnd>().Subscribe(msg =>
			{
				speedEffectMultiplayer = 1;
				Debug.Log("BackToNormalSpeed");
			});
			Debug.Log(" SpeedReduced"+speedEffectMultiplayer + "::: Speed" + speed);
			//ControlHelper.Delay(2.0f, SpeedEffectRevert);
		}
		public void Interact()
		{
			IInteractable interactable = Interactables.Lowest(i => (i.Position - Position).sqrMagnitude);
			interactable.Interact(this);
		}

		public void Pickup(Weapon weapon)
		{
			pickupWeapon = weapon;
			GameManager.Instance.DeactivateInput(this);
			Animator.SetTrigger(PickupWeaponParam);
		}

		public void Pickup(Modifier modifier)
		{
			pickupModifier = modifier;
			GameManager.Instance.DeactivateInput(this);
			Animator.SetTrigger(PickupModifierParam);
		}

		public void TriggerDropWeapon()
		{
			if (HasWeapon)
				DropWeapon();
		}

		public void TriggerWeaponPickup()
		{
			Equip(pickupWeapon);
			pickupWeapon = null;
			GameManager.Instance.ActivateInput(this);
		}

		public void TriggerModifierPickup()
		{
			Equip(pickupModifier);
			pickupModifier = null;
			GameManager.Instance.ActivateInput(this);
		}

		public void Equip(Weapon weapon)
		{
			CurrentWeapon = weapon;
			weapon.Attach(this);
			MessageBroker.Instance.Publish(new WeaponEquipped { Player = this, Weapon = weapon });
		}
		public void Equip(Modifier modifier)
		{
			CurrentModifier = modifier;
			modifier.Attach(this);
			Debug.Log("OnmodifierPick");
		}

		public void DropWeapon()
		{
			Destroy(CurrentWeapon.gameObject);
		}

		public void PerformJumpAnimation()
		{
			Animator.SetTrigger(JumpParam);
		}

		public void Aim(bool aim)
		{
			CameraManager.Instance.Switch(aim);
		}

		public void Jump()
		{
			Controller.ForceUnground();
			State.Movement = State.Movement.SetY(CalculateJumpForce());
		}

		public float CalculateJumpForce()
		{
			float height = State.IsSprinting ? JumpHeightSprint : JumpHeightMove;
			return Mathf.Sqrt(height * -Gravity);
		}

		public void Fire()
		{
			Animator.SetTrigger(FireParam);
		}

		public void TriggerFire()
		{
			CurrentWeapon.Use();
		}

		public void Reload()
		{
			Animator.SetTrigger(ReloadParam);
		}

		public void TriggerReload()
		{
			CurrentWeapon.Reload();
		}

		public void Hit(Item item, float damage)
		{
			if (IsDead)
				return;

			if (item.Owner == this)
				return;

			TakeDamage(damage);
			Animator.SetTrigger(HitParam);
			OnHit?.Invoke(item);
			MessageBroker.Instance.Publish(new PlayerHit() { Player = this, With = item });
		}

		public void TakeDamage(float damage)
		{
			float newHealth = State.Health - damage;
			if (newHealth < 0)
				newHealth = 0;
			State.Health = newHealth;
		}

		public void Die()
		{
			Animator.SetTrigger(DieParam);
			GameManager.Instance.DeactivateInput(this);
			OnKilled?.Invoke();
			MessageBroker.Instance.Publish(new PlayerKilled() { Player = this });
		}

		public bool HasAmmo => State.Ammo > 0;
		public Vector3 Position { get => transform.position; set => transform.position = value; }
		public Quaternion Rotation { get => transform.rotation; set => transform.rotation = value; }
		public Vector3 Angles { get => transform.rotation.eulerAngles; set => transform.rotation = value.ToQuaternion(); }

		public bool LocalIsGrounded => Controller.GroundingStatus.IsStableOnGround;
		public Vector3 LocalAimLocation => CameraManager.Instance.LookAt;

		public bool CanJump => State.IsGrounded;

		[FoldoutGroup("Local")]
		[ShowInInspector]
		[HideInEditorMode]
		public bool HasWeapon => CurrentWeapon != null;

		[FoldoutGroup("Local")]
		[ShowInInspector]
		[HideInEditorMode]
		public bool IsBusy
		{
			get
			{
				for (int i = LayerStart; i < LayerCount; i++)
					if (Animator.IsInTransition(i) ||
						!Animator.GetCurrentAnimatorStateInfo(i).IsName(NoneState) &&
						!Animator.GetCurrentAnimatorStateInfo(i).IsName(AimingState))
						return true;
				return false;
			}
		}

		public bool CanFire => !IsBusy              && HasWeapon && CurrentWeapon.CanUse;
		public bool CanReload => State.IsGrounded   && !IsBusy   && HasWeapon && CurrentWeapon.CanReload;
		public bool CanInteract => State.IsGrounded && !IsBusy   && Interactables.Count > 0;
		public bool IsAlive => State.Health > 0;
		public bool IsDead => !IsAlive;

		#region ICharacterController
		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
		{
			currentRotation = kinematicRotation;
		}

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
		{
			currentVelocity = kinematicMovement;
		}

		public void BeforeCharacterUpdate(float deltaTime)
		{
		}

		public void PostGroundingUpdate(float deltaTime)
		{
		}

		public void AfterCharacterUpdate(float deltaTime)
		{
		}

		public bool IsColliderValidForCollisions(Collider coll)
		{
			return true;
		}

		public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
		{
		}
		public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
		{
			
		}
		public void ProcessHitStabilityReport(Collider hitCollider,
											  Vector3 hitNormal,
											  Vector3 hitPoint,
											  Vector3 atCharacterPosition,
											  Quaternion atCharacterRotation,
											  ref HitStabilityReport hitStabilityReport)
		{
		}

		public void OnDiscreteCollisionDetected(Collider hitCollider)
		{
		}
		#endregion
	}
}