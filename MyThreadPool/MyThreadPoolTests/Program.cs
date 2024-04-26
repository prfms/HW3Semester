namespace MyThreadPoolTests;
using NUnit.Framework;
public class Tests
{
    private readonly int _threadPoolNumber = Environment.ProcessorCount;
    private MyThreadPool.MyThreadPool _threadPool = null!;

    [SetUp]
    public void Initialization()
    {
        _threadPool = new MyThreadPool.MyThreadPool(_threadPoolNumber);
    }

    [TearDown]
    public void Cleanup()
    {
        _threadPool.Shutdown();
    }

    [Test]
    public void Submit_FunctionReturningValue_ExpectedValueReturned()
    {
        var task = _threadPool.Submit(() => 2 + 2);
        Assert.That(task.Result, Is.EqualTo(4));
    }
    
    [Test]
    public void ThreadPoolShouldUseAllThreads()
    {
        for (var i = 0; i < _threadPoolNumber * 5; ++i)
        {
            _threadPool.Submit(() =>
            {
                Thread.Sleep(100);

                return 1;
            });
        }
        
        Thread.Sleep(200);
        
        Assert.That(_threadPool.WorkingThreadsNumber, Is.EqualTo(_threadPoolNumber));
    }

    [Test]
    public void ContinueWith_ParentAndContinuationTask_ParentTaskCompletesFirst()
    {
        var flag = false;

        var parentTask = _threadPool.Submit(() =>
        {
            Thread.Sleep(100);
            return 0;
        });

        parentTask.ContinueWith(_ =>
        {
            if (parentTask.IsCompleted)
            {
                Volatile.Write(ref flag, true);
            }
            return 1;
        });

        Thread.Sleep(200);

        Assert.That(flag, Is.True);
    }

    [Test]
    public void MultipleContinueWithShouldHaveExpectedResult()
    {
        const int expectedResult = 442;

        var myTask = _threadPool.Submit(() => 2 * 2)
            .ContinueWith(x => x.ToString())
            .ContinueWith(x => x + "42")
            .ContinueWith(int.Parse!);
        
        Assert.That(myTask.Result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void Shutdown_TasksSubmitted_AllTasksComplete()
    {
        var flag = false;

        _threadPool.Submit(() =>
        {
            Thread.Sleep(500);
            Volatile.Write(ref flag, true);
            return 0;
        });

        _threadPool.Shutdown();
        Assert.That(flag, Is.True);
    }

    [Test]
    public void ContinueWith_TasksAndMainThread_MainThreadNotBlocked()
    {
        var mainThreadContinueSignal = new AutoResetEvent(false);

        var parentTask = _threadPool.Submit(() =>
        {
            Thread.Sleep(10000);
            return 0;
        });

        var continuationTask = parentTask.ContinueWith(_ =>
        {
            mainThreadContinueSignal.Set();
            return 1;
        });

        var flag = !mainThreadContinueSignal.WaitOne(1000);

        Assert.That(flag, Is.True);
    }

    [Test]
    public void Submit_AfterShutdown_ThrowsInvalidOperationException()
    {
        _threadPool.Shutdown();
        Assert.Throws<InvalidOperationException>(() => _threadPool.Submit(() => 0));
    }

    [Test]
    public void ContinueWith_AfterShutdown_ThrowsInvalidOperationException()
    {
        var task = _threadPool.Submit(() => 0);
        _threadPool.Shutdown();
        Assert.Throws<InvalidOperationException>(() => task.ContinueWith(result => result + 1));
    }
}