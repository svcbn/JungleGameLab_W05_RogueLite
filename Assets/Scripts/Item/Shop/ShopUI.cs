using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;

    private void Awake()
    {
        Instance = this; 
    }

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

    public void findItem(int id)
    {
        foreach(var v in Inventory.Instance.shopItemList)
        {
            if(v.ID == id)
            {
                artifactNameText.text = v.Name;
                artifactMaxHpText.text = $"체력 +{v.MaxHp}";
                artifactDefText.text = $"방어 +{v.Def}";
                artifactArmorPenText.text = $"관통 +{v.ArmorPen}";
                artifactStrText.text = $"파워 +{v.Str}";
                artifactCritText.text = $"치명률 +{v.Crit}";
                artifactDexText.text = $"민첩 +{v.Dex}";
                artifactCritDmgText.text = $"치명타데미지 +{v.CritDmg}";
            }
        }
    }
}
