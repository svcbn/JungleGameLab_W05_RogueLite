using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

public enum FieldType
{
    Field,
    Princess, 
    Knight,
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    public GameManager gameManager;
    public UIManager _UIManager;
    public Camera fieldCamera;

    [Header("Generator")]
    [HideInInspector] public Dictionary<int, Vector2Int> StairUpPositionList;
    List<Tilemap> fieldTileList;
    float mapEventRatio = 0.2f;
    float mapItemboxRatio = 0.1f;
    List<Vector2Int> _fieldSizeList;

    [Header("Field")]
    public GameObject ObjectField;
    public Tilemap FieldTileMap;
    public Tilemap UITileMap;
    public Tilemap FloorTileMap;
    public Tilemap WallTileMap;
    public TileBase IsLightTile;
    public TileBase ItemTile;
    public TileBase EventTile;
    public TileBase FightMonsterTile;
    public TileBase MonsterTile;
    public TileBase HideTile;
    public TileBase HealTile;
    public TileBase ChestTile;
    public TileBase ShopTile;
    public TileBase TargetTile;
    public TileBase PrincessTile;
    public TileBase KnightTile;
    public TileBase CanSelectTile;
    public TileBase RedCanSelectTile;
    public TileBase BlockTile;
    public TileBase EmptyTile;
    public TileBase DoorTile;
    public TileBase StairUpTile;
    public TileBase StairDownTile;
    public TileBase SealstoneTile;
    public TileBase BossTile;
    public TileBase DragonTile;

    public Dictionary<int, FieldPiece[,]> AllFieldMapData = new Dictionary<int, FieldPiece[,]>();
    int floorCount = 0;
    int currentFloor = 0;
    public bool isPause = false;
    Vector2Int currentHoverGrid;

    public Vector2Int targetPlayerPosition;

    GameObject selectCusorObj;
    public List<FieldPiece> canSelectList = new List<FieldPiece>();
    public List<FieldPiece> PlayerTempLight = new List<FieldPiece>(9);
    public List<FieldPiece> PlayerPrevLight = new List<FieldPiece>();
    public List<FieldPiece> DoorTempLight = new List<FieldPiece>();
    public Dictionary<int, List<Vector2Int>> MonsterList = new Dictionary<int, List<Vector2Int>>();
    float cellSize = 1.28f;
    
    int knightSeeRange = 3;

    public FieldPiece dragonPiece;


    public Vector3[] fieldFloorOffset;
    
    public bool LightCellMode { get; set; }

    private void Awake() {
        selectCusorObj = Instantiate(Resources.Load<GameObject>("SelectCursorObject"));
        
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

    }

    public void InitMap(){

        mapEventRatio = DataManager.Instance.mapEventRatio;
        mapItemboxRatio = DataManager.Instance.mapItemboxRatio;
        fieldTileList = new List<Tilemap>(DataManager.Instance.fieldTileList);
        _fieldSizeList = DataManager.Instance.fieldSizeList;
        floorCount = _fieldSizeList.Count;
        fieldFloorOffset = new Vector3[floorCount];
        StairUpPositionList = new Dictionary<int, Vector2Int>();
        // _fieldSizeList = new List<Vector2Int>();
        // foreach (var item in DataManager.Instance.monsterFloorInfo) // csv파일에서 불러온 정보 Init
        // {
        //     _fieldSizeList.Add(Vector2Int.one * item.mapSize);
        // }
        for(int i = 0; i < floorCount; ++i){
            currentFloor = i;
            if (!AllFieldMapData.ContainsKey(i))
                AllFieldMapData.Add(i, CreateMap(i, _fieldSizeList[i]));
            fieldFloorOffset[i] = new Vector3((20 -_fieldSizeList[i].x)/2, (20 -_fieldSizeList[i].x)/2, 0);
        }        
        currentFloor = 0;
        
    }
    

