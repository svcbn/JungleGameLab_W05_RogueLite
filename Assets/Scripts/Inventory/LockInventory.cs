using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockInventory : MonoBehaviour
{
    public int lockLevel;
    void FixedUpdate()
    {
        if(GameManager.Instance.player.Status.Level >= lockLevel)
        {
            GetComponentInParent<InventorySlot>().isLock = false;
            Destroy(this.gameObject);
        }
    }
}
