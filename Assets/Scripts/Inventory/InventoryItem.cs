using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    Image itemIcon;
    public CanvasGroup canvasGroup { get; private set; }
    [OnInspectorGUI]
    public Item Artifact { get; set; }
    public InventorySlot activeSlot { get; set; }
    Color[] outLineColor = new Color[] {Color.white, Color.black, Color.blue, Color.magenta, Color.yellow };

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        itemIcon = GetComponent<Image>();
    }

    public void Initialize(Item item, InventorySlot parent)
    {
        activeSlot = parent;
        activeSlot.myItem = this;
        Artifact = item;
        if (itemIcon == null)
        {
            itemIcon = GetComponent<Image>();
        }
        itemIcon.sprite = item.Sprite;
        GetComponent<Outline>().effectColor = outLineColor[GetComponent<InventoryItem>().Artifact.Rarity];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            Inventory.Instance.SetCarriedItem(this);
        }
    }
}
