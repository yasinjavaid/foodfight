using UnityEngine;

interface IModifierCollision
{
    public  void OnCollision(Collider col);
}

interface IResetTimeForModifier
{
    public void ModifierResetTime();
}
