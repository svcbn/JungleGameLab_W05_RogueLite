using System;
using UnityEngine;

public class MonsterData
{
    public MonsterData(float id, float level, string name, string type, float maxHp, float power, float def, float dex, string sprite)
    {
        this.id     = id;
        this.level  = level;
        this.exp    = level;
        this.name   = name;
        this.type   = type;
        this.maxHp  = maxHp;
        this.power  = power;
        this.def    = def;
        this.dex    = dex;
        this.sprite = sprite;
    }

    public float id { get; set; }
    public float level { get; set; }
    public float exp { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public float maxHp { get; set; }
    public float power { get; set; }
    public float def { get; set; }
    public float dex { get; set; }

    public string sprite { get; set; }

    public static MonsterData MonsterDataParseCsv(string line)
    {
        try
        {
            string[] data = line.Split(",");
            MonsterData monsterdata = new MonsterData(
                float.Parse(data[0]),
                float.Parse(data[1]),
                data[2],
                data[3],
                float.Parse(data[4]),
                float.Parse(data[5]),
                float.Parse(data[6]),
                float.Parse(data[7]),
                data[8]
                );
            return monsterdata;
        }
        catch(Exception e)
        {
            Debug.LogError($"Failed to parse CSV line: {line} Error: {e}");
        }
        return null;
    }
}