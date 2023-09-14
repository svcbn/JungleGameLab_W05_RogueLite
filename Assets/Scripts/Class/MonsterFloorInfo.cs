using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterFloorInfo
{
    public int mapSize;
    public List<Vector3Int> monsterCount;

    public MonsterFloorInfo(int mapSize, List<Vector3Int> monsterCount)
    {
        this.mapSize = mapSize;
        this.monsterCount = monsterCount;
    }

    public static MonsterFloorInfo MonsterFloorDataParseCsv(string line)
    {
        var monsterCount = new List<Vector3Int>();
        try
        {
            string[] data = line.Split(",");
            for(int i = 2; i < data.Length; i+=2)
            {
                if (data[i] != "")
                {
                    monsterCount.Add(new Vector3Int(Convert.ToInt32(data[1]), Convert.ToInt32(data[i]), Convert.ToInt32(data[i + 1])));
                }
                else
                    break;
            }
            MonsterFloorInfo monsterFloorInfo = new MonsterFloorInfo(
                    Convert.ToInt32(data[0]),
                    monsterCount
                );
            return monsterFloorInfo;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse CSV line: {line} Error: {e}");
        }
        return null;
    }
}