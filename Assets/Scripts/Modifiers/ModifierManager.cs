using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Linq;
using Game;
using Game.Networking;
using Kit;
using Photon.Bolt;
using UnityEngine;
using UnityEngine.Events;

public struct HitInfoForProjectile
{
    public Vector3 hitpoint;
    public Vector3 hitnormal;
    public NetworkId networkId;
    public string colliderTag;
}

struct OnModifierEffectLifeEnd
{
    
}

public enum ModifierTypes
{
    StickyTaffy,
    SlipperyButter,
    SpringlyMarshmallows
}

public class ModifierManager : Singleton<ModifierManager>
{
    private const string ModifierStickyTaffyId = "StickyTaffyMod";
    private const string ModifierSlipperyButterId = "SlipperyButterMod";
    private const string ModifierSpringlyMarshmallowId = "SpringlyMarshmallowsMod";
 
    public const string ModifierStickyTaffyTag = "StickyTaffy";
    public const string ModifierSlipperyButterTag = "SlipperyButter";
    public const string ModifierSpringlyMarshmallowTag = "SpringlyMarshmallows";

    public const float StickyTaffySpeedTime = 2.0f;
    public const float SlipperyButterSpeedTime = 2.0f;
    public const float SpringlyMarshmallowsSpeedTime = 2.0f;

    [HideInInspector]
    public int ignoreModifierLayer = 1 << 2;
    public Dictionary<string, float> dicModifierSpeedTimer = new Dictionary<string, float>();
    public Dictionary<string, float> dicModifierStartTimer = new Dictionary<string, float>();
    public Dictionary<string, int> stacks = new Dictionary<string, int>();


    private float startTime { get; set; }

    // public StickyTaffy stickyTaffy;
  //  public StickyTaffy stickyTaffy;
    public UnityAction<int> OnModifierReduceSpeed;

    private float updateTime = 0;
    private float serverTime => NetworkManager.Instance.Time;
    private void ReduceTimeOld(ModifierTypes currentModifier)
    {
        if (startTime == 0) startTime = NetworkManager.Instance.Time;

        updateTime = serverTime - startTime;

        if (updateTime >= dicModifierSpeedTimer[ModifierStickyTaffyId])
        {
            MessageBroker.Instance.Publish(new OnModifierEffectLifeEnd());
            startTime = 0;
            dicModifierSpeedTimer[ModifierStickyTaffyId] = 0;
        }
    }
    private void ReduceTime(ModifierTypes currentModifier)
    {
        var currentModifierId = GetModifierId(currentModifier);
        if (dicModifierSpeedTimer[currentModifierId] >= 0.01f)
        {
            if (dicModifierStartTimer[currentModifierId] == 0)
                dicModifierStartTimer[currentModifierId] = serverTime;
            updateTime = serverTime - dicModifierStartTimer[currentModifierId];
            if (updateTime >= dicModifierSpeedTimer[currentModifierId])
            {
                MessageBroker.Instance.Publish(new OnModifierEffectLifeEnd());
                dicModifierStartTimer[currentModifierId] = 0;
                dicModifierSpeedTimer[currentModifierId] = 0;
            }
        }
    }

    public void SpeedTimeChange()
    {
        ControlHelper.EachFrame(() =>
            {
                /*if (dicModifierSpeedTimer[ModifierStickyTaffyId] >= 0.01f)
                {
                  //  ReduceTime();
                }*/
                ReduceTime(ModifierTypes.StickyTaffy);
            }
        );
    }

    
    public string GetModifierId(ModifierTypes modifier)
    {
        switch (modifier)
        {
            case ModifierTypes.StickyTaffy:
                return ModifierStickyTaffyId;
               // break;
            case ModifierTypes.SlipperyButter:
                return ModifierSlipperyButterId;
              //  break;
            case ModifierTypes.SpringlyMarshmallows:
                return ModifierSpringlyMarshmallowId;
             //   break;
            default:
                throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null);
        }
    }

    public void Initialize()
    {
        dicModifierSpeedTimer.Add(ModifierStickyTaffyId, 0);
        dicModifierSpeedTimer.Add(ModifierSlipperyButterId, 0);
        dicModifierSpeedTimer.Add(ModifierSpringlyMarshmallowId, 0);
        dicModifierStartTimer.Add(ModifierStickyTaffyId, 0);
        dicModifierStartTimer.Add(ModifierSlipperyButterId, 0);
        dicModifierStartTimer.Add(ModifierSpringlyMarshmallowId, 0); 
        SpeedTimeChange();
        if(NetworkManager.Instance.IsServer)
            MessageBroker.Instance.Receive<HitInfoForProjectile>().Subscribe(CreateModifierEvent);
    }

    private void CreateModifierEvent(HitInfoForProjectile collInfo)
    {
        var hitEvent = OnProjectileHit.Create();
        hitEvent.HitPoint = collInfo.hitpoint;
        hitEvent.HitNormal = collInfo.hitnormal;
        hitEvent.PlayerToken = collInfo.networkId;
        hitEvent.HitTag = collInfo.colliderTag;
        hitEvent.Send();
    }

    public void IncreaseTimeOfModiferMultiplayer(ModifierTypes modifierTypes)
    {
        dicModifierSpeedTimer[GetModifierId(modifierTypes)] = 
             dicModifierSpeedTimer[GetModifierId(modifierTypes)] 
              + 
              StickyTaffySpeedTime;
    }
}
