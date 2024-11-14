using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Modifier", menuName = "Skills/Modifier/StaminaModifier", order = 0)]
public class StaminaModifier : Modifier
{
    public int staminaModifier;

    public override void ModifyStats()
    {
        player.IncreaseTotalStamina(staminaModifier);
        Debug.Log("Increasing Stamina by: " + staminaModifier);
    }
}
