// using Mirror;
// using UnityEngine;
//
// namespace Game.Networking.Mirror
// {
// 	public class NetworkPlayer: NetworkBehaviour
// 	{
// 		private Player player;
// 		private static readonly int State = Animator.StringToHash("State");
//
// 		private void Awake()
// 		{
// 			player = GetComponent<Player>();
// 		}
//
// 		public void Start()
// 		{
// 			if (!isLocalPlayer)
// 				return;
//
// 			GameManager.Instance.GainControl(player);
// 		}
//
// 		private void FixedUpdate()
// 		{
// 			if (!isLocalPlayer)
// 				return;
//
// 			player.Animator.SetInteger(State, InputManager.Forward || InputManager.Backward || InputManager.Left || InputManager.Right ? 1 : 0);
// 			transform.position += InputManager.CalculateMovement(InputManager.GetMovementInput(), player.Speed, Time.deltaTime);
// 			MovePlayer(InputManager.GetMovementInput());
// 		}
//
// 		[Command]
// 		public void MovePlayer(Vector2 input)
// 		{
// 			player.Animator.SetInteger(State, input.sqrMagnitude > 0.1f ? 1 : 0);
// 			player.Move(InputManager.CalculateMovement(input, player.MoveSpeed, Time.fixedDeltaTime));
// 		}
// 	}
// }