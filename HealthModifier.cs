using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Modifier", menuName = "Skills/Modifier/HealthModifier", order = 0)]
public class HealthModifier : Modifier
{
    public int healthModifier;

    public override void ModifyStats()
    {
        player.IncreaseTotalHealth(healthModifier);
        // Debug.Log("Increasing Health by: " + healthModifier);
    }
}
