using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTest : MonoBehaviour
{
    public delegate void Testa(int num);

    private Testa TestDelegate;

    private Action<object> action;

    private void Update()
    {
        if (TestDelegate != null)
        {
            TestDelegate(123);
        }

        if (action != null)
        {
            action.Invoke("action");
        }

    }

    public void AddEvent(Testa testa)
    {
        TestDelegate += testa;
    }

    public void AddEvent1(Action<object> callback)
    {
    }
    
    public void RegisterListener(Action<object> callback)
    {
        
    }
}



