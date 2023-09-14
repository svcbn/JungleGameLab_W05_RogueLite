using System;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossBattleEvent : MonoBehaviour
{
    private GameManager _gameManager;
    private UIManager _uiManager;
    private Player _player;
    private Monster _monster;
    private BattleSelection _battleSelection;

    public Image monsterImg;
    private bool _isLastBoss = false;
    private bool _isBattle = false;

    private int _attDefCount;

    [Header("CombatPanelUI")]
    private GameObject _combatPanel;
    public TextMeshProUGUI combatText;
    public TextMeshProUGUI monsterInf;
    private TurnGage _playerTurnGage;
    private TurnGage _monsterTurnGage;

    private int maxLines = 5;
    private int lineCount = 0;
    public GameObject scrollbarVertical;

    public GameObject combatPanelExitButton;
    public GameObject gameOverButton;

    [Header("Turn")]
    float turnGage;
    bool _isknightTurn = false;
    WaitUntil isPlayerAttack;
    WaitWhile isPlayerSelecting;

    public void Init(Player knight, Monster monster)
    {
        _combatPanel ??= gameObject;
        _gameManager ??= GameObject.Find(nameof(GameManager)).GetComponent<GameManager>();
        _uiManager ??= GameObject.Find(nameof(UIManager)).GetComponent<UIManager>();
        _battleSelection ??= transform.GetComponentInChildren<BattleSelection>();
        _playerTurnGage ??= transform.GetComponentsInChildren<TurnGage>()[0];
        _monsterTurnGage ??= transform.GetComponentsInChildren<TurnGage>()[1];

        _player = knight;
        _monster = monster;
        monsterImg.sprite = _monster.Sprite;
        _attDefCount = 0;
        ClearCombatText();

        isPlayerAttack = new WaitUntil(() => _battleSelection.isAttack);
        isPlayerSelecting = new WaitWhile(() => _isknightTurn);
        _battleSelection.gameObject.SetActive(false);
    }

    public void Execute(bool isLastBoss = false)
    {
        _isLastBoss = isLastBoss;

        combatText.text = string.Empty;
        _combatPanel.SetActive(true);
        combatPanelExitButton.SetActive(false);

        UpdateMonsterInfoText();

        _isBattle = true;
        turnGage = 1f;
        StartCoroutine(PlayerTurn());
        StartCoroutine(MonsterTurn());
    }

    bool Dodges(float unitDex)
    {
        float dex = 10f * Mathf.Log10(unitDex);

        float random = UnityEngine.Random.Range(0f, 100f);

        return random > dex;
    }

    IEnumerator PlayerTurn() //플레이어 턴 계산
    {
        while (_isBattle)
        {
            //yield return new WaitForSeconds(Mathf.Lerp(1, 0.5f, (_knight.Status.Dex - Player.defaultDex) / _knight.Status.Dex));
            //if (!_isBattle) yield break;
            float currentGage = 0f;

            while (turnGage > currentGage)
            {
                yield return new WaitForSeconds(0.1f);
                currentGage += 0.1f / (Mathf.Lerp(1, 0.5f, (_player.Status.Dex - Player.defaultDex) / _player.Status.Dex));
                if (!_isBattle) yield break;
                _playerTurnGage.ApplyGage(currentGage, turnGage);
            }
            _isknightTurn = true;
            _battleSelection.gameObject.SetActive(true);
            yield return isPlayerAttack;
            PlayerAttack();
            if (Inventory.Instance.IsSwordEquiped)
            {
                float rand = UnityEngine.Random.Range(0f, 100f);
                if (rand < 44.44f)
                {
                    PlayerAttack();
                }
            }

            if (_monster.Status.MaxHp <= 0) // 몬스터가 죽었을 때
            {
                _isBattle = false;
                CombatPlayerWinText(_monster.Name);

                yield return PerformLevelUp();
                SoundManager.Instance.Play(3);
                string name = _monster.DropMonsterItem(_monster);
                if(name != null)
                    DropItemText(name);
                //if (_isLastBoss) _gameManager.ShowSelectArtifact(); // 아티펙트 선택 화면 추가

                combatPanelExitButton.SetActive(true);
                combatPanelExitButton.GetComponent<Button>().onClick.AddListener(End);
                // 종료 로직
                yield break;
            }
        }
    }
    IEnumerator MonsterTurn() //몬스터 턴 계산
    {
        while (_isBattle)
        {
            float currentGage = 0f;

            while (turnGage > currentGage)
            {
                yield return isPlayerSelecting;
                yield return new WaitForSeconds(0.1f);
                currentGage += 0.1f / (Mathf.Lerp(1, 0.5f, (_player.Status.Dex - Player.defaultDex) / _player.Status.Dex) * _player.Status.Dex / _monster.Status.Dex);
                if (!_isBattle) yield break;
                _monsterTurnGage.ApplyGage(currentGage, turnGage);
            }
            MonsterAttack();
            if (_player.Status.CurrentHp <= 0) //플레이어가 죽었을 때
            {
                _isBattle = false;
                CombatMonsterWinText();
                MapManager.Instance.isPause = true;
                StartCoroutine(_uiManager.ActiveGameOverObj());
                yield return new WaitForSeconds(2f);
                _combatPanel.SetActive(false);
                yield break;
            }
        }
    }

    public void PlayerAttack() //플레이어가 공격
    {

        float ap = Mathf.Max(_player.Status.ArmorPen, 0f);
        float dmg = _player.Status.Str * (100 / (100 + Mathf.Clamp((_monster.Status.Def - ap), 0f, Mathf.Infinity)));
        float playerDmg = Mathf.Max(dmg, 1);
        playerDmg = CaculateCritical(playerDmg);

        _attDefCount++;
        if (Dodges(_monster.Status.Dex))
        {
            _monster.Status.MaxHp -= playerDmg;
            OutputCombatText2((int)playerDmg);
        }
        else
        {
            OutputCombatMissText($"<color=#FF0000>{_monster.Name}</color>", $"<color=#008000>{_gameManager.player.playerName}</color>");
        }
        if (DataManager.Instance.ARTI_AddAtack)
        {
            var value = 2 * DataManager.Instance.ARTI_AddAtack_Interval;

            if (_attDefCount >= value)
            {
                AppendBattleInfoText("\n<color=#682699>아티펙트의 효과로 한번 더 공격합니다.</color>");
                _attDefCount = 0;
            }
        }
        UpdateMonsterInfoText();
        _battleSelection.isAttack = false;
        _isknightTurn = false;
    }

    float CaculateCritical(float damage)
    {
        float rand = UnityEngine.Random.Range(0f, 100f);
        if (_player.Status.Crit >= rand)
        {
            return damage * _player.Status.CritDmg;
        }
        else
        {
            return damage;
        }
    }

    public void MonsterAttack() //몬스터가 공격
    {
        SoundManager.Instance.Play(1);
        float dmg = _monster.Status.Str * (100 / (100 + (_player.Status.Def - _monster.Status.ArmorPen)));
        float monsterDam = Mathf.Max(dmg, 1);
        if (Dodges(_player.Status.Dex))
        {
            _player.Status.CurrentHp -= monsterDam;
            if (Inventory.Instance.IsHelmetEquiped)
            {
                float rand = UnityEngine.Random.Range(0f, 100f);
                if (rand < 44.44f)
                {
                    _player.Status.CurrentHp += _player.Status.MaxHp * 0.04f;
                }
            }

            OutputCombatText($"<color=#FF0000>{_monster.Name}</color>", $"<color=#008000>{_gameManager.player.playerName}</color>", (int)monsterDam, (int)_player.Status.CurrentHp);
        }
        else
        {
            OutputCombatMissText($"<color=#008000>{_gameManager.player.playerName}</color>",$"<color=#FF0000>{_monster.Name}</color>");
        }
    }

    public void DropItemText(string item)
    {

        lineCount++;

        string currentText = combatText.text;
        string newCombatInfo = "유물" + item+"을 획득했습니다.";
        string updatedText = currentText + "\n" + newCombatInfo;

        combatText.text = updatedText;


        if (lineCount > maxLines)
        {
            ScrollCombatText();
        }
    }
    private void End()
    {
        _combatPanel.SetActive(false);
        
        // 공통 동작
        combatPanelExitButton.SetActive(true);
        if (_player.Status.Buff)
        {
            _player.Status.Buff = false;
        }
        _gameManager.EventPrinting = false;
        
        // 보스를 깬 경우, 클리어 정보 추가
        if (_isLastBoss)
        {
            _gameManager.ClearBoss();
        }
    }



    #region Text Related

    void OutputCombatText(string name1, string name2, int name1power, int name2currentHP) //적이 때렸을 때
    {
        if (name2currentHP < 0)
        {
            name2currentHP = 0;
        }

        lineCount++;

        string currentText = combatText.text;
        string newCombatInfo = name1 + "(이)가 " + name1power + " 의 피해를 입혔습니다. " + name2 + "의 남은 HP = " + name2currentHP;
        string updatedText = currentText + "\n" + newCombatInfo;

        combatText.text = updatedText;


        if (lineCount > maxLines)
        {
            ScrollCombatText();
        }

    }

    void OutputCombatText2(int power) //플레이어가 때렸을 때
    {

        lineCount++;

        string currentText = combatText.text;
        string newCombatInfo = $"<color=#008000>{_gameManager.player.playerName}</color>가 " + power + " 의 피해를 입혔습니다. " ;
        string updatedText = currentText + "\n" + newCombatInfo;

        combatText.text = updatedText;


        if (lineCount > maxLines)
        {
            ScrollCombatText();
        }

    }

    void OutputCombatMissText(string name1, string name2)
    {
        lineCount++;

        string currentText = combatText.text;
        string newCombatInfo = name1 + "(이)가 " + name2 + " 공격을 <color=#0019FA>회피</color>했습니다.";
        string updatedText = currentText + "\n" + newCombatInfo;

        combatText.text = updatedText;


        if (lineCount > maxLines)
        {
            ScrollCombatText();
        }

    }

    void TestCombat()
    {

        OutputCombatText("player", "monster", 5, 5);
    }


    void CombatPlayerWinText(string monsterName)
    {
        lineCount++;

        string currentText = combatText.text;
        string newCombatInfo = $"<color=#008000>{_gameManager.player.playerName}</color>가 " + monsterName + "(을)를 무찔렀습니다!";
        string updatedText = currentText + "\n" + newCombatInfo;

        combatText.text = updatedText;

        if (lineCount > maxLines)
        {
            ScrollCombatText();
        }

    }

    void CombatMonsterWinText()
    {
        lineCount++;

        string currentText = combatText.text;
        string newCombatInfo = $"<color=#008000>{_gameManager.player.playerName}</color>의 눈앞이 깜깜해집니다..";
        string updatedText = currentText + "\n" + newCombatInfo;

        combatText.text = updatedText;

        if (lineCount > maxLines)
        {
            ScrollCombatText();
        }
    }

    private void ScrollCombatText()
    {
        RectTransform contentRectTransform = combatText.transform.parent.GetComponent<RectTransform>();
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentRectTransform.sizeDelta.y + 50f);

        StartCoroutine(UpdateScroll());
    }

    IEnumerator UpdateScroll()
    {
        yield return new WaitForSeconds(.01f);
        scrollbarVertical.GetComponent<Scrollbar>().value = 0f;
    }

    void ClearCombatText()
    {
        combatText.text = "";
        lineCount = 0;
    }

    string UpdateMonsterInfoText()
    {
        return monsterInf.text = $"HP: {(int)_monster.Status.MaxHp}, 파워: {(int)_monster.Status.Str}";
    }

    void AppendBattleInfoText(string text)
    {
        lineCount++;
        combatText.text += text;
        ScrollCombatText();
    }

    IEnumerator PerformLevelUp()
    {
        _player.Status.Exp += _monster.Status.Exp;
        float expNeed = DataManager.Instance.ExpNeedForLevelUp;

        yield return new WaitForSeconds(0.5f);

        AppendBattleInfoText($"\n경험치를 {_monster.Status.Exp} 획득했습니다.");

        if (_player.Status.Exp >= expNeed)
        {
            _player.Status.Level++;
            _player.Status.Exp -= expNeed;
            // DataManager.Instance.ExpNeedForLevelUp = 180 + (_knight.Status.Level * 100); // 경험치 필요량 증가
            DataManager.Instance.ExpNeedForLevelUp = 2*(_player.Status.Level * _player.Status.Level); // 경험치 필요량 증가

            _player.Status.LevelUpStatusUp(); // 성장 스탯 적용

            yield return new WaitForSeconds(0.5f);
            AppendBattleInfoText($"\n{_gameManager.player.playerName}의 레벨이 {_player.Status.Level}로 올랐다!");
            _gameManager.StatusPoint++;
        }
    }
    #endregion
}