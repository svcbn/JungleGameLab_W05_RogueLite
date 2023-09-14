using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public InventoryItem myItem { get; set; }
    public bool isLock = false;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Inventory.carriedItem == null || this.isLock) return;
            SetItem(Inventory.carriedItem);
            Inventory.Instance.HideItemStatus();
        }
    }
    public void SetItem(InventoryItem item)
    {
        Inventory.carriedItem = null;

        item.activeSlot.myItem = null;
        
        myItem = item;
        myItem.activeSlot = this;
        myItem.transform.SetParent(transform);
        myItem.canvasGroup.blocksRaycasts = true;

        int index = Array.FindIndex<InventorySlot>(Inventory.Instance.inventorySlots, element => element == this);
        if (index != 23)
        {
            Inventory.Instance.artifacts[index] = myItem;
        }
        else
        {
            Destroy(myItem.gameObject);
        }

        Inventory.Instance.EquipmentCheck(myItem, index);
    }
}
