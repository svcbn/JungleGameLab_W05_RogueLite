using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UsePotion : MonoBehaviour
{
    public void UsePotionFunction(TMP_Text text)
    {
        if(Inventory.Instance.HealingPotion > 0)
        {
            text.text = $"{--Inventory.Instance.HealingPotion}";
            GameManager.Instance.player.Status.CurrentHp += GameManager.Instance.player.Status.MaxHp / 3; //최대 체력의 1/3회복
        }
    }
}
