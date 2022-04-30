using GameCreator.Melee;
using GameCreator.Variables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEffectSpawn : MonoBehaviour
{
    private CharacterMelee characterMelee;

    private void Start()
    {
        characterMelee = gameObject.GetComponentInParent<CharacterMelee>();
    }

    // Generic animation event
    private void InvokeAction(string name)
    {
        if (this.characterMelee == null) return;
        this.characterMelee.InvokeAction(name);
    }

    //void InstantiateEffect(int EffectNumber)
    //{
    //    var element = (float)VariablesManager.GetGlobal("Elemental");

    //    EffectInfo[] effects = null;
    //    if (element == 1f)
    //        effects = PyroEffects;
    //    if (element == 2f)
    //        effects = AeroEffects;
    //    if (element == 3f)
    //        effects = HydroEffects;
    //    if (element == 4f)
    //        effects = GeoEffects;


    //    var instance = Instantiate(effects[EffectNumber].Effect, effects[EffectNumber].StartPositionRotation.position, effects[EffectNumber].StartPositionRotation.rotation);

    //    if (effects[EffectNumber].UseLocalPosition)
    //    {
    //        instance.transform.parent = effects[EffectNumber].StartPositionRotation.transform;
    //        instance.transform.localPosition = Vector3.zero;
    //        instance.transform.localRotation = new Quaternion();
    //    }
    //    Destroy(instance, effects[EffectNumber].DestroyAfter);
    //}
}
