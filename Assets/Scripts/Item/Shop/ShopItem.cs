using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public int id;
    [HideInInspector] public int gold;
    public GameObject panel;

    

    public void SetItem(ShopItem shopItem, Item item)
    {
        shopItem.GetComponent<Image>().sprite = item.Sprite;
        shopItem.id = item.ID;
        shopItem.gold = item.Rarity * 200 + Random.Range(-20, 21); // Èñ±Íµµ * 200 +- 20;
        SetGold(shopItem);
    }

    public void SetGold(ShopItem item)
    {
        item.GetComponentInChildren<TMP_Text>().text = $"{item.gold}G";
    }

    public void BuyItem() // »óÁ¡ Ä­ ´­·¶À» ¶§ ±¸¸Å
    {
        if(this.id == 0 && Inventory.Instance.Gold >= this.gold) // Æ÷¼Ç ±¸¸Å
        {
            Inventory.Instance.HealingPotion += 1;
            Inventory.Instance.Gold -= this.gold;
        }
        else if (Inventory.Instance.CheckCurrentItemCount() < 15 && Inventory.Instance.Gold >= this.gold)
        {
            Inventory.Instance.Gold -= this.gold;
            Inventory.Instance.AddItem(this.id, false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (this.id == 0) return;
        ShopUI.Instance.findItem(this.id);
        panel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        panel?.SetActive(false);
    }

    

}
