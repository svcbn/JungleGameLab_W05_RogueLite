using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public int Level { get; set; }
    public string Name { get; set; }
    public Status Status { get; set; }
    
    public Sprite Sprite;

    public Monster(int level, string name, Status status, Sprite sprite)
    {
        Level = level;
        Name = name;
        Status = status;
        Sprite = sprite;
    }

    public string DropMonsterItem(Monster monster)
    {
        int dropProb = 0;
        List<Item> dropItem = new List<Item>();
        GetMoney(monster.Level);
        switch (monster.Name)
        {
            case string n when n == "슬라임" || n == "고블린":
                dropProb = 20;
                break;
            case string n when n == "오크" || n == "스텀프":
                dropProb = 10;
                break;
            case string n when n == "방패병" || n == "토글" || n == "드레이크":
                dropProb = 5;
                break;
            case string n when n == "러글" || n == "데스나이트":
                dropProb = 100;
                break;
        }
        if (Random.Range(1, 101) <= dropProb)
        {
            dropItem = Inventory.Instance.dropItemList.Where(item => item.Name.Substring(1, monster.Name.Length).Equals(monster.Name)).ToList();
            int index = Random.Range(0, dropItem.Count);
            string name = Inventory.Instance.AddItem(dropItem[index].ID, true);
            string colorName = "";
            if(dropItem[index].Rarity == 1)
                colorName = $"<color=#000000>{name}</color>";
            else if(dropItem[index].Rarity == 2)
                colorName = $"<color=#0000FF>{name}</color>";
            else if(dropItem[index].Rarity == 3)
                colorName = $"<color=#9800FF>{name}</color>";
            else if(dropItem[index].Rarity == 4)
                colorName = $"<color=#FF0000>{name}</color>";
            return colorName;
        }
        return null;
    }
    
    void GetMoney(int monsterlevel)
    {
        Inventory.Instance.Gold += monsterlevel * 5 + Random.Range(-3, 4);
    }
}