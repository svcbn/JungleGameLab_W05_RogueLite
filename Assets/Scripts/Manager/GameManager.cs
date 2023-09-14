using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public ResourceManager _resourceManager;
    public ShopManager _shopManager;
    private DataManager _dataManager;
    private UIManager _uiManager;
    public CameraManager CameraManager { get; private set; }
    public MapManager MapManager { get; private set; }

    public bool HasKey { get; set; }
    public bool GameEnd = false;
    public string whoseTurn;
    public int _turn;

    public int Turn
    {
        get => _turn;
        set
        {
            _turn = value;
            UIManager.Instance.UpdateTurnText(_turn);
        }
    }

    public int waveInterval;

    [Header("설정 값")]
    public Vector2 mapSize;
    public int emptyMapProportion;
    public int battleMapProportion;
    public int eventMapProportion;
    public int boxMapProportion;
    public int blockMapProportion;

    [Header("필드 관련")]
    public Transform FieldGenTransform;

    int sealstoneCount = 0;
    // private FieldPiece[,] knightFields;
    // private FieldPiece[,] princessFields;

    // [Header("맵 관련")]
    // public Transform MapGenTransform;
    // private MapPiece[,] KnightMaps;
    // private MapPiece[,] PrincessMaps;

    private int _displayFloor;
    public int DisplayFloor
    {
        get => _displayFloor;
        set
        {
            _displayFloor = value;
            if (_displayFloor != 0)
            {
                UIManager.Instance.UpdateCurrentDisplayFloor(_displayFloor);
                MapManager.ChangeFloor(_displayFloor);

                // 현재 표시된 층에 따라 Player 오브젝트 표시 
                //player.SetSpriteRenderer(_displayFloor == CurrentKnightFloor);

                ChangeBehavior(player.SelectedIdx);
                // // 층에 따라 업데이트
                // if (whoseTurn.Equals(nameof(princess)))
                // {
                //     ChangeBehavior(princess.SelectedIdx);
                // }
                // else
                // {
                //     ChangeBehavior(player.SelectedIdx);
                // }

            }
        }
    }

    /// <summary>
    /// 현재 용사가 존재하는 층의 정보
    /// </summary>
    public int CurrentKnightFloor;


    [Header("플레이어")]
    public Player player;
    public Sprite[] playerSprite;
    public GameObject playerInfo;
    public int StatusPoint
    {
        get => DataManager.Instance.StatusPoint;
        set
        {
            DataManager.Instance.StatusPoint = value;
            _uiManager.UpdatePlayerStatusInfo();
        }
    }

    public int PrincessMaxCost;
    public int KnightMaxCost;

    [Header("이벤트 관련")]
    public BattleEvent battleEvent;
    public BossBattleEvent bossbattleEvent;
    public FieldEvent fieldEvent;
    public ItemEvent itemEvent;
    public HealEvent healEvent;

    public bool EventPrinting { get; set; }

    [Header("웨이브 시스템")] private bool _dotDamageTime;

    [Header("디버그")]
    public bool debugIgnoreObject = false;
    public bool isHideMap = true;

    private bool DotDamageTime
    {
        get => _dotDamageTime;
        set
        {
            _dotDamageTime = value;
            _uiManager.BurningObj.SetActive(_dotDamageTime);
        }
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    private int turnsBeforeAscend;

    public void Start()
    {
        _resourceManager = GetComponentInChildren<ResourceManager>();
        _dataManager = GameObject.Find(nameof(DataManager)).GetComponent<DataManager>();
        MapManager = GetComponentInChildren<MapManager>();
        CameraManager = Camera.main.GetComponent<CameraManager>();
        _uiManager = GameObject.Find(nameof(UIManager)).GetComponent<UIManager>();

        Init();
    }

    /// <summary>
    /// 기본 설정 초기화
    /// </summary>
    private void Init()
    {
        MapManager.InitMap();

        //PrincessMaxCost = _dataManager.PrincessMaxCost;
        //KnightMaxCost = _dataManager.KnightMaxCost;
        player.SelectedFloor = CurrentKnightFloor = 1;
        //princess.SelectedFloor = 3;
        MapManager.ChangeFloor(1);
        InitGamePlayer();
        InitPlayerPosition();
        
        battleEvent.Init(player);

        // 게임 정보 초기화
        Turn = 1;
        StatusPoint = 0;

        // 시작
        StartCoroutine(nameof(PlayGame));

        // Map Test 위에줄 주석치고 밑에거 주석풀기
        // MapManager.BuildAllField(FieldType.Field);
    }
    // private void Update() {
    // }

    #region playerSetting
    Color background;
    Sprite sprite;
    void InitGamePlayer()
    {
        CameraManager.setTarget(player.gameObject);
        string playerName = PlayerPrefs.GetString("SelectCharacter");

        if (playerName == "Knight")
        {
            player.playerName = "용사";
            background = new Color(0.3537736f, 0.401642f, 1, 1);
            sprite = playerSprite[0];
            MapManager.TargetTile = MapManager.PrincessTile;
        }
        else if (playerName == "Princess")
        {
            player.playerName = "공주";
            background = new Color(1, 0.6650944f, 0.9062265f, 1);
            sprite = playerSprite[1];
            MapManager.TargetTile = MapManager.KnightTile;
        }
        _uiManager.PlayerSetting(background, sprite);
        _uiManager.UpdatePlayerStatusInfo();
    }
    #endregion
    void InitPlayerPosition()
    {
        player.transform.position = MapManager.GridToWorldPosition(new Vector2(0, 0));
        player.CurrentFieldPiece = MapManager.GetFieldPiece(player.SelectedFloor, new Vector2Int(0, 0));
        MapManager.LightTempKnightMove(player.CurrentFieldPiece.gridPosition);
        // MapManager.LightField(FieldType.Princess, new Vector2Int(19,19));

        MapManager.ChangeFloor(CurrentKnightFloor);
    }
    private void Update() {
        
        if(player.Status.CurrentHp > player.Status.MaxHp/3){
            playerInfo.SetActive(false);
        }
        else{
            playerInfo.SetActive(true);
        }
    }
    IEnumerator PlayGame()
    {
        while (true)
        {

            whoseTurn = nameof(player);
            MapManager.BuildAllField();
            yield return StartCoroutine(PlayPlayer(player));
            if (DotDamageTime)
            {
                // [TODO] 도트 데미지 액션 출력
                player.Status.CurrentHp -= GetDotDam();
            }

            // Camera.main.backgroundColor = 
            // whoseTurn = nameof(princess);
            // MapManager.BuildAllField();
            yield return StartCoroutine(PlayPlayer(player));

            if (GameEnd)
            {
                // 게임이 종료되면 실행한다.
                // 왜 종료되었는 지는 각 오브젝트에서 설정해준다.
                yield break;
            }

            Turn++;
            // if (Turn % waveInterval == 0)
            // {
            //     MapManager.DoWave(.1f);
            // }

            // 도트 데미지 여부 설정
            if (GetDotDam() > 0)
            {
                if (!DotDamageTime)
                {
                    Log($"<color=#D73502>{CurrentKnightFloor}층이 불타기 시작합니다.</color>");
                    DotDamageTime = true;
                }
            }
        }
    }

    public int GetDotDam()
    {
        var usedTurnThisFloor = Turn - turnsBeforeAscend;
        return -(_dataManager.WaveCountByFloor[CurrentKnightFloor - 1] - usedTurnThisFloor);   // 이전에 오르는데 사용됬던 턴수는 차감 
    }

    public bool CheckDotDam() => GetDotDam() > 0;

    IEnumerator PlayPlayer(Player player)
    {
        do
        {
            player.StartTurn(KnightMaxCost);

            DisplayFloor = CurrentKnightFloor;
            ChangeBehavior(player.SelectedIdx);


            CameraManager.Target = player.transform;
            yield return new WaitUntil(() => player.IsTurnEnd);
        } while (!player.IsTurnEnd);
    }

    /// <summary>
    /// true가 반환 된 경우, 스킬 사용이 유효한 상태로 코스트 차감
    /// false가 반환된 경우, 스킬 사용이 실패한 경우로 코스트를 차감하지 않음
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public bool ClickMap(FieldPiece field)
    {
        bool complete = true;
        // if (field._canSelect) // 필드에서 판단
        if (MapManager.canSelectList.Contains(field)) // 필드에서 판단
        {
            complete = player.SelectedIdx switch
            {
                0 => MoveKnight(field),
                //1 => Rest(),
                _ => false,
            };

            if (!complete)
            {
                //Log($"스킬이 실행되지 않음.");
            }
        }
        else
        {
            Log($"선택 가능한 영역이 아님");
            complete = false;
        }


        return complete;
    }

    #region Player Skill
    private bool MoveKnight(FieldPiece field)
    {
        bool result = true;
        bool isMove = true;

        if (field.MapType == MapType.Block)
        {
            Log("이동할 수 없는 지형입니다.");
        }
        else if (!debugIgnoreObject && ((CurrentKnightFloor == 9 && field.MapType == MapType.Boss) || field.MapType == MapType.Monster && field.gridPosition.x - player.CurrentFieldPiece.gridPosition.x != 0 && field.gridPosition.y - player.CurrentFieldPiece.gridPosition.y != 0))
        {
            Log("몬스터가 가로막고 있습니다.");
        }
        else
        {
            if(!debugIgnoreObject){
                switch (field.MapType)
                {
                    case MapType.Empty: break; // 이벤트가 없으면 종료
                    default:
                        if (field.MapType == MapType.Monster){
                            isMove = false;
                        }
                        if (field.MapType == MapType.Item || field.MapType == MapType.Door || field.MapType == MapType.Shop || field.MapType == MapType.Sealstone) SoundManager.Instance.Play(0);
                        else if (field.MapType == MapType.Monster || field.MapType == MapType.Event || field.MapType == MapType.Boss) SoundManager.Instance.Play(1);

                        ExecuteMapEvent(field);
                        
                        if (field.MapType != MapType.Monster && field.MapType != MapType.Shop && field.MapType != MapType.Door && field.MapType != MapType.StairUp && field.MapType != MapType.StairDown) {
                            MapManager.UpdateMapType(field, MapType.Empty);
                        }
                        
                        break;
                }
            }
            else if(field.MapType == MapType.Door || field.MapType == MapType.StairUp || field.MapType == MapType.StairDown){
                HasKey = true;
                ExecuteMapEvent(field);

            }

            if(isMove){
                // 이동
                player.transform.position = MapManager.GridToWorldPosition(new Vector2(field.gridPosition.x, field.gridPosition.y));
                player.CurrentFieldPiece = field;
                // 맵을 밝힘
                MapManager.LightTempKnightMove(field.gridPosition);
            }
            MapManager.MonsterBehaviour();
            ChangeBehavior(player.SelectedIdx);
        }

        // 이동 가능 영역 업데이트

        return result;
    }
    public void DefeatMonster(FieldPiece field){
        MapManager.MonsterList[CurrentKnightFloor - 1].Remove(field.gridPosition);
        MapManager.UpdateMapType(field, MapType.Empty);

    }
    private bool TurnOnMapPiece(FieldPiece field, bool isKnight = false, bool outputLog = true)
    {
        bool result = true;
        int cost = _dataManager.princessSkillCost[0];

        if (!isKnight && player.Cost >= cost)
        {
            if (MapManager.CheckCanUsedLightSkill(field))
            {
                // field.IsLight = true;
                //MapManager.LightField(FieldType.Princess, field.gridPosition);
                MapManager.LightFieldPrincess(field.gridPosition);
                MapManager.RefreshMap();
                ChangeBehavior(player.SelectedIdx);
                player.Cost -= _dataManager.princessSkillCost[player.SelectedIdx];
            }
            else
            {
                Log("스킬 범위 내 영역이 전부 밝혀져있습니다.");
                result = false;
            }
        }
        else
        {
            Log("코스트가 부족하여 실행 할 수 없습니다.");
            result = false;
        }

        return result;
    }

    private void Rest()
    {
        bool result = true;

        // if (player.Cost >= 1)
        // {
        //     player.Status.CurrentHp += player.Cost + _dataManager.KnightRestRecoveryHpAddValue;
        //     player.Cost = 0;
        // }
        // else
        // {
        //     Log("휴식에 필요한 코스트가 충분하지 않습니다.");
        //     result = false;
        // }

        //return result;
    }

    private void BuffKnight()
    {
        bool result = true;
        var knight_ = player;

        if (!knight_.Status.Buff)
        {
            int cost = _dataManager.princessSkillCost[player.SelectedIdx];

            if (player.Cost >= cost)
            {
                knight_.Status.Buff = true;
                player.Cost -= _dataManager.princessSkillCost[player.SelectedIdx];
            }
            else
            {
                Log("코스트가 부족하여 실행 할 수 없습니다.");
                result = false;
            }
        }
        else
        {
            Log("버프는 한 번만 사용할 수 있습니다.");
            result = false;
        }

        //return result;
    }

    #endregion

    public void ApplyArtifactStats(ArtifactChangeAmount value, bool isReset)
    {
        if (isReset)
        {
            player.Status.MaxHp -= value.maxHp;
            if(player.Status.CurrentHp > player.Status.MaxHp)
            {
                player.Status.CurrentHp = player.Status.MaxHp;
            }
            player.Status.Str -= value.str;
            player.Status.Dex -= value.dex;
            player.Status.Def -= value.def;
            player.Status.Crit -= value.crit;
            player.Status.CritDmg -= value.critDmg;
            player.Status.ArmorPen -= value.armorPen;
        }
        else
        {
            player.Status.MaxHp += value.maxHp;
            //player.Status.CurrentHp += value.maxHp;
            player.Status.Str += value.str;
            player.Status.Dex += value.dex;
            player.Status.Def += value.def;
            player.Status.Crit += value.crit;
            player.Status.CritDmg += value.critDmg;
            player.Status.ArmorPen += value.armorPen;
        }
        UIManager.Instance.UpdatePlayerStatusInfo();
    }

    #region Map-Related

    /// <summary>
    /// 행동이 변경될 때, 행동 사용 가능 지역을 표시
    /// </summary>
    /// <param name="index"></param>
    public void ChangeBehavior(int index)
    {
        List<FieldPiece> changePiece = new();
        FieldPiece[,] baseFields = MapManager.GetCurrentFloorField();
        FieldPiece curPiece = player.CurrentFieldPiece;

        MapManager.LightCellMode = false;
        AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x - 1, curPiece.gridPosition.y);
        AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x + 1, curPiece.gridPosition.y);
        AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y - 1);
        AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y + 1);
        AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x - 1, curPiece.gridPosition.y - 1);
        AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x + 1, curPiece.gridPosition.y + 1);
        AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x + 1, curPiece.gridPosition.y - 1);
        AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x - 1, curPiece.gridPosition.y + 1);
        // switch (index)
        // {
        //     case 0:
        //         // 1칸 간격의 4방향 전달
        //         break;
        //     case 1:
        //         // 용사가 서 있는 위치 전달 
        //         if (player.Cost > 0)
        //         {
        //             _uiManager.ActiveSomeThingBox($"휴식하시겠습니까?\n({player.Cost}만큼 체력이 회복됩니다.)", Rest);
        //         }
        //         else
        //         {
        //             Log("휴식을 취할 코스트가 충분하지 않습니다.");
        //         }
        //         //changePiece.Add(player.CurrentFieldPiece);
        //         break;
        //     case 2:
        //         // 2칸 간격으로 4방향 전당
        //         AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x - 2, curPiece.gridPosition.y);
        //         AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x + 2, curPiece.gridPosition.y);
        //         AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y - 2);
        //         AddPieceInList(changePiece, baseFields, curPiece.gridPosition.x, curPiece.gridPosition.y + 2);
        //         break;
        //     case 3:  // 8방향 전달 
        //         changePiece = GetFieldKnightSkill1(baseFields, curPiece, new[] { -1, 1, 0, 0 }, new[] { 0, 0, -1, 1 }).ToList();
        //         break;
        // }


        // MapManger에게 changePiece 전달
        MapManager.showCanSelectField(curPiece, changePiece);
    }
    #endregion

    #region Map Area


    /// <summary>
    /// way X, way Y 조합에 해당하는 방향내 Field Piece 리스트를 반환
    /// </summary>
    /// <param name="baseFields"></param>
    /// <param name="CurPiece"></param>
    /// <param name="wayX"></param>
    /// <param name="wayY"></param>
    /// <returns></returns>
    IEnumerable<FieldPiece> GetFieldKnightSkill1(FieldPiece[,] baseFields, FieldPiece CurPiece, int[] wayX, int[] wayY)
    {
        List<FieldPiece> list = new();

        for (int xIdx = 0; xIdx < wayX.Length; xIdx++)
        {
            for (int yIdx = 0; yIdx < wayY.Length; yIdx++)
            {
                (int x, int y) pivot = (CurPiece.gridPosition.x + wayX[xIdx], CurPiece.gridPosition.y + wayY[yIdx]);

                AddPieceInList(list, baseFields, pivot.x, pivot.y);
            }
        }

        return list;
    }

    void AddPieceInList(List<FieldPiece> list, FieldPiece[,] baseFields, int x, int y)
    {
        if (!(x < 0 || x >= baseFields.GetLength(0) || y < 0 || y >= baseFields.GetLength(1)))
        {
            var piece = baseFields[x, y];
            if (!list.Contains(piece)) list.Add(piece);
        }
    }

    /// <summary>
    /// 맵 이동 후 이벤트가 존재하면 수행 
    /// </summary>
    /// <param name="field"></param>
    private void ExecuteMapEvent(FieldPiece field)
    {
        EventPrinting = true;

        switch (field.MapType)
        {
            case MapType.Monster:
                // battleEvent.Init(player, field.monsterInfo);
                battleEvent.PlayerAttack(field);
                if (Inventory.Instance.IsSwordEquiped)
                {
                    float rand = UnityEngine.Random.Range(0f, 100f);
                    if(rand < 44.44f)
                    {
                        battleEvent.PlayerAttack(field);
                    }
                }
                // battleEvent.Execute();
                EventPrinting = false;
                break;
            case MapType.Event:
                fieldEvent.Execute(field.fieldEventInfo);
                break;
            case MapType.Item:
                itemEvent.Execute(field.itemInfo);
                break;
            case MapType.Shop:
                _shopManager.OpenShop();
                break;
            // case MapType.Heal:
            //     healEvent.Execute(
            //         player, _resourceManager.healEventSprite);
            //     break;
            case MapType.Dragon:
                if(sealstoneCount < 7)
                    bossbattleEvent.Init(player, _resourceManager.DragonNoSealStone);
                else
                    bossbattleEvent.Init(player, _resourceManager.Dragon);
                bossbattleEvent.Execute(true);
                break;
            case MapType.Boss:
                switch (CurrentKnightFloor)
                {
                    case 3:
                        bossbattleEvent.Init(player, _resourceManager.Ruggle);
                        bossbattleEvent.Execute(true);
                        break;
                    case 6:
                        bossbattleEvent.Init(player, _resourceManager.DeathKnight);
                        bossbattleEvent.Execute(true);
                        break;
                    case 9:
                        EventPrinting = false;
                        // bossbattleEvent.Init(player, _resourceManager.DeathKnight);
                        // bossbattleEvent.Execute(true);
                        break;
                }
                break;
            case MapType.StairUp:
                _uiManager.ActiveSomeThingBox("다음 층으로 올라가시겠습니까?", MoveNextFloor);
                break;
            case MapType.Sealstone:
                _uiManager.SetSkillInfoText("봉인석이 활성화 되었습니다.");
                ActiveSealstone();
                EventPrinting = false;
                break;
            case MapType.StairDown:
                _uiManager.ActiveSomeThingBox("이전 층으로 내려가시겠습니까?", MovePrevFloor);
                break;
            case MapType.Door:
                if (HasKey)
                {
                    _uiManager.ActiveSomeThingBox("다음 층으로 올라가시겠습니까?", OpenDoorAndMoveNextFloor);
                }
                else
                {
                    Log("열쇠가 없어 문을 열 수 없습니다.");
                }
                EventPrinting = false;
                break;
            case MapType.Princess:
                _uiManager.ActiveEndingScene();
                break;
        }
    }
    
    private void ActiveSealstone()
    {
        if(CurrentKnightFloor < 3)
            _uiManager.ActivateSealStone(CurrentKnightFloor);
        else if(CurrentKnightFloor < 6)
            _uiManager.ActivateSealStone(CurrentKnightFloor-1);
        else
            _uiManager.ActivateSealStone(CurrentKnightFloor-2);
        sealstoneCount++;
        if(sealstoneCount == 7){
            MapManager.dragonPiece.monsterInfo = _resourceManager.Dragon;
            _uiManager.SetSkillInfoText("봉인석이 모두 활성화 되었습니다.\n드래곤이 크게 약화됩니다.");
        }
    }
    #endregion

    #region 층 이동 관련
    /// <summary>
    /// 층 관련
    /// </summary>
    private void MovePrevFloor()
    {
        MoveFloor(--CurrentKnightFloor, MapManager.StairUpPositionList[CurrentKnightFloor - 1]);
    }
    private void MoveNextFloor()
    {
        MoveFloor(++CurrentKnightFloor, new Vector2Int(0, 0));
    }
    private void OpenDoorAndMoveNextFloor()
    {
        HasKey = false;
        MoveNextFloor();
    }
    private void MoveFloor(int floor, Vector2Int position) {

        Debug.Log("move Floor" + floor);
        // // 도트 데미지 관련 초기화
        // DotDamageTime = false;
        // turnsBeforeAscend = Turn;
        // _uiManager.UpdateTurnText(Turn);
        // 층 이동
        CurrentKnightFloor = floor;
        DisplayFloor = CurrentKnightFloor;
        player.SelectedFloor = CurrentKnightFloor;
        player.SelectedFloor = CurrentKnightFloor;

        var nextFloorInitPosField = MapManager.AllFieldMapData[CurrentKnightFloor - 1][position.x, position.y];

        player.transform.position = MapManager.GridToWorldPosition(nextFloorInitPosField.gridPosition);
        player.CurrentFieldPiece = nextFloorInitPosField;
        MapManager.LightTempKnightMove(nextFloorInitPosField.gridPosition);

        // player.Status.CurrentHp = player.Status.MaxHp; // 체력 맥스로
        if(CurrentKnightFloor == 9) isHideMap = false;
        else isHideMap = true;

        MapManager.RefreshMap();
        ChangeBehavior(player.SelectedIdx);

    }



    //   public void ShowSelectArtifact(int count = 3 )
   // {
    //    var canSelectArtifactList = _resourceManager.Artifacts.Where(x => !_dataManager.HasArtifactList.Contains(x)).ToList();
   //     var shuffleList = new List<Artifact>();

    //    for (int i = canSelectArtifactList.Count - 1; i >= 0; i--)
    //    {
    //        int index = Random.Range(0, i);
    //        shuffleList.Add(canSelectArtifactList[index]);
    //        canSelectArtifactList.RemoveAt(index);
    //    }
        
    //    var indexs = new List<int>();
    //    do
    //    {
    //        int index = Random.Range(0, shuffleList.Count);
    //        if (!indexs.Contains(index)) indexs.Add(index);

    //    } while (indexs.Count < count);
        
    //    // 화면 설정
    //    _uiManager.artifactSelectorObj.SetActive(true);
    //    for (int idx = 0; idx < count; idx++)
    //    {
    //        //_uiManager.UIArtifacts[idx].Init(shuffleList[idx]);
    //    }
    //}
    
    //public void GetArtifact(Artifact artifact)
    //{
    //    _dataManager.HasArtifactList.Add(artifact);
    //    _uiManager.AddHasArtifactUI(artifact);

    //    switch (artifact.Type)
    //    {
    //        case ArtifactType.AllStatUp :
    //            player.Status.MaxHp     += _dataManager.ARTI_AllStatUp_Value;
    //            player.Status.CurrentHp += _dataManager.ARTI_AllStatUp_Value;
    //            player.Status.Dex       += _dataManager.ARTI_AllStatUp_Value;
    //            player.Status.Power     += _dataManager.ARTI_AllStatUp_Value;
    //            player.Status.Defense   += _dataManager.ARTI_AllStatUp_Value;
    //            break;
    //        case ArtifactType.DexUp :
    //            player.Status.Dex += _dataManager.ARTI_DEXUP_Value;
    //            break;
    //        case ArtifactType.HpUp :
    //            player.Status.CurrentHp += _dataManager.ARTI_HPUP_Value;
    //            player.Status.MaxHp += _dataManager.ARTI_HPUP_Value;
    //            break;
    //        case ArtifactType.CostUp :
    //            KnightMaxCost++;
    //            PrincessMaxCost++;
    //            break;
    //        case ArtifactType.PrincessSkillUp :
    //            _dataManager.PrincessPowerSkillValue += _dataManager.ARTI_PrincessSkillUP_Value;
    //            break;
    //        case ArtifactType.KnightSkillUp :
    //            _dataManager.KnightRestRecoveryHpAddValue += _dataManager.ARTI_KnightSkillUP_Value;
    //            break;
    //        case ArtifactType.AddAttack :
    //            _dataManager.ARTI_AddAtack = true;
    //            break;
    //    }
    //}

    public void CompleteSelectArtifact()
    {
        _uiManager.artifactSelectorObj.SetActive(false);
    }

    public void ClearBoss()
    {
        HasKey = true;
        _uiManager.UpdatePlayerStatusInfo();
    }

    private void Ending()
    {
        EventPrinting = true;
        _uiManager.ActiveEndingScene();
    }
    
    private void Log(string text)
    {
        _uiManager.OutputInfo(text);
    }

    public void B_Restart()
    {
        Destroy(_uiManager.gameObject);
        Invoke(nameof(Restart), .5f);
    }

    void Restart()
    {
        SceneManager.LoadScene("Title");
    }
    #endregion

    #region Debug

    public void StatusMax(){
        
        player.Status.Level++;
        player.Status.LevelUpStatusUp();
        _uiManager.UpdatePlayerStatusInfo();
    }
    public void ToggleIgnoreObject(){
        debugIgnoreObject = !debugIgnoreObject;
    }
    public void ToggleHideMap(){
        isHideMap = !isHideMap;
    }
    public void RemainHP1(){
        player.Status.CurrentHp = 1;
    }

    #endregion

}