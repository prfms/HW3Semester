namespace MyThreadPool;

using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// A thread pool for executing tasks concurrently.
/// </summary>
public class MyThreadPool 
{
    private readonly int _maxThreads;
    private readonly ConcurrentQueue<Action> _taskQueue = new();
    private readonly Thread[] _threads;
    private readonly CancellationTokenSource _cts = new ();
    private readonly object _lockObject = new();
    
    /// <summary>
    /// Constructor to initialize thread pool with specified number of threads.
    /// </summary>
    /// <param name="maxThreads">Number of threads in thread pool.</param>
    public MyThreadPool(int maxThreads)
    {
        _maxThreads = maxThreads;
        _threads = new Thread[_maxThreads];
        InitializeThreads();
    }
    
    /// <summary>
    /// Shows number of working threads.
    /// </summary>
    /// <returns>Number of working threads.</returns>
    public int WorkingThreadsNumber()
        => _threads.Count(thread => thread.IsAlive);
    
    private void InitializeThreads()
    {
        for (var i = 0; i < _maxThreads; i++)
        {   
            _threads[i] = new Thread(ExecuteTasks);
            _threads[i].Start();
        }
    }
    
    private void ExecuteTasks()
    {
        while (!_cts.IsCancellationRequested)
        {
            if (_taskQueue.TryDequeue(out var taskAction))
            {
                taskAction();
            }
        }
    }
    
    /// <summary>
    /// Method to add new task to the thread pool.
    /// </summary>
    /// <param name="func">Function representing the task.</param>
    /// <typeparam name="TResult">Type of returning value of the function.</typeparam>
    /// <returns>New <see cref="MyTask{TResult}"/> to be submitted.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        if (_cts.IsCancellationRequested)
            throw new InvalidOperationException("Cannot submit tasks after shutdown requested.");

        lock (_lockObject)
        {
            var task = new MyTask<TResult>(func, this, _cts.Token);
            _taskQueue.Enqueue(() => task.Execute());
            Monitor.Pulse(_lockObject);
            
            return task;
        }
    }

    /// <summary>
    /// Stops the thread pool.
    /// New tasks cannot be submitted, but old ones must be calculated, despite shut down.
    /// </summary>
    public void Shutdown()
    {
        lock (_lockObject)
        {
            _cts.Cancel();
        }
        
        foreach (var thread in _threads)
        {
            thread.Join();
        }
    }
}