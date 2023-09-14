using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    private GameManager _gameManager;
    private MapManager _mapManager;
    private UIManager _uiManager;

    SpriteRenderer spriteRenderer;

    public string playerName;

    [Header("오브젝트 및 스킬 정보")]
    public GameObject playerUI;
    private GameObject playerSkillUI;
    public FieldPiece CurrentFieldPiece { get; set; }

    private int _selectedIdx;
    public int SelectedIdx
    {
        get => _selectedIdx;
        set
        {
            if (_selectedIdx != value)
            {
                _selectedIdx = value;
                _selectedIdx = Mathf.Min(_selectedIdx, 3);
                _uiManager.UpdateInfoText(_selectedIdx);
                _uiManager.FocusSkill(playerSkillUI, _selectedIdx);
            }
        }
    }

    [Header("스텟 및 코스트 정보")]
    public const float defaultHp = 580f;
    public const float defaultPower = 55f;
    public const float defaultDefense = 26f;
    public const float defaultDex = 10f;
    public const float defaultExp = 0f;
    public const float defaultArmorPenetration = 0f;
    public const float defaultCriticalProb = 0f;
    public const float defaultCriticalDamage = 1.5f;

    private int _cost;
    public int Cost
    {
        get => _cost;
        set
        {
            _cost = value;
            _uiManager.UpdateCostText(_cost);
        }
    }

    public bool IsTurnEnd;

    public Status Status { get; set; }

    [FormerlySerializedAs("selectedFloor")] [Header("other")] public int SelectedFloor;


    public void Start()
    {
        SelectedIdx = 0;
        _gameManager = GameObject.Find(nameof(GameManager)).GetComponent<GameManager>();
        _mapManager = GameObject.Find(nameof(MapManager)).GetComponent<MapManager>();
        _uiManager = GameObject.Find(nameof(UIManager)).GetComponent<UIManager>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        playerSkillUI = playerUI.transform.GetChild(0).gameObject;
        Status = new Status(defaultHp, defaultPower, defaultDefense, defaultDex, defaultExp, defaultArmorPenetration, defaultCriticalProb, defaultCriticalDamage, true);

        _uiManager.UpdatePlayerStatusInfo();
    }

    public void StartTurn(int cost)
    {
        SelectedIdx = 0;
        Cost = cost;
        IsTurnEnd = false;
        playerUI.SetActive(true);
        _uiManager.UpdateInfoText(_selectedIdx);
    }

    public void EndTurn()
    {
        IsTurnEnd = true;
        playerUI.SetActive(false);
    }

    public void Update()
    {
        if (!IsTurnEnd && !_gameManager.EventPrinting)
        {
            CheckScroll();
        }
    }

    #region Check Scroll
    void CheckScroll()
    {
        // Vector2 wheelInput2 = Input.mouseScrollDelta;
        // if (wheelInput2.y > 0) // 휠을 밀어 돌렸을 때의 처리 ↑
        // {
        //     if (_selectedIdx <= 0) return;
        //     SelectedIdx--;
        // }
        // else if (wheelInput2.y < 0) // 휠을 당겨 올렸을 때의 처리 ↓
        // {
        //     if (SelectedIdx > 0) return; // 임시 스킬은 3개만 
        //     SelectedIdx++;
        // }
    }
    public void SetSpriteRenderer(bool _bool){
        spriteRenderer.enabled = _bool;
    }
    #endregion
}