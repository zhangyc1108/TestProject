using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class MainTest : MonoBehaviour
{
    public void Awake()
    {
        Debug.Log("主线程id: " + Thread.CurrentThread.ManagedThreadId);

        Thread thread = new Thread(() =>
        {
            Debug.Log("thread 线程id: " + Thread.CurrentThread.ManagedThreadId);
        });
        thread.Start();

        Task task = new Task(() =>
        {
            Debug.Log("task 线程id: " + Thread.CurrentThread.ManagedThreadId);
        });
        task.Start();

        asyncFunc();
    }

    public async void asyncFunc()
    {
        await Task.Delay(3000);

        Debug.Log("await 之后的代码段 线程id: " + Thread.CurrentThread.ManagedThreadId);
    }
}
