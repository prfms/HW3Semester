namespace MyThreadPool;

public class MyTask<TResult> : IMyTask<TResult>
{
    private volatile bool _isCompleted = false;
    private readonly Func<TResult> _func;
    private MyThreadPool _threadPool;
    private TResult? _result;
    private CancellationToken _cancellationToken;
    private Action? _continuation { get; set; }
    public bool IsCompleted => _isCompleted;
    public TResult? Result => _result;
    
    public MyTask (Func<TResult> func, MyThreadPool threadPool, CancellationToken cancellationToken)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _threadPool = threadPool;
        _cancellationToken = cancellationToken;
    }

    public void Execute()
    {
        try
        {
            _result = _func();
            if (_continuation != null) _threadPool.Submit(() => _continuation);
        }
        catch (Exception ex)
        {
            _result = default;
            throw new AggregateException(ex);
        }
        finally
        {
            _isCompleted = true;
        }
        
    }

    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult?, TNewResult> continueFunc)
    {
        if (!IsCompleted) throw new Exception("Parent task hasn't been executed yet.");
        
        var newFunc = new Func<TNewResult>(() => continueFunc(Result));
        var newTask = new MyTask<TNewResult>(newFunc, _threadPool, _cancellationToken);

        _continuation = () => newTask.Execute();

        return newTask;
    }
}