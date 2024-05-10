namespace MyThreadPool;

/// <summary>
/// Represents an asynchronous task with a result of type TResult.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
public interface IMyTask<out TResult>
{
    /// <summary>
    /// Shows whether task is completed.
    /// </summary>
    public bool IsCompleted { get; }
    
    /// <summary>
    /// Gets the result of calculating the task.
    /// </summary>
    public TResult? Result { get; }
    
    /// <summary>
    /// Method to create new task that will be calculated, based on the results of task.
    /// </summary>
    /// <param name="func">Function representing new task.</param>
    /// <typeparam name="TNewResult">Type of new function output.</typeparam>
    /// <returns>Instance of new <see cref="IMyTask{TNewResult}"/></returns>
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult?, TNewResult> func);
}