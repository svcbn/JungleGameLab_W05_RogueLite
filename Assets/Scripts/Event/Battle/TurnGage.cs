using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnGage : MonoBehaviour
{

    public void ApplyGage(float cur_Value, float max_Value)
    {
        GetComponent<Slider>().value = cur_Value / max_Value;
    }
}
