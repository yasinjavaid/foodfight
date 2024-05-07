
using Game;
using UnityEngine;
using Photon.Bolt;

interface IModifier 
{
    public int damage { get;  set; }

    public int duration { get; set; }

    public int playerSpeedDecrease { get; set; }

    public int stackCount { get; set; }

    public bool isFriednlyFire { get; set; }

    public string colliderTag { get; set; }
    public NetworkId networkId { get; set; }

    public Vector3 colliderNormal { get; set; }

    public bool isVertical { get; set; }


    public void Relocate(Vector3 pos, Quaternion rot);

}
