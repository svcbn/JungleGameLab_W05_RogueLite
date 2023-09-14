using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Item")]
[Serializable]
public class Item : ScriptableObject
{
    public int ID;
    
    public Sprite Sprite;
    public string Name;
    public int Rarity;
    public float MaxHp = 0;
    public float Str = 0;
    public float Dex = 0;
    public float Def = 0;
    public float Crit = 0;
    public float CritDmg = 0;
    public float ArmorPen = 0;

    public bool IsEquipped = false;
}
