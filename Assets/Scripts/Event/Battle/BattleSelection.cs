using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSelection : MonoBehaviour
{
    
    public bool isAttack = false;

    public void Attack()
    {
        isAttack = true;
        gameObject.SetActive(false);
    }

    public void ShowInventory()
    {
        //인벤토리 열기
    }
}
