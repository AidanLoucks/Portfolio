using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Modifier", menuName = "Skills/Modifier/DamageModifier", order = 0)]
public class NormalDamageModifier : Modifier
{
    public int damageModifier;

    public override void ModifyStats()
    {
        player.IncreaseNormalDamage(damageModifier);
    }
}
