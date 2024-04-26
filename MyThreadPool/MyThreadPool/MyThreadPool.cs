namespace MyThreadPool;

using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// Represents a thread pool for executing tasks concurrently.
/// </summary>
public class MyThreadPool
{
    private readonly Thread[] _threads;
    private readonly ConcurrentQueue<Action> _taskQueue;
    private readonly CancellationTokenSource _token;
    private readonly object _lockObject;
    public bool IsShutdown = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
    /// </summary>
    /// <param name="threadNumber">The number of threads in the pool.</param>
    public MyThreadPool(int threadNumber)
    {
        if (threadNumber < 1)
        {
            throw new ArgumentException("Thread pool must have at least 1 thread.");
        }

        _threads = new Thread[threadNumber];
        _taskQueue = new ConcurrentQueue<Action>();
        _token = new CancellationTokenSource();
        _lockObject = new object();

        for (var i = 0; i < threadNumber; i++)
        {
            _threads[i] = new Thread(Work);
            _threads[i].IsBackground = true;
            _threads[i].Start();
        }
    }
    
    /// <summary>
    /// How many threads can work in thread pool.
    /// </summary>
    public int GetNumberOfAbleThreads()
        => _token.Token.IsCancellationRequested 
            ? 0 
            : _threads.Length;
    
    /// <summary>
    /// Gets number of working threads.
    /// </summary>
    public int WorkingThreadsNumber
    {
        get => _threads.Count(thread => thread.IsAlive);
    }

    /// <summary>
    /// Submits a new task to the thread pool.
    /// </summary>
    /// <typeparam name="TResult">The type of task result.</typeparam>
    /// <param name="function">The function representing the task.</param>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> function)
    {
        if (_token.Token.IsCancellationRequested)
        {
            throw new InvalidOperationException("Thread pool was shut down.");
        }

        lock (_lockObject)
        {
            var myTask = new MyTask<TResult>(function, this);
            _taskQueue.Enqueue(myTask.Execute);
            Monitor.Pulse(_lockObject);

            return myTask;
        }
    }

    /// <summary>
    /// Shuts down the thread pool and waits for all threads to complete execution.
    /// </summary>
    public void Shutdown()
    {
        lock (_lockObject)
        {
            _token.Cancel();
            Monitor.PulseAll(_lockObject);
            IsShutdown = true;
        }

        foreach (var thread in _threads)
        {
            thread.Join();
        }
    }

    private void Work()
    {
        while (!_token.Token.IsCancellationRequested)
        {
            Action? task = null;
            lock (_lockObject)
            {
                while (!_taskQueue.TryDequeue(out task) && !_token.Token.IsCancellationRequested)
                {
                    Monitor.Wait(_lockObject);
                }
            }
            task?.Invoke();
        }
    }

    private void SubmitContinuation(Action action)
    {
        lock (_lockObject)
        {
            if (_token.Token.IsCancellationRequested)
            {
                return;
            }

            _taskQueue.Enqueue(action);
            Monitor.Pulse(_lockObject);
        }
    }

    private class MyTask<TResult> : IMyTask<TResult>
    {
        private readonly Func<TResult> _function;
        private readonly MyThreadPool _threadPool;
        private TResult? _result;
        private volatile bool _isCompleted = false;
        private Exception? _exception;
        private readonly ConcurrentQueue<Action> _continuationTasks = new();
        private readonly object _syncObject = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MyTask{TResult}"/> class.
        /// </summary>
        /// <param name="function">The function representing the task.</param>
        /// <param name="threadPool">The thread pool to which this task belongs.</param>
        public MyTask(Func<TResult> function, MyThreadPool threadPool)
        {
            _function = function;
            _threadPool = threadPool;
        }

        /// <inheritdoc />
        public bool IsCompleted => _isCompleted;

        /// <inheritdoc />
        public TResult? Result
        {
            get
            {
                lock (_syncObject)
                {
                    while (!_isCompleted)
                    {
                        Monitor.Wait(_syncObject);
                    }

                    if (_exception is not null)
                    {
                        throw new AggregateException(_exception);
                    }

                    return _result;
                }
            }
        }

        internal void Execute()
        {
            try
            {
                _result = _function();
            }
            catch (Exception e)
            {
                _exception = e;
            }
            finally
            {
                lock (_syncObject)
                {
                    _isCompleted = true;
                    Monitor.Pulse(_syncObject);
                    ExecuteContinuations();
                }
            }
        }

        private void ExecuteContinuations()
        {
            foreach (var continuation in _continuationTasks)
            {
                _threadPool._taskQueue.Enqueue(continuation);
            }
        }

        /// <inheritdoc />
        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult?, TNewResult> continuationFunction)
        {
            if (_threadPool.IsShutdown)
            {
                throw new InvalidOperationException("Thread pool was shut down.");
            }

            lock (_syncObject)
            {
                var continuationTask = new MyTask<TNewResult>(() => continuationFunction(Result), _threadPool);

                if (_isCompleted)
                {
                    _threadPool.SubmitContinuation(continuationTask.Execute);
                }
                else
                {
                    _continuationTasks.Enqueue(continuationTask.Execute);
                }

                return continuationTask;
            }
        }
    }
}