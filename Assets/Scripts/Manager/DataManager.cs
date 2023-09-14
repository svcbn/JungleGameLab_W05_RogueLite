using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    //MapManager
    [Header("Map Manager")]
    public float mapEventRatio;
    public float mapItemboxRatio;
    public List<Vector2Int> fieldSizeList;
    public List<Tilemap> fieldTileList;
    public List<Vector3Int> monsterNumberPerFloor;
    // public List<MonsterFloorInfo> monsterFloorInfo;
    public Dictionary<int, List<Vector3Int>> monsterFloorInfo;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;            
        }
        else
        {
            Destroy(this);
        }
    }

    [Header("Knight Status")]
    [SerializeField]
    public int StatusPoint;
    public float ExpNeedForLevelUp = 2f;
    
    [Header("Player Skill")]
    
    public int PrincessMaxCost;
    public int KnightMaxCost;
    public int[] princessSkillCost;
    public int[] knightSkillCost;

    [Tooltip("용사 스킬에서 추가 회복량입니다.")]  public int KnightRestRecoveryHpAddValue;
    [Tooltip("공주 스킬에서 추가 파워 업될 수치입니다.")] public int PrincessPowerSkillValue;

    [Header("게임")]
    /// <summary>
    /// index 0 = 1층에서 도트 데미지를 받기 시작하는 턴 
    /// </summary>
    public List<int> WaveCountByFloor = new()
    { 10, 10, 10 };


    public int ARTI_AllStatUp_Value;
    public bool ARTI_AddAtack;
    public int ARTI_AddAtack_Interval;
    public int ARTI_DEXUP_Value;
    public int ARTI_COSTUP_Value;
    public int ARTI_PrincessSkillUP_Value;
    public int ARTI_KnightSkillUP_Value;
    public int ARTI_HPUP_Value;

    private void Start() // 층 별 몬스터 수 정보 Init
    {
        InitMonsterFloorInfo();
        /*for (int i = 1; i <= 9; i++)
        {
            for (int monsterLevel = )
        }
        monsterNumberPerFloor.Add(new Vector3Int(1, 1, 4));
        monsterNumberPerFloor.Add(new Vector3Int(1, 1, 4));*/
    }

    [Button]
    private void InitMonsterFloorInfo() // 층 별 몬스터 정보 csv파일 읽어오기
    {
        // monsterFloorInfo = FindObjectOfType<CSVReader>().ReadMonsterFloorDataCsv("MonsterFloorInfo");
        monsterFloorInfo = new Dictionary<int, List<Vector3Int>>();
        foreach(var item in monsterNumberPerFloor){
            if(!monsterFloorInfo.ContainsKey(item.x-1)){
                monsterFloorInfo[item.x-1] = new List<Vector3Int>();
            }
            monsterFloorInfo[item.x-1].Add(item);
        }
    }
}
    