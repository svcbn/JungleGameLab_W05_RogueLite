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
                artifactMaxHpText.text = $"ü�� +{v.MaxHp}";
                artifactDefText.text = $"��� +{v.Def}";
                artifactArmorPenText.text = $"���� +{v.ArmorPen}";
                artifactStrText.text = $"�Ŀ� +{v.Str}";
                artifactCritText.text = $"ġ��� +{v.Crit}";
                artifactDexText.text = $"��ø +{v.Dex}";
                artifactCritDmgText.text = $"ġ��Ÿ������ +{v.CritDmg}";
            }
        }
    }
}
