using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnenmyStats : CharacterStats
{
    public override void Die()
    {
        base.Die();


        Destroy(gameObject);
    }
}

   