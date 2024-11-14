using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class Skill : ScriptableObject
{
    public Image iconImg; // This will turn into the icon for the respective skill
    public string skillName;
    public int cost = 2;
    public int tier = 0;

    protected bool purchased = false;
    protected Color skillColor;
    protected Player player;
    protected SkillData data;

    protected GameCanvas gameCanvas;
    protected Image image;
    private TextMeshProUGUI costText;

    // Gets called after being instantiated. Think start
    public virtual void SetUpSkill(SkillData data, Image img, Player player, GameCanvas gameCanvas)
    {
        skillColor = data.Color;
        this.data = data;
        image = img;

        if(data.depth == 0)
            PurchaseAble();

        costText = img.GetComponentInChildren<TextMeshProUGUI>();
        costText.text = cost.ToString();

        this.player = player;
        this.gameCanvas = gameCanvas;
    }

    public void HandleClick()
    {
        if (player.Exp >= cost && data.depth <= 0)
        {
            SkillUnlocked();
        }
    }

    public virtual void SkillUnlocked()
    {
        purchased = true;
        OnPointerExit();
        data.SkillPurchased();
        player.LoseExperience(cost);
        image.color = image.color * 2f;
        costText.text = "";
    }

    public virtual void OverrideUnlock()
    {
        image.color = skillColor;
        costText.text = "";
    }

    public void PurchaseAble()
    {
        image.color = new Color(skillColor.r * 0.5f, skillColor.g * 0.5f, skillColor.b * 0.5f, 1);
    }


    public void OnPointerEnter()
    {
        if(data.depth <= 0 && !purchased) 
        { 
            gameCanvas.MenuController.SetSkillDescription(skillName, cost);
        }
    }

    public void OnPointerExit()
    {
        gameCanvas.MenuController.DisableSkillDescription();
    }
}
