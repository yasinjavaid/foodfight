using System.Collections;
using System.Collections.Generic;
using Game;
using Photon.Bolt;
using UnityEngine;

public class SpringlyMarshmallows : IModifier
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    public Player player { get; set; }
    public void Relocate(Vector3 pos, Quaternion rot)
    {
        throw new System.NotImplementedException();
    }
}
