using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public class Test_2 : MonoBehaviour
{
    public EventTest test;

    public delegate void Del1ay(int time, Action action);

    public Del1ay a;
    
    private void Start()
    {
        // Task1();
        // t1();
        // t3();
        // if (test)
        // {
        //     test.AddEvent(Hello);
        //     test.AddEvent(Hello2);
        //     test.AddEvent1(keko);
        // }
        // Debug.Log(1);
        // int num = t3().GetAwaiter().GetResult();
        // Debug.Log(2);
        // Debug.Log(num);
        // Debug.Log(3);

    }

    private void Hello(int a)
    {
        Debug.Log($"hello 1 {a}");
    }
    
    private void Hello2(int a)
    {
        Debug.Log($"hello 2 {a}");
    }

    private void keko(object obj)
    {
        Debug.Log(obj);
    }

    private async void Task1()
    {
        
        int i = 10;
        while (i > 0)
        {
            await Task.WhenAll(t1(), t2(), t3());
            float a = 1;
            Debug.Log($"{i}");
            i--;
            await Task.Delay(1000);
        }
    }
    
    private async Task t1()
    {
        await Task.Delay(500);
        Debug.Log("Task1");
    }

    private async Task t2()
    {
        await Task.Delay(2000);
        Debug.Log("Task2");
    }
    private async Task<int> t3()
    {
        await Task.Delay(3000);
        Debug.Log("Task3");
        return 3000;
    }
    
}