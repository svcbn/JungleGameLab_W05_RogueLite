using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CSVReader : MonoBehaviour
{
    List<MonsterData> monsterDataList = new List<MonsterData>();
    public List<MonsterData> ReadMonsterDataCsv(string csvFileName) // ���� ����
    {
        var textAsset = Resources.Load<TextAsset>("Files/" + csvFileName);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load CSV file: {csvFileName}");
        }
        string[] lines = textAsset.text.Split('\n');

        foreach (string line in lines.Skip(1))
        {
            //if (csvFileName.Equals("MonsterData", StringComparison.OrdinalIgnoreCase))
                monsterDataList.Add(MonsterData.MonsterDataParseCsv(line.Trim()));
        }
        return monsterDataList;
    }

    List<MonsterFloorInfo> monsterFloorInfos = new List<MonsterFloorInfo>(); // �� ����(���� ������ ��ü ��, ��ũ��)
    public List<MonsterFloorInfo> ReadMonsterFloorDataCsv(string csvFileName)
    {
        var textAsset = Resources.Load<TextAsset>("Files/" + csvFileName);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load CSV file: {csvFileName}");
        }
        string[] lines = textAsset.text.Split('\n');

        foreach (string line in lines.Skip(1))
        {
            //if (csvFileName.Equals("MonsterFloorInfo", StringComparison.OrdinalIgnoreCase))
                monsterFloorInfos.Add(MonsterFloorInfo.MonsterFloorDataParseCsv(line.Trim()));
        }
        return monsterFloorInfos;
    }
}
