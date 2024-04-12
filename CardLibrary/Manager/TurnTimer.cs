namespace CardLibrary.Manager;

public class TurnTimer
{
    public Action? OnTimerFinishedCb;
    private float _timer;
    private bool _isTimerRunning;

    public void StartTimer()
    {
        ResetTimer();
        _isTimerRunning = true;
    }

    public void ResetTimer()
    {
        _timer = MatchConstants.TimePerTurn;
    }

    public void Update(float deltaTime)
    {
        if (!_isTimerRunning) return;
        _timer -= deltaTime;

        if (!(_timer <= 0)) return;
        _timer = 0;
        StopTimer();
        OnTimerFinishedCb?.Invoke();
    }

    public void StopTimer()
    {
        _isTimerRunning = false;
    }

    public bool TimeIsOver()
    {
        return _timer <= 0;
    }
}