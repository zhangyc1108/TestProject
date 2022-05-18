using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class MainTest : MonoBehaviour
{
    public void Awake()
    {
        Debug.Log("���߳�id: " + Thread.CurrentThread.ManagedThreadId);

        Thread thread = new Thread(() =>
        {
            Debug.Log("thread �߳�id: " + Thread.CurrentThread.ManagedThreadId);
        });
        thread.Start();

        Task task = new Task(() =>
        {
            Debug.Log("task �߳�id: " + Thread.CurrentThread.ManagedThreadId);
        });
        task.Start();

        asyncFunc();
    }

    public async void asyncFunc()
    {
        await Task.Delay(3000);

        Debug.Log("await ֮��Ĵ���� �߳�id: " + Thread.CurrentThread.ManagedThreadId);
    }
}
