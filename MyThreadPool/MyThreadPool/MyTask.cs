using System.Collections.Concurrent;

namespace MyThreadPool;

public class MyTask<TResult> : IMyTask<TResult>
{
    private volatile bool _isCompleted = false;
    private readonly Func<TResult> _func;
    private readonly MyThreadPool _threadPool;
    private TResult? _result;
    private readonly ManualResetEvent _taskCompletedEvent = new (false);
    private readonly CancellationToken _cancellationToken;
    private readonly ConcurrentQueue<Action> _waitingContinuations = new();
    private readonly object _lockObject = new ();
    public bool IsCompleted => _isCompleted;
    
    public MyTask (Func<TResult> func, MyThreadPool threadPool, CancellationToken cancellationToken)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _threadPool = threadPool;
        _cancellationToken = cancellationToken;
    }

    public TResult? Result
    {
        get
        {
            if (!_isCompleted)
            {
                _taskCompletedEvent.WaitOne();
            }
            
            return _result;
        }
    }

    public void Execute()
    {
        lock (_lockObject)
        {
            try
            {
                _result = _func();
            }
            catch (Exception e)
            {
                _result = default;
                throw new AggregateException(e);
            }
            finally
            {
                _isCompleted = true;
                if (_waitingContinuations.TryDequeue(out var continuation))
                {
                    _threadPool.Submit(() => continuation);
                }
                _taskCompletedEvent.Set();
            }
        }
    }

    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult?, TNewResult> continueFunc)
    {
        if (_cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("Thread was shut down.");
        }

        lock (_lockObject)
        {
            if (IsCompleted)
            {
                return _threadPool.Submit(() => continueFunc(Result));
            }
            var newFunc = new Func<TNewResult>(() => continueFunc(Result));
            var newTask = new MyTask<TNewResult>(newFunc, _threadPool, _cancellationToken);
            
            _waitingContinuations.Enqueue(() => newTask.Execute());

            return newTask;
        }
    }
}