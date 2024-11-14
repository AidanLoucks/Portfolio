using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Modifier : Skill
{
    public override void SkillUnlocked()
    {
        base.SkillUnlocked();
        ModifyStats();
    }

    public abstract void ModifyStats();
}
