using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceManager : MonoBehaviour
{
    public List<MonsterData> monsterData;
    public Dictionary<int , List<MonsterData>> monsters; //레벨 별로 정리된 값
    public List<FieldEventInfo> FieldEvents { get; private set; }

    public List<ItemInfo> Items { get; private set; }

    //public List<Artifact> Artifacts { get; private set; }

    public Sprite healEventSprite;

    public void Start()
    {
        InitFieldEvent();
        InitMonster();
        InitItemEvent();
        InitArtifact();
        InitMonsterCSV();
        // LoadData();
    }

    // public void LoadData()
    // {
    //     string[] data;
    //     TextAsset field = Resources.Load("FieldEvent/events") as TextAsset;
    //     data = field.text.Split(new string[] { ",", "\n" }, StringSplitOptions.None);
    //
    //     FieldEvents = new();
    //     for (int i = 0, cnt = data.Length / 2; i < cnt ; i++)
    //     {
    //         string text = data[2 + (i * 2)];
    //         Sprite sprite = Resources.Load($"{data[3 * (i + 1) + 1]}").GetComponent<Sprite>();
    //     
    //         var fieldInfo = new FieldEventInfo(sprite, text); 
    //         FieldEvents.Add(fieldInfo);
    //     }
    //
    //     
    // }

    void InitFieldEvent()
    {
        FieldEvents = new();
        FieldEvents.Add(new(EventType.HP, 4, GetSrc("FieldEvent", "blessinglake"), "축복의 샘입니다.\n\n체력이 조금 회복됩니다."));
        FieldEvents.Add(new(EventType.HP, 4, GetSrc("FieldEvent", "blessinglake"), "축복의 샘입니다.\n\n체력이 조금 회복됩니다."));
        FieldEvents.Add(new(EventType.HP, 4, GetSrc("FieldEvent", "blessinglake"), "축복의 샘입니다.\n\n체력이 조금 회복됩니다."));
        // FieldEvents.Add(new(EventType.HP, -2, GetSrc("FieldEvent", "goblinevent"), "수풀을 헤치며 지나가고 있었습니다. 갑자기 주위에서 고블린 덮칩니다./힘겹게 급습을 막아냈으나, 너무나 지칩니다.\n\n체력이 2 줄어듭니다."));
        FieldEvents.Add(new(EventType.HP, -4, GetSrc("FieldEvent", "goblinevent"), "수풀을 헤치며 지나가고 있었습니다. 갑자기 주위에서 고블린 덮칩니다./힘겹게 급습을 막아냈으나, 너무나 지칩니다.\n\n체력이 일부 줄어듭니다."));
        FieldEvents.Add(new(EventType.HP, 2, GetSrc("FieldEvent", "fruitevent"), "수풀을 헤치며 지나가고 있었습니다. 기다렸다는 듯, 탐스러워 보이는 열매를 찾았습니다./아직 신은 나를 버리지 않았나 봅니다. \n\n체력이 절반 회복됩니다."));
        FieldEvents.Add(new(EventType.HP, 2, GetSrc("FieldEvent", "fruitevent"), "수풀을 헤치며 지나가고 있었습니다. 기다렸다는 듯, 탐스러워 보이는 열매를 찾았습니다./아직 신은 나를 버리지 않았나 봅니다. \n\n체력이 절반 회복됩니다."));
        FieldEvents.Add(new(EventType.Power, 1, GetSrc("FieldEvent", "boyevent"), "늑대들에게 둘러싸여 있는 한 소년을 발견하고, 검을 뽑고 달려가 늑대들을 물리쳤습니다./소년은 감사하다는 인사를 하며, 자기도 꼭 커서 용사가 될 것이라 다짐합니다. 흐뭇한 표정을 지으며 갈 길을 이어서 갑니다. \n파워가 1 올라갑니다."));
        FieldEvents.Add(new(EventType.Dex, 3, GetSrc("FieldEvent", "ghost"), "갑자기 등골이 오싹해 집니다. 뒤를 돌아보니 귀신이 저를 쳐다보고 있습니다./화들짝 놀라 전력질주 합니다. \n\n 민첩이 3 증가합니다."));
        // FieldEvents.Add(new(EventType.HP, -1, GetSrc("FieldEvent", "blood"), "서둘러 나아가다, 바닥에 고인 핏물에 미끄러집니다. \n\n체력이 1 줄어듭니다."));
        // FieldEvents.Add(new(EventType.HP, -1, GetSrc("FieldEvent", "blood"), "서둘러 나아가다, 바닥에 고인 핏물에 미끄러집니다. \n\n체력이 1 줄어듭니다."));
        FieldEvents.Add(new(EventType.HP, -4, GetSrc("FieldEvent", "blood"), "서둘러 나아가다, 바닥에 고인 핏물에 미끄러집니다. \n\n체력이 일부 줄어듭니다."));
        FieldEvents.Add(new(EventType.HP, -4, GetSrc("FieldEvent", "arrow"), "'딸깍'\n불길한 예감이 듭니다./사방에서 화살이 날라옵니다. 빠르게 피했지만, 모두 피할 순 없었습니다. \n\n체력이 일부 줄어듭니다."));
    }

    public void InitArtifact()
    {
        //var data = DataManager.Instance;
        //Artifacts = new List<Artifact>();
        //Artifacts.Add(new Artifact(ArtifactType.AllStatUp, "공주의 편지", $"전체 스텟이 {data.ARTI_AllStatUp_Value}만큼 증가합니다.", GetSrc("Artifact", "princessletter")));
        //Artifacts.Add(new Artifact(ArtifactType.AddAttack, "분신술 비급서", $"용사는 {data.ARTI_AddAtack_Interval}공수마다 한번 더 공격합니다.", GetSrc("Artifact", "ninja")));
        //Artifacts.Add(new Artifact(ArtifactType.DexUp, "날개 달린 신발", $"용사의 민첩이 {data.ARTI_DEXUP_Value}만큼 증가합니다.", GetSrc("Artifact", "flyingshoes")));
        //Artifacts.Add(new Artifact(ArtifactType.CostUp, "반짝이는 돌", $"기이한 돌의 효과로 용사와 공주는 {data.ARTI_COSTUP_Value}만큼 추가 행동력을 가집니다.", GetSrc("Artifact", "shinestone")));
        //Artifacts.Add(new Artifact(ArtifactType.PrincessSkillUp, "유니콘의 뿔", $"공주의 성력이 증가하여, 용사 강화 시 {data.ARTI_PrincessSkillUP_Value}만큼 추가 강화합니다.", GetSrc("Artifact", "unicorn")));
        //Artifacts.Add(new Artifact(ArtifactType.KnightSkillUp, "빠짝 마른 장작", $"휴식 스킬 사용 시 {data.ARTI_KnightSkillUP_Value}만큼 추가 휴식합니다.", GetSrc("Artifact", "tree")));
        //Artifacts.Add(new Artifact(ArtifactType.HpUp, "트롤의 피", $"체력이 {data.ARTI_HPUP_Value}만큼 증가합니다.", GetSrc("Artifact", "blood")));
    }



    public Monster Ruggle;
    public Monster DeathKnight;
    public Monster Dragon;
    public Monster DragonNoSealStone;


    void InitMonster()
    {
        Ruggle = new Monster(8,"러글", new(1000, 80, 60, 60, 30), GetSrc("Monster", "ruggle"));
        DeathKnight = new Monster(17,"데스나이트", new(1600, 315, 100, 105, 50), GetSrc("Monster", "deathknight"));
        Dragon = new Monster(20,"드래곤", new(2000, 350, 120, 130, 0), GetSrc("Monster", "dragon"));
        DragonNoSealStone = new Monster(20,"드래곤", new(9999, 999, 999, 999, 0), GetSrc("Monster", "dragon"));
    }

    void InitItemEvent()
    {
        Items = new();
        Items.Add(new(EventType.HP, 3, GetSrc("ItemEvent", "fruit"), "열매를 주웠다!\n\n체력이 절반 회복됩니다."));
    }

    Sprite GetSrc(string folder, string name)
    {
        return Resources.Load<Sprite>($"{folder}/{name}");
    }

    [Button]
    void InitMonsterCSV() // 몬스터 정보 읽어오기
    {
        monsterData = GetComponent<CSVReader>().ReadMonsterDataCsv("MonsterData");
        monsters = new Dictionary<int, List<MonsterData>>();
        foreach (var item in monsterData)
        {
            if(!monsters.ContainsKey((int)item.level))
            {
                List<MonsterData> md = new List<MonsterData>();
                monsters.Add((int)item.level, md);
            }
            monsters[(int)item.level].Add(item);
        }
    }

    public Monster GetRandomMonster(int monlevel)
    {
        if (!monsters.ContainsKey(monlevel))
        {
            monlevel = monsters.Count;
        }
        int index = Random.Range(0, monsters[monlevel].Count);
        return new Monster((int)monsters[monlevel][index].level, monsters[monlevel][index].name,
            new (monsters[monlevel][index].maxHp, monsters[monlevel][index].power, monsters[monlevel][index].def, monsters[monlevel][index].dex, monsters[monlevel][index].exp), GetSrc("Monster", monsters[monlevel][index].sprite));
    }

    public FieldEventInfo GetRandomFieldEvent()
    {
        int index = Random.Range(0, FieldEvents.Count);
        return FieldEvents[index];
    }

    public ItemInfo GetRandomItemEvent()
    {
        int index = Random.Range(0, Items.Count);
        return Items[index];
    }
}


public enum EventType
{
    HP,
    Power,
    Defense,
    Dex,
    Exp,

}