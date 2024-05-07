using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Linq;
using Game;
using Game.Networking;
using Kit;
using PaintIn3D;
using UnityEngine;
using Random = UnityEngine.Random;
using Photon.Bolt;
using UdpKit;
using Event = Photon.Bolt.Event;

public class ModifierHelper : GlobalEventListener
{
    public GameObject stickyTaffyPrefab;
    public override void BoltStartBegin()
    {
        BoltNetwork.SetPrefabPool(new ModifierPooler());
    }
    [SerializeField] private IHitPoint hitPointCache;
    public enum OrientationType
    {
        WorldUp,
        CameraUp
    }

   
    public override void OnEvent(OnProjectileHit evnt)
    {
        if (NetworkManager.Instance.IsClient)
        {
            ShowModifier(evnt.HitPoint, evnt.HitNormal, evnt.HitTag, evnt.PlayerToken);
            if (evnt.HitTag == "player")
            {
               
            }
        }
        if (NetworkManager.Instance.IsServer)
        {
           StacksMaking(); 
        }
    }
    public void ShowModifier(Vector3 hitPoint, Vector3 hitNormal, string colliderTag, NetworkId id)
    {
        var finalUp = orientation == OrientationType.CameraUp ? P3dHelper.GetCameraUp() : Vector3.up;
     
        var finalPosition = hitPoint;
        var finalRotation = Quaternion.LookRotation(-hitPoint, finalUp);
        var seed = Random.Range(int.MinValue, int.MaxValue);
        var tempObj = BoltNetwork.Instantiate(BoltPrefabs.StickyTaffyModifier, finalPosition, finalRotation);//GetStickyTaffyFromPooler();
        var modifer = tempObj.GetComponent<IModifier>();
        modifer.colliderTag = colliderTag;
        modifer.colliderNormal = hitNormal;
        var yOfNormal = Mathf.Abs( hitNormal.y);
        modifer.isVertical = false || yOfNormal >= 0.0f && yOfNormal <= 0.35f;
        if (GameManager.Instance.isFriendlyFire)
        {
            modifer.networkId = id;
        }
        Debug.Log("Created && Vertical Plane: " + modifer.isVertical);
        //   hitPointCache.HandleHitPoint(false, 0, 0, seed, finalPosition, finalRotation);

        //modifier.Relocate(finalPosition, finalRotation);
    }
    public void StacksMaking()
    {
        
    }
    public OrientationType Orientation { set { orientation = value; } get { return orientation; } } [SerializeField] private OrientationType orientation;
    // Start is called before the first frame update

    private void Awake()
    {
        hitPointCache =   gameObject.GetComponent<IHitPoint>();
        ModifierManager.Instance.Initialize();
        if (!NetworkManager.Instance.IsServer && !NetworkManager.Instance.IsClient && !NetworkManager.Instance.IsSingleplayer)
            MessageBroker.Instance.Receive<HitInfoForProjectile>().Subscribe(ShowModifierOnLocal);
    }

    public void ShowModifierOnLocal(HitInfoForProjectile collhit)
    {
        var finalUp = orientation == OrientationType.CameraUp ? P3dHelper.GetCameraUp() : Vector3.up;
     
        var finalPosition = collhit.hitpoint;
        var finalRotation = Quaternion.LookRotation(-collhit.hitpoint, finalUp);
        var seed = Random.Range(int.MinValue, int.MaxValue);
        var tempObj = Instantiate(stickyTaffyPrefab, finalPosition, finalRotation);//GetStickyTaffyFromPooler();
       
        var modifer = tempObj.GetComponent<IModifier>();
        modifer.colliderTag = collhit.colliderTag;
        modifer.colliderNormal = collhit.hitnormal;
        var yOfNormal = Mathf.Abs( collhit.hitnormal.y);
        modifer.isVertical = false || yOfNormal >= 0.0f && yOfNormal <= 0.35f;
        if (GameManager.Instance.isFriendlyFire)
        {
            modifer.networkId = collhit.networkId;
        }
        Debug.Log("Created && Vertical Plane: " + modifer.isVertical);
    }

    private StickyTaffy GetStickyTaffyFromPooler()
    {
        //somepooler
       // var Modifier = Instantiate(ModifierManager.Instance.stickyTaffy);
       return null;
    }
    
}


