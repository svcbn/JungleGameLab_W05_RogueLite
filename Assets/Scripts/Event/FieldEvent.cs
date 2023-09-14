using System.Collections.Generic;
using UnityEngine;

public class FieldEvent : MonoBehaviour
{
    private GameManager _gameManager;
    private UIImgText _uiImgTxt;
    
    /// <summary>
    /// 텍스트
    /// </summary>
    public string Text { get; set; }
    public Sprite Sprite { get; set; }
    
    #region 선택지
    /// <summary>
    /// 선택지 (선택지과 인덱스 동일할 것)
    /// </summary>
    public string Question { get; set; }
    
    /// <summary>
    /// 확률 (선택지과 인덱스 동일할 것)
    /// </summary>
    public string Percent { get; set; }
    
    /// <summary>
    /// 응답 (선택지과 인덱스 동일할 것)
    /// </summary>
    public string Answer { get; set; }
    #endregion
    
    public void Execute(FieldEventInfo info)
    {
        gameObject.SetActive(true);
        _uiImgTxt ??= GetComponent<UIImgText>();
        _gameManager ??= GameObject.Find(nameof(GameManager)).GetComponent<GameManager>(); 
        

        switch(info.Type){
            case EventType.HP:
                _gameManager.player.Status.CurrentHp += _gameManager.player.Status.MaxHp / info.EffectAmount;
            break;
            case EventType.Power:
                _gameManager.player.Status.Str += info.EffectAmount;

            break;
            case EventType.Defense:
                _gameManager.player.Status.Def += info.EffectAmount;

            break;
            case EventType.Dex:
                _gameManager.player.Status.Dex += info.EffectAmount;

            break;
            case EventType.Exp:
                _gameManager.player.Status.Exp += info.EffectAmount;

            break;



        }

        _uiImgTxt.Init(info.Sprite, End, info.GetText);
    }


    void End()
    {
        gameObject.SetActive(false);
        _gameManager.EventPrinting = false;
    }
}
