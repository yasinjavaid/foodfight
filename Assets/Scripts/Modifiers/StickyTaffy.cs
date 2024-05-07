using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Networking;
using UnityEngine;
using Photon.Bolt;
using Kit;
using NetworkPlayer = Game.Networking.Bolt.NetworkPlayer;

public class StickyTaffy : EntityEventListener<IStickyTaffyModifier> , IModifier
{
    private double startTime;

    private double updatedTime;
    
    private bool isDestoyed;
    
    // Start is called before the first frame update
  
    // Update is called once per frame
   
    /*public void OnCollision(Collider col)
    {
        switch (col.gameObject.tag)
        {
            case "Player":
                ModifierManager.Instance.OnModifierReduceSpeed?.Invoke(playerSpeedDecrease);
                break;
            default:
                break;
        }
    }*/
    public void Awake()
    {
      //  state.OnCreateModifier += () => { };
        startTime = NetworkManager.Instance.Time;
        duration = 10;
        playerSpeedDecrease = 25;
    }

    public override void Attached()
    {
       
            state.OnCreateModifier += () => { };
            startTime = NetworkManager.Instance.Time;
            duration = 10;
            playerSpeedDecrease = 25;
      
       
       
            /*Debug.Log("Test");
         //   NetworkManager.Instance.ServerWaitTimeQueues(new ServerWaitTime(5.0f, () =>  Debug.Log("test destroy")));
            NetworkManager.Instance.ServerWaitTimeQueues(new ServerWaitTime(duration, () =>
            { 
                Debug.Log("test destroy");
                BoltNetwork.Destroy(this.gameObject);
            }));*/
      
    }
    public void Update()
    {
        if (isDestoyed) return;
        updatedTime = NetworkManager.Instance.Time - startTime;
        if (updatedTime >= duration)
        {
            isDestoyed = true;
            BoltNetwork.Destroy(this.gameObject);
        }
    }
    public int damage { get; set; }
    public int duration { get; set; }
    public int playerSpeedDecrease { get; set; }
    public int stackCount { get; set; }
    public bool isFriednlyFire { get; set; }
    public string colliderTag { get; set; }
    public NetworkId networkId { get; set; }
    public Vector3 colliderNormal { get; set; }
    public bool isVertical { get; set; }

    public void Relocate(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }
}
