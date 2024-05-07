namespace MyThreadPool;

using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// Represents a thread pool for executing tasks concurrently.
/// </summary>
public class MyThreadPool 
{
    private readonly int _maxThreads;
    private readonly ConcurrentQueue<Action> _taskQueue = new();
    private readonly List<Thread> _threads = [];
    private readonly CancellationTokenSource _cts = new ();

    public MyThreadPool(int maxThreads)
    {
        _maxThreads = maxThreads;
        InitializeThreads();
    }

    private void InitializeThreads()
    {
        for (var i = 0; i < _maxThreads; i++)
        {   
            var thread = new Thread(ExecuteTasks);
            _threads.Add(thread);
            thread.Start();
        }
    }

    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        var task = new MyTask<TResult>(func, this, _cts.Token);
        
        if (!_cts.IsCancellationRequested)
        {
            _taskQueue.Enqueue(() => task.Execute());
        }
        else
        {
            throw new InvalidOperationException("Cannot submit tasks after shutdown requested.");
        }

        return task;
    }

    public void Shutdown()
    {
        _cts.Cancel();
        foreach (var thread in _threads)
        {
            thread.Join();
        }
    }

    private void ExecuteTasks()
    {
        if (_taskQueue.TryDequeue(out var taskAction))
        {
            taskAction();
        }
    }
}