    public FieldPiece GetFieldPiece(int floor, Vector2Int position){
            return AllFieldMapData[floor-1][position.x, position.y];
    }
    public FieldPiece GetFieldPiece(Vector2Int position){
            return AllFieldMapData[currentFloor][position.x, position.y];
    }
    public FieldPiece[,] CreateMap(int floor, Vector2Int fieldSize){

        // Init
        FieldPiece[,] MapData = new FieldPiece[fieldSize.x,fieldSize.y];
        Tilemap currentTileMap = fieldTileList[floor];
        List<Vector2Int> monList = new List<Vector2Int>();
        MonsterList.Add(currentFloor, monList);
        currentFloor = floor;
        bool isSetTile = false;
        for (int i = 0; i < fieldSize.x; i++)
        {
            for (int j = 0; j < fieldSize.y; j++)
            {
                MapData[i, j] = new FieldPiece();
                if((floor == 0) && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == StairUpTile){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.StairUp);
                    StairUpPositionList.Add(floor,MapData[i, j].gridPosition);
                }
                else if(floor == 2 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == BossTile){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Boss);
                    MapData[i, j].monsterInfo = gameManager._resourceManager.Ruggle;
                }
                else if(floor == 2 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == DoorTile){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Door);
                    StairUpPositionList.Add(floor,MapData[i, j].gridPosition);
                }
                else if(floor == 2 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == ShopTile ){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Shop);
                }
                else if(floor == 5 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == MonsterTile){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Monster);
                    MapData[i,j].monsterInfo = gameManager._resourceManager.GetRandomMonster(10);
                    MapData[i,j].monsterInfo.Status.monsterActive = false;
                    monList.Add( MapData[i,j].gridPosition);
                }   
                else if(floor == 5 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == BossTile){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Boss);
                    MapData[i, j].monsterInfo = gameManager._resourceManager.DeathKnight;
                }
                else if(floor == 5 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == DoorTile){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Door);
                    StairUpPositionList.Add(floor,MapData[i, j].gridPosition);
                }
                else if(floor == 8 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == DragonTile ){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Dragon);
                    MapData[i, j].monsterInfo = gameManager._resourceManager.DragonNoSealStone;
                    dragonPiece = MapData[i, j];
                }
                else if(floor == 5 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == ShopTile ){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Shop);
                }
                else if(floor == 8 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == HealTile ){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Empty);
                }
                else if(floor == 8 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == TargetTile ){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Princess);
                    targetPlayerPosition = MapData[i, j].gridPosition;
                }
                else if(floor == 8 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == BossTile){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Boss);
                    MapData[i, j].monsterInfo = gameManager._resourceManager.DeathKnight;
                }
                else if(floor == 8 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == ChestTile ){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Chest);
                }
                else if(floor == 8 && currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) == ShopTile ){
                    MapData[i, j].Init(floor, new Vector2Int(i, j), MapType.Shop);
                }
                else
                    MapData[i, j].Init(floor, new Vector2Int(i, j), currentTileMap.GetTile(new Vector3Int(i+1, j+1, 0)) != null ? MapType.Block : MapType.Empty);
            }
        }

        //Stair
        if(floor == 0)
            MapData[0, 0].SetMapType(MapType.Player);
        else
            MapData[0, 0].SetMapType(MapType.StairDown);
        if(floor != 0 && floor%3 != 2){
            Vector2Int pos =  SelectRandomEmptyTilePosition(MapData);
            MapData[pos.x, pos.y].SetMapType(MapType.StairUp);
            StairUpPositionList.Add(floor,MapData[pos.x, pos.y].gridPosition);
        }

        // sealstone
        if(floor %3 != 2 || floor == 8){
            Vector2Int pos =  SelectRandomEmptyTilePosition(MapData);
            MapData[pos.x, pos.y].SetMapType(MapType.Sealstone);
        }

        // create monster
        if(floor %3 != 2){
            int floorMonsterNum = 0;
            List<Vector3Int> floorRemainMonsterNum = new List<Vector3Int>();
            foreach (Vector3Int item in DataManager.Instance.monsterFloorInfo[floor])
            {
                floorMonsterNum += item.z;
                floorRemainMonsterNum.Add(item);
            }
            /*foreach (Vector3Int monsterNum in DataManager.Instance.monsterNumberPerFloor)
            {
                if (monsterNum.x - 1 == floor)
                {
                    floorMonsterNum += monsterNum.z;
                    floorRemainMonsterNum.Add(monsterNum);
                }
            }*/
            float monsterRatio = (float)floorMonsterNum / (float)(_fieldSizeList[floor].x*_fieldSizeList[floor].y);
            while(floorMonsterNum != 0){
                for (int i = 0; i < _fieldSizeList[currentFloor].x; i++)
                {
                    for (int j = 0; j < _fieldSizeList[currentFloor].y; j++)
                    {
                        if((i == 0 && j == 0) || (i == _fieldSizeList[currentFloor].x -1 && j == _fieldSizeList[currentFloor].y -1)){
                            continue;
                        }
                        if(MapData[i, j].MapType == MapType.Empty){
                            float val = UnityEngine.Random.value;
                            if(val < monsterRatio){
                                monList.Add( MapData[i,j].gridPosition);
                                MapData[i,j].SetMapType(MapType.Monster);
                                int selectMonsterLevelIdx = UnityEngine.Random.Range(0, floorRemainMonsterNum.Count);
                                while(floorRemainMonsterNum[selectMonsterLevelIdx].z == 0 && floorMonsterNum != 0){
                                    selectMonsterLevelIdx = (selectMonsterLevelIdx + 1) % floorRemainMonsterNum.Count;
                                }
                                MapData[i,j].monsterInfo = gameManager._resourceManager.GetRandomMonster(floorRemainMonsterNum[selectMonsterLevelIdx].y);
                                floorRemainMonsterNum[selectMonsterLevelIdx] -= new Vector3Int(0,0,1);
                                floorMonsterNum -= 1;
                                if(floorMonsterNum == 0) break;
                            }
                        }
                    }
                    if(floorMonsterNum == 0) break;
                }
            }


            GenerateFieldObjects(MapData, mapEventRatio, MapType.Event);
        }
        GenerateFieldObjects(MapData, mapItemboxRatio, MapType.Item);

        if(floor == 0)
            MapData[0, 0].SetMapType(MapType.Empty);

        return MapData;
    }
    
    public Vector2Int SelectRandomEmptyTilePosition(FieldPiece[,] MapData){
        int count = 0;
         while(true){
            int i = UnityEngine.Random.Range(0,_fieldSizeList[currentFloor].x);
            int j = UnityEngine.Random.Range(0, _fieldSizeList[currentFloor].y);
            if(MapData[i, j].MapType == MapType.Empty){
                return new Vector2Int(i, j);
            }
            count++;
            if(count > 1000){
                Debug.Log("SelectRandomEmptyTilePosition Error");
                return new Vector2Int(-1, -1);
            }
        }
    }
    public int fightMonsterNum;
    public void MonsterBehaviour(){
        int monsterAgroRange = knightSeeRange - 1;
        Vector2Int knightPos = gameManager.player.CurrentFieldPiece.gridPosition;
        Vector2Int[] randomList = {new Vector2Int(0, 0), new Vector2Int(0, 1),new Vector2Int(0, -1),new Vector2Int(1, 0),new Vector2Int(-1, 0)};
        int fightNum = 0;
        for (int i = 0; i < MonsterList[currentFloor].Count; ++i)
        {
            Vector2Int monsterPos = MonsterList[currentFloor][i];
            Monster monster = AllFieldMapData[currentFloor][monsterPos.x,monsterPos.y].monsterInfo;
            if(monster.Status.monsterActive){
                // // if player select monster
                // if(piece.gridPosition.Equals(monster)) continue;

                // if fight mode
                if((Math.Abs(monsterPos.x-knightPos.x) == 1 &&  monsterPos.y-knightPos.y == 0)|| (monsterPos.x-knightPos.x == 0 && Math.Abs(monsterPos.y-knightPos.y) == 1)){
                    if(!gameManager.debugIgnoreObject && monster.Status.monsterFighting){
                        fightNum++;
                        isPause = true;
                        StartCoroutine(gameManager.battleEvent.MonsterAttack(monster, fightNum));
                        // pause(2.0f);
                    }
                    else{
                        monster.Status.monsterFighting = true;
                    }
                    continue;
                }

                // if following player
                if(Math.Abs(monsterPos.x-knightPos.x) <= monsterAgroRange && Math.Abs(monsterPos.y-knightPos.y) <= monsterAgroRange){
                    int x = monsterPos.x, y = monsterPos.y;
                    if(knightPos.x- monsterPos.x > 0) x  += 1;
                    else if(knightPos.x- monsterPos.x < 0) x  += -1;
                    if (isInGrid(new Vector2Int(x, y)) && AllFieldMapData[currentFloor][x, y].MapType == MapType.Empty){
                        UpdateMapType(AllFieldMapData[currentFloor][x, y], MapType.Monster);
                        AllFieldMapData[currentFloor][x, y].monsterInfo = AllFieldMapData[currentFloor][monsterPos.x, monsterPos.y].monsterInfo;
                        UpdateMapType(AllFieldMapData[currentFloor][monsterPos.x, monsterPos.y], MapType.Empty);
                        MonsterList[currentFloor][i] = AllFieldMapData[currentFloor][x, y].gridPosition;
                        continue;
                    }
                    x = monsterPos.x;
                    if(knightPos.y- monsterPos.y > 0) y  += 1;
                    else if(knightPos.y- monsterPos.y < 0) y += -1;
                    if (isInGrid(new Vector2Int(x, y)) && AllFieldMapData[currentFloor][x, y].MapType == MapType.Empty){
                        UpdateMapType(AllFieldMapData[currentFloor][x, y], MapType.Monster);
                        AllFieldMapData[currentFloor][x, y].monsterInfo = AllFieldMapData[currentFloor][monsterPos.x, monsterPos.y].monsterInfo;
                        UpdateMapType(AllFieldMapData[currentFloor][monsterPos.x, monsterPos.y], MapType.Empty);
                        MonsterList[currentFloor][i] = AllFieldMapData[currentFloor][x, y].gridPosition;
                        continue;
                    }
                    continue;
                }

                // random moving
                monster.Status.monsterFighting = false;
                int count = randomList.Count();
                int randomIdx = UnityEngine.Random.Range(0, count);
                for (int j = 0; j < count; j ++) {
                    int idx = (randomIdx + j) % count;
                    int checkX = monsterPos.x + randomList[idx].x;
                    int checkY = monsterPos.y + randomList[idx].y;
                    if (isInGrid(new Vector2Int(checkX, checkY)) && AllFieldMapData[currentFloor][checkX, checkY].MapType == MapType.Empty){
                        UpdateMapType(AllFieldMapData[currentFloor][checkX, checkY], MapType.Monster);
                        AllFieldMapData[currentFloor][checkX, checkY].monsterInfo = AllFieldMapData[currentFloor][monsterPos.x, monsterPos.y].monsterInfo;
                        UpdateMapType(AllFieldMapData[currentFloor][monsterPos.x, monsterPos.y], MapType.Empty);
                        MonsterList[currentFloor][i] = AllFieldMapData[currentFloor][checkX, checkY].gridPosition;
                        break;
                    }
                }
            }
        }
        fightMonsterNum = fightNum;
        RefreshMap();
    }
    void GenerateFieldObjects(FieldPiece[,] mapData, float generateRatio, MapType value)
    {
        for (int i = 0; i < _fieldSizeList[currentFloor].x; i++)
        {
            for (int j = 0; j < _fieldSizeList[currentFloor].y; j++)
            {
                if((i == 0 && j == 0) || (i == _fieldSizeList[currentFloor].x -1 && j == _fieldSizeList[currentFloor].y -1)){
                    continue;
                }
                if(mapData[i, j].MapType == MapType.Empty){
                    float val = UnityEngine.Random.value;
                    if(val < generateRatio){
                        UpdateMapType(mapData[i, j], value);
                    }
                }
            }
        }
    }

    private Vector2Int beforeVector;
    private void Update()
    {
        if (!isPause && !gameManager.EventPrinting)
        {
            Vector2 mousePosition = fieldCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int grid = WorldPositionToGrid(mousePosition, ObjectField.transform.position);
            if(isInGrid(grid)){
                if(Input.GetMouseButtonDown(0)){
                    gameManager.ClickMap(AllFieldMapData[currentFloor][(int)grid.x, (int)grid.y]);
                }
                if(!grid.Equals(currentHoverGrid)){
                    PlaceSelectCursor(mousePosition, ObjectField.transform.position);
                    FieldPiece fieldPiece = AllFieldMapData[currentFloor][grid.x, grid.y];
                    // Debug.Log(fieldPiece.MapType);
                    if(!gameManager.isHideMap || PlayerTempLight.Contains(fieldPiece)){
                        if(fieldPiece.MapType == MapType.Monster){
                            _UIManager.TileInfUI(MapType.Monster, fieldPiece.monsterInfo);
                        }
                        else if(fieldPiece.MapType == MapType.Boss){
                            _UIManager.TileInfUI(MapType.Boss, fieldPiece.monsterInfo);
                        }
                        else if(fieldPiece.MapType == MapType.Dragon){
                        Debug.Log("Dragon " + fieldPiece.monsterInfo.Name);
                            _UIManager.TileInfUI(MapType.Dragon, fieldPiece.monsterInfo);
                        }
                        else if(fieldPiece.MapType == MapType.Item){
                                _UIManager.TileInfUI(MapType.Item, null);
                        }
                        else if(fieldPiece.MapType == MapType.Event){
                            _UIManager.TileInfUI(MapType.Event, null);
                        }
                        else if(fieldPiece.MapType == MapType.Door){
                            _UIManager.TileInfUI(MapType.Door, null);
                        }
                        // else _UIManager.TileInfUI(MapType.Empty);
                        currentHoverGrid = grid;
                }
                }
            // }
                else{
                    currentHoverGrid = new Vector2Int(-100, -100);
                }
            }
        }
    }

    /// <summary>
    /// 공주의 밝히기 스킬 사용 가능 여부를 반환
    /// </summary>
    public bool CheckCanUsedLightSkill(FieldPiece piece)
    {
        bool result = false;
        Vector2Int vec = piece.gridPosition;
        
        result |= !piece.IsLight;
        if(isInGrid(new Vector2(vec.x + 1, vec.y))) result |= !AllFieldMapData[currentFloor][vec.x + 1, vec.y].IsLight;
        if(isInGrid(new Vector2(vec.x, vec.y + 1))) result |= !AllFieldMapData[currentFloor][vec.x, vec.y + 1].IsLight;
        if(isInGrid(new Vector2(vec.x + 1, vec.y + 1))) result |= !AllFieldMapData[currentFloor][vec.x + 1, vec.y + 1].IsLight;

        return result;
    }
    
    public void LightFieldPrincess(Vector2Int position){
        AllFieldMapData[currentFloor][position.x, position.y].IsLight = true;
        if(isInGrid(new Vector2Int(position.x, position.y+1)))AllFieldMapData[currentFloor][position.x, position.y+1].IsLight = true;
        if(isInGrid(new Vector2Int(position.x+1, position.y)))AllFieldMapData[currentFloor][position.x+1, position.y].IsLight = true;
        if(isInGrid(new Vector2Int(position.x+1, position.y+1)))AllFieldMapData[currentFloor][position.x+1, position.y+1].IsLight = true;
    }
    public void LightTempKnightMove(Vector2Int position){
        PlayerTempLight.Clear();
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                int checkX = position.x + x;
                int checkY = position.y + y;
                if (isInGrid(new Vector2Int(checkX, checkY))){
                    FieldPiece piece = AllFieldMapData[currentFloor][checkX, checkY];
                    if((x == 0 || y == 0) && piece.MapType != MapType.Block){
                        LightTempKnightMoveRecur(1, piece.gridPosition, position);
                    }
                    else{
                        PlayerTempLight.Add(piece);
                        piece.IsLight = true;
                    }
                }
            }
        }
        

        RefreshMap();
    }
    
    void LightTempKnightMoveRecur(int distance, Vector2Int cur, Vector2Int origin){
        if( isInGrid(cur) && distance < knightSeeRange*2 
            && Math.Abs(cur.x-origin.x) <= knightSeeRange && Math.Abs(cur.y-origin.y) <= knightSeeRange )
        {
            if(!PlayerTempLight.Contains(AllFieldMapData[currentFloor][cur.x, cur.y])){
                FieldPiece piece = AllFieldMapData[currentFloor][cur.x, cur.y];
                PlayerTempLight.Add(piece);
                piece.IsLight = true;
            }
            if(AllFieldMapData[currentFloor][cur.x, cur.y].MapType != MapType.Block){
                LightTempKnightMoveRecur(distance + 1, cur + Vector2Int.up, origin);
                LightTempKnightMoveRecur(distance + 1, cur + Vector2Int.down, origin);
                LightTempKnightMoveRecur(distance + 1, cur + Vector2Int.left, origin);
                LightTempKnightMoveRecur(distance + 1, cur + Vector2Int.right, origin);
            }
        }
    }
    int floor = 0;
    public void ChagneFloorAuto(){
        
        floor = (floor+1)%_fieldSizeList.Count;
        ChangeFloor(floor);

    }
    public void ChangeFloor(int floor){
        currentFloor = floor -1;
        ObjectField.transform.position = fieldFloorOffset[currentFloor] * cellSize;
        if(gameManager.CurrentKnightFloor-1 != currentFloor) 
            gameManager.player.SetSpriteRenderer(false);
        else 
            gameManager.player.SetSpriteRenderer(true);
        RefreshMap();
    }

    // private List<FieldPiece> _backup;
    public void showCanSelectField(FieldPiece currentPiece, List<FieldPiece> _canSelectFields)
    {
        // canSelectList.Clear();
        canSelectList = new List<FieldPiece>(_canSelectFields);
        
        UITileMap.ClearAllTiles();
        foreach (FieldPiece piece in _canSelectFields)
        {   
            if(isInGrid(piece.gridPosition)){
                if(piece.MapType == MapType.Monster && (piece.gridPosition.x - currentPiece.gridPosition.x == 0 || piece.gridPosition.y - currentPiece.gridPosition.y == 0)){
                    UITileMap.SetTile(new Vector3Int(piece.gridPosition.x + 1, piece.gridPosition.y+ 1, 0), RedCanSelectTile);
                }
                else if(piece.MapType != MapType.Block){
                    UITileMap.SetTile(new Vector3Int(piece.gridPosition.x + 1, piece.gridPosition.y+ 1, 0), CanSelectTile);
                }
                canSelectList.Add(AllFieldMapData[currentFloor][piece.gridPosition.x, piece.gridPosition.y]);
            }
        }
        
    }
    // public void pause(float delay){
    //     isPause = true;
    //     StartCoroutine(pauseRoutine(delay));
    // }
    // IEnumerator pauseRoutine(float delay){
    //     yield return new WaitForSeconds(delay);
    //     isPause = false;
    // }
    bool isInGrid(Vector2 gridPosition){
        if(gridPosition.x >= 0 && gridPosition.x < _fieldSizeList[currentFloor].x && gridPosition.y >= 0 && gridPosition.y < _fieldSizeList[currentFloor].y){
            return true;
        }
        return false;
    }
    void PlaceSelectCursor(Vector2 position, Vector2 offset){
        Vector2 grid = WorldPositionToGrid(position, offset);
        if(isInGrid(grid)){
            selectCusorObj.transform.position = GridToWorldPosition(grid, offset);
        }
        else{
            selectCusorObj.transform.position = new Vector2(100,100);
        }
    }

    public Vector2 GridToWorldPosition(Vector2 gridPosition, Vector2 offset){
        Vector2 position = new Vector2(gridPosition.x + 1, gridPosition.y + 1);
        return position * cellSize + new Vector2(cellSize / 2, cellSize / 2) + offset;
    }
    public Vector2 GridToWorldPosition(Vector2 gridPosition){
        Vector2 position = new Vector2(gridPosition.x + 1, gridPosition.y + 1);
        return position * cellSize + new Vector2(cellSize / 2 + ObjectField.transform.position.x, cellSize / 2 + ObjectField.transform.position.y);
    }

    public Vector2Int WorldPositionToGrid(Vector2 worldPosition, Vector2 offset){
        Vector2 tmp = worldPosition - offset;
        return  new Vector2Int((int)(tmp.x / cellSize) - 1,(int)(tmp.y / cellSize) - 1);   
    }

    public void BuildAllField(){
        ClearAllMaps();
        
        WallTileMap.size = new Vector3Int(_fieldSizeList[currentFloor].x+2, _fieldSizeList[currentFloor].y+2, 0);
        WallTileMap.BoxFill(new Vector3Int(0, 0, 0), BlockTile, 0, 0, _fieldSizeList[currentFloor].x+2, _fieldSizeList[currentFloor].y+2 );
        WallTileMap.BoxFill(new Vector3Int(1, 1, 0), EmptyTile, 1, 1, _fieldSizeList[currentFloor].x, _fieldSizeList[currentFloor].y );
        FloorTileMap.size = new Vector3Int(_fieldSizeList[currentFloor].x+1, _fieldSizeList[currentFloor].y+1, 0);
        FloorTileMap.BoxFill(new Vector3Int(1, 1, 0), EmptyTile, 1, 1, _fieldSizeList[currentFloor].x+1, _fieldSizeList[currentFloor].y+1 );

        var typeToFloorTile = new Dictionary<MapType, TileBase>() {
            {MapType.Block, BlockTile},
            {MapType.Empty, EmptyTile}, 
            {MapType.Door, DoorTile}, 
            {MapType.StairUp, StairUpTile}, 
            {MapType.StairDown, StairDownTile}, 
            {MapType.Sealstone, SealstoneTile}, 
        };
        var typeToObjectTile = new Dictionary<MapType, TileBase>() {
            {MapType.Block, BlockTile},
            {MapType.Empty, EmptyTile}, 
            {MapType.Door, DoorTile}, 
            {MapType.StairUp, StairUpTile}, 
            {MapType.StairDown, StairDownTile}, 
            {MapType.Player, EmptyTile}, 
            {MapType.Item, ItemTile}, 
            {MapType.Monster, MonsterTile}, 
            {MapType.Event, EventTile}, 
            {MapType.Sealstone, SealstoneTile}, 
            {MapType.Boss, BossTile}, 
            {MapType.Dragon, DragonTile},
            // {MapType.Heal, EmptyTile},
            {MapType.Princess, TargetTile},
            {MapType.Shop, ShopTile},
            {MapType.Chest, ChestTile}
        };

        NewBuildMap(FloorTileMap, typeToFloorTile);
        NewBuildMap(FieldTileMap, typeToObjectTile);
    }

    public void NewBuildMap(Tilemap map, Dictionary<MapType, TileBase> typeToTile)
    {
        for (int x = 0; x < _fieldSizeList[currentFloor].x; x++){
            for (int y = 0; y < _fieldSizeList[currentFloor].y; y++){
                var targetTile = AllFieldMapData[currentFloor][x, y];
                if (!typeToTile.ContainsKey(targetTile.MapType))
                    continue;

                var tile = typeToTile[targetTile.MapType];
                if(map == FieldTileMap){
                    if(gameManager.isHideMap && targetTile.IsLight && !PlayerTempLight.Contains(targetTile)){ 
                        tile = IsLightTile;    
                    }
                    else if(gameManager.isHideMap && !PlayerTempLight.Contains(targetTile) && !DoorTempLight.Contains(targetTile)){
                        tile = HideTile;
                    }
                    else if(targetTile.MapType == MapType.Monster && targetTile.monsterInfo.Status.monsterFighting){
                        tile = FightMonsterTile;
                    }
                    else if(targetTile.MapType == MapType.Empty || targetTile.MapType == MapType.Block){
                        continue;
                    }

                }

                map.SetTile(new Vector3Int(x + 1, y + 1, 0), tile);
            }
        }
    }
    
    public void ClearMapPiece(FieldPiece fieldPiece){
        fieldPiece.SetMapType(MapType.Empty);
        RefreshMap();
    }
    public void ClearAllMaps(){
        
        FieldTileMap.ClearAllTiles();
        UITileMap.ClearAllTiles();
        FloorTileMap.ClearAllTiles();
        WallTileMap.ClearAllTiles();
    }

    public void RefreshMap(){
        BuildAllField();
    }

    public void UpdateMapType(FieldPiece fieldPiece, MapType type){
        fieldPiece.SetMapType(type);
        
        if(type == MapType.Item){
            // fieldPiece.itemInfo = itemInfo;
            fieldPiece.itemInfo = gameManager._resourceManager.GetRandomItemEvent();
        }
        else if(type == MapType.Event){
            // fieldPiece.fieldEventInfo = eventInfo;
            fieldPiece.fieldEventInfo = gameManager._resourceManager.GetRandomFieldEvent();
        }
    }

    void printMap(FieldPiece[,] pieces){
        string arrayStr = "";
            for (int j = 0; j < _fieldSizeList[currentFloor].y; j++)
            {
                for (int i = 0; i < _fieldSizeList[currentFloor].x; i++)
                {
                    arrayStr += pieces[i,j].MapType + " ";
                }
                arrayStr += "\n";
            }
        Debug.Log(arrayStr);
    }
    public FieldPiece[,] GetFloorField(int floor){
        return AllFieldMapData[floor];
    }
    public FieldPiece[,] GetCurrentFloorField(){
        return AllFieldMapData[currentFloor];
    }
}


public class FieldPiece
{
    private MapType _mapType = MapType.Empty;
    public MapType MapType {
        get { return _mapType; }
        private set { _mapType = value; }
    }

    public void Init(int _currentFloor, Vector2Int _gridPosition, MapType type){
        currentFloor = _currentFloor;
        gridPosition = _gridPosition;
        _mapType = type;
    }
    public void SetMapType(MapType type){
        _mapType = type;
    }

    public bool IsLight = false;

    public Vector2Int gridPosition{private set; get;}
    public int currentFloor{private set; get;}

    public Monster monsterInfo;
    public FieldEventInfo fieldEventInfo;
    public ItemInfo itemInfo;

}