namespace MyThreadPool;

/// <summary>
/// Represents an asynchronous task with a result of type TResult.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
public interface IMyTask<out TResult>
{
    bool IsCompleted { get; }
    
    TResult? Result { get; }
    
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult?, TNewResult> func);
}