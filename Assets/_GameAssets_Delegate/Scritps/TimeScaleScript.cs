using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleScript : MonoBehaviour
{
    [Range(0,1)]
    public float TimeScale;

    private void OnValidate()
    {
        Time.timeScale = TimeScale;
    }
}
