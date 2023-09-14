using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public GameObject panelInventory;
    public Button btnInventory;
    bool isInvenOpen = false;

    private void Start()
    {
        panelInventory.SetActive(false);
        btnInventory.onClick.AddListener(OpenInventory);   
    }

    void OpenInventory()
    {
        Inventory.Instance.newItemImg.gameObject.SetActive(false);
        panelInventory.SetActive(!isInvenOpen);
        isInvenOpen = !isInvenOpen;
    }

}
