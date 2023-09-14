using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactChangeAmount
{
    public float maxHp = 0;
    public float str = 0;
    public float dex = 0;
    public float def = 0;
    public float crit = 0;
    public float critDmg = 0;
    public float armorPen = 0;
}

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public static InventoryItem carriedItem;

    public InventorySlot[] inventorySlots;

    [SerializeField] Transform draggablesTransform;
    [SerializeField] InventoryItem itemPrefab;

    public List<Item> shopItemList;

    public List<Item> dropItemList;

    public InventoryItem[] artifacts;

    public ArtifactChangeAmount changeAmount = new();

    public TMP_Text potionText;
    public TMP_Text goldText;

    public Image newItemImg;

    #region status
    public TMP_Text artifactNameText;
    public TMP_Text artifactMaxHpText;
    public TMP_Text artifactDefText;
    public TMP_Text artifactArmorPenText;
    public TMP_Text artifactStrText;
    public TMP_Text artifactCritText;
    public TMP_Text artifactDexText;
    public TMP_Text artifactCritDmgText;
    #endregion

    public bool IsSwordEquiped { get; private set; } = false;
    public bool IsHelmetEquiped { get; private set; } = false;

    int _gold;
    public int Gold
    {
        get
        {
            return _gold;
        }
        set
        {
            _gold = value;
            goldText.text = $"{_gold} Gold";
        }
    }

    int _healPotion;
    public int HealingPotion
    {
        get
        {
            return _healPotion;
        }
        set
        {
            _healPotion = value;
            potionText.text = $"{_healPotion}";
        }
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        artifacts = new InventoryItem[25];
        Gold = 500; // 초기 골드 TODO 0으로 초기화
        HealingPotion = 3; // 힐링 포션 갯수 TODO 0으로 초기화 + text 0 설정
    }

    private void Start()
    {
    }

    [Button]
    public string AddItem(int ID, bool isDrop)
    {
        Item _item = new Item();
        if (isDrop) // 드롭 아이템
        {
            newItemImg.gameObject.SetActive(true);
            int index = dropItemList.FindIndex(item => item.ID == ID);
            _item = Instantiate(dropItemList[index]);
        }
        else // 상점 아이템
        {
            int index = shopItemList.FindIndex(item => item.ID == ID);
            _item = Instantiate(shopItemList[index]); 
        }


        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].myItem == null)
            {
                InventoryItem artifact = Instantiate(itemPrefab, inventorySlots[i].transform);
                artifact.Initialize(_item, inventorySlots[i]);
                artifacts[i] = artifact;
                return artifact.Artifact.Name;
                break;
            }
        }
        return null;
    }
    public int CheckCurrentItemCount()
    {
        int cnt = 0;
        for(int i = 0; i < 15;i++)
        {
            if (inventorySlots[i].myItem != null)
            {
                cnt++;
            }
        }
        return cnt;
    }

    public void EquipmentCheck(InventoryItem item, int index)
    {
        if (index >= 15 && index <= 22)
        {
            item.Artifact.IsEquipped = true;
            if (item.Artifact.ID == 53)
            {
                IsSwordEquiped = true;
            }
            if(item.Artifact.ID == 54)
            {
                IsHelmetEquiped = true;
            }
        }
        else
        {
            item.Artifact.IsEquipped = false;
            if (item.Artifact.ID == 53)
            {
                IsSwordEquiped = false;
            }
            if (item.Artifact.ID == 54)
            {
                IsHelmetEquiped = false;
            }
        }
        UpdateStatus();
        SynergyCheck();
    }

    
    
    public void UpdateStatus()
    {
        GameManager.Instance.ApplyArtifactStats(changeAmount, true);

        ResetEquipmentValue();

        for(int i = 0; i < artifacts.Length; i++)
        {
            if (artifacts[i] == null) continue;
            else
            {
                if (artifacts[i].Artifact.IsEquipped)
                {
                    changeAmount.maxHp += artifacts[i].Artifact.MaxHp;
                    changeAmount.str += artifacts[i].Artifact.Str;
                    changeAmount.dex += artifacts[i].Artifact.Dex;
                    changeAmount.def += artifacts[i].Artifact.Def;
                    changeAmount.crit += artifacts[i].Artifact.Crit;
                    changeAmount.critDmg += artifacts[i].Artifact.CritDmg;
                    changeAmount.armorPen += artifacts[i].Artifact.ArmorPen;
                }
            }
        }

        GameManager.Instance.ApplyArtifactStats(changeAmount, false);
    }

    void ResetEquipmentValue()
    {
        changeAmount.maxHp = 0;
        changeAmount.str = 0;
        changeAmount.dex = 0;
        changeAmount.def = 0;
        changeAmount.crit = 0;
        changeAmount.critDmg = 0;
        changeAmount.armorPen = 0;
    }

    void SynergyCheck()
    {

    }

    private void Update()
    {
        if (carriedItem == null) return;
        carriedItem.transform.position = Input.mousePosition;
    }

    public void SetCarriedItem(InventoryItem item)
    {
        if(carriedItem != null)
        {
            item.activeSlot.SetItem(carriedItem);
        }
        ShowItemStatus(item);

        carriedItem = item;
        carriedItem.canvasGroup.blocksRaycasts = false;
        item.transform.SetParent(draggablesTransform);
       
        int _index = Array.FindIndex<InventoryItem>(artifacts, element => element == item);
        artifacts[_index] = null;
    }
    void ShowItemStatus(InventoryItem item)
    {
        artifactNameText.text = item.Artifact.Name;
        artifactMaxHpText.text = $"체력 +{item.Artifact.MaxHp}";
        artifactDefText.text = $"방어 +{item.Artifact.Def}";
        artifactArmorPenText.text = $"관통 +{item.Artifact.ArmorPen}";
        artifactStrText.text = $"파워 +{item.Artifact.Str}";
        artifactCritText.text = $"치명률 +{item.Artifact.Crit}";
        artifactDexText.text = $"민첩 +{item.Artifact.Dex}";
        artifactCritDmgText.text = $"치명타데미지 +{item.Artifact.CritDmg}";
    }

    public void HideItemStatus()
    {
        artifactNameText.text = "";
        artifactMaxHpText.text = "체력";
        artifactDefText.text = "방어";
        artifactArmorPenText.text = "관통";
        artifactStrText.text = "파워";
        artifactCritText.text = "치명률";
        artifactDexText.text = "민첩";
        artifactCritDmgText.text = "치명타데미지";
    }
}
