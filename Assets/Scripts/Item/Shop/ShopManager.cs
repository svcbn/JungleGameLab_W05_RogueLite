using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] List<Item> items;
    [SerializeField] List<ShopItem> displayItems;
    [SerializeField] List<TextMeshProUGUI> displayTexts;

    List<Item> sortItems;

    ShopItem shopItem;
    public Sprite potionImg;

    public Item[,] floorShopItems = new Item[3,4];

    private void Start()
    {
        shopItem = GetComponentInChildren<ShopItem>();
        items = Inventory.Instance.shopItemList;
        sortItems = new List<Item>();

        this.gameObject.SetActive(false);
    }

    [Button]
    public void OpenShop()
    {
        gameObject.SetActive(true);
        Inventory.Instance.gameObject.SetActive(true);
        if (floorShopItems[(GameManager.Instance.player.SelectedFloor / 3) - 1, 0] == null)
        {
           foreach(var i in GetComponentsInChildren<Button>())
            {
                i.interactable = true;
            }
            SettingShop();
        }
        else
        {
            SettingItem();
        }
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
        Inventory.Instance.gameObject.SetActive(false);
        GameManager.Instance.EventPrinting = false;
    }

    public void SettingShop()
    {
        for (int i = 0; i < 4; i++)
        {
            Item item = GetArtifact();
            shopItem.SetItem(displayItems[i], item);
            floorShopItems[(GameManager.Instance.player.SelectedFloor / 3) - 1, i] = item;
        }
        for(int i = 4; i < 6; i++) // 포션
        {
            displayItems[i].GetComponent<Image>().sprite = potionImg;
            displayItems[i].GetComponent<Image>().color = Color.red;
            displayItems[i].gold = 150;
            displayItems[i].id = 0;
            displayItems[i].SetGold(displayItems[i]);
        }
    }

    void SettingItem()
    {
        for (int i = 0; i < 4; i++)
        {
            shopItem.SetItem(displayItems[i], floorShopItems[(GameManager.Instance.player.SelectedFloor / 3) - 1, i]);
        }
        for (int i = 4; i < 6; i++) // 포션
        {
            displayItems[i].GetComponent<Image>().sprite = potionImg;
            displayItems[i].GetComponent<Image>().color = Color.red;
            displayItems[i].gold = 150;
            displayItems[i].id = 0;
            displayItems[i].SetGold(displayItems[i]);
        }
    }

    Item GetArtifact()
    {
        int randNum = Random.Range(1, 101); // 1~ 100;
        sortItems = items.Where(item => item.Rarity == FloorShop((GameManager.Instance.CurrentKnightFloor - 1) / 3, randNum)).ToList(); // �� �� ���� ���� ������ ���� Ȯ�� �Ǵ�
        int index = Random.Range(0, sortItems.Count);
        return sortItems[index];
    }

    private int FloorShop(int floor, int randNum)
    {
        int range = 0;
        int[][] prob = new int[][] { new int[]{ 80, 20, 0 }, new int[] { 30, 45, 25 }, new int[] { 0, 40, 60 } }; // {��, ��, ��}
        for(int i = 0; i < 3; i++)
        {
            range += prob[floor][i];
            if (range >= randNum)
                return i+1;
        }
        return 0;
    }
}