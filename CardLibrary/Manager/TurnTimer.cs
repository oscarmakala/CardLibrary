namespace CardLibrary.Manager;

public class TurnTimer
{
    /// <summary>
    /// 
    /// </summary>
    public Action? OnTimerFinishedCb;

    private Timer? _timer;
    private float _timeRemaining = 0f;
    private bool _isTimerRunning;

    /// <summary>
    /// 
    /// </summary>
    public virtual void StartTimer()
    {
        ResetTimer();
        _isTimerRunning = true;
        _timer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
    }

    private void TimerCallback(object? state)
    {
        if (!_isTimerRunning) return;
        _timeRemaining--;
        if (!(_timeRemaining <= 0)) return;
        _timeRemaining = 0;
        _timer?.Change(Timeout.Infinite, Timeout.Infinite); // Pause the timer for processing
        StopTimer();
        OnTimerFinishedCb?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void ResetTimer()
    {
        _timeRemaining = MatchConstants.TimePerTurn;
    }


    /// <summary>
    /// 
    /// </summary>
    public virtual void StopTimer()
    {
        _isTimerRunning = false;
        _timer?.Dispose(); //dispose th timer
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool TimeIsOver()
    {
        return _timeRemaining <= 0;
    }
}