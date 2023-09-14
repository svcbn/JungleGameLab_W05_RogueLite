using Unity.VisualScripting;
using UnityEngine;

public class Status
{
    public int Level { get; set; } = 1;
    
    private bool player;
    private float maxHp;
    private float currentHp;
    private float str;
    private float dex;
    private float def;
    private float critChance;
    private float critDmg;
    private float armorPen;
    private float exp = 0;
    public bool monsterActive = true;
    public bool monsterFighting = false;

    private bool _buff;

    // 성장형 스탯
    private float statusMaxHpUp = 101f;
    private float statusPowerUp = 4.5f;
    private float statusDefUp = 4f;
    private float statusDexUp = 5f;

    // 레벨업 포인트 스탯
    private float statusPointPowerUp = 4f;
    private float statusPointDefUp = 4.5f;
    private float statusPointDexUp = 6f;
    private Vector3 statusPointAll = Vector3.zero;


    public bool Buff
    {
        get => _buff;
        set
        {
            _buff = value;
            if (player) UIManager.Instance.UpdatePlayerStatusInfo();
        }
    }
    public float MaxHp
    {
        get { return maxHp; }
        set
        {
            maxHp = value;
            // Ensure CurrentHp doesn't exceed MaxHp
            currentHp = Mathf.Min(currentHp, maxHp);
            if (player) UIManager.Instance.UpdatePlayerStatusInfo();
        }
    }

    public float CurrentHp
    {
        get { return currentHp; }
        set
        {   if(player){
                if (value < currentHp)
                {
                    UIManager.Instance.BloodEffect();
                }
            }
            else if (!player && value < currentHp)
            {
                UIManager.Instance.BloodEffect(false);
                monsterFighting = true;
                monsterActive = true;
                
            }
            // Ensure CurrentHp is between 0 and MaxHp
            currentHp = Mathf.Clamp(value, 0, maxHp);
            if (player)
            {
                UIManager.Instance.UpdatePlayerStatusInfo();
                if (currentHp == 0)
                {
                    UIManager.Instance.ActiveGameOverObj();
                }
            }
        }
    }

    public float Str
    {
        get
        {
            return str + (Buff ? 2 + DataManager.Instance.PrincessPowerSkillValue : 0);
        }
        set
        {
            str = value;
            if (player) UIManager.Instance.UpdatePlayerStatusInfo();
        }
    }

    public float Def
    {
        get { return def; }
        set 
        { 
            def = value;
            if (player) UIManager.Instance.UpdatePlayerStatusInfo();
        }
    }

    public float Dex
    {
        get => dex;
        set
        {
            dex = value;
            if (player) UIManager.Instance.UpdatePlayerStatusInfo();
        }
    }

    public float ArmorPen
    {
        get => armorPen;
        set
        {
            armorPen = value;
            if (player) UIManager.Instance.UpdatePlayerStatusInfo();
        }
    }

    public float Crit
    {
        get => critChance;
        set
        {
            critChance = value;
            if (player) UIManager.Instance.UpdatePlayerStatusInfo();
        }
    }

    public float CritDmg
    {
        get => critDmg;
        set
        {
            critDmg = value;
            if (player) UIManager.Instance.UpdatePlayerStatusInfo();
        }
    }



    public float Exp
    {
        get
        {
            return exp;
        }
        set
        {
            exp = Mathf.Max(value, 0);
            if (player) UIManager.Instance.UpdatePlayerStatusInfo();
        }
    }

    public void GetStatusPowerUp() // 레벨업 포인트 스탯
    {
        statusPointAll.x += statusPointPowerUp;
        Str += statusPointPowerUp;
    }
    public void GetStatusDefUp() // 레벨업 포인트 스탯
    {
        statusPointAll.y += statusPointDefUp;
        Def += statusPointDefUp;
    }
    public void GetStatusDexUp() // 레벨업 포인트 스탯
    {
        statusPointAll.z += statusPointDexUp;
        Dex += statusPointDexUp;
    }

    public void LevelUpStatusUp()
    {
        float growStat = /*(Level - 1) **/ (0.7025f + 0.0175f * (Level - 1));
        MaxHp += statusMaxHpUp * growStat;
        Str += statusPowerUp * growStat;
        Def += statusDefUp * growStat;
        Dex += statusDexUp * growStat;

        Inventory.Instance.UpdateStatus();
        CurrentHp = MaxHp;
    }
    public Status(float maxHp, float power, float defense, float dex, float exp, float armorPenetration = 0, float criticalProb = 0, float criticalDamage = 0, bool isPlayer = false)
    {
        player = isPlayer;

        MaxHp = maxHp;
        Exp = exp;
        currentHp = maxHp;
        Str = power;
        Def = defense;
        Dex = dex;
        ArmorPen = armorPenetration;
        Crit = criticalProb;
        CritDmg = criticalDamage;
    }
}
