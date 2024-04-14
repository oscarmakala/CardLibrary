using CardLibrary.Types;

namespace CardLibrary.Manager;

public class ResolveQueue
{
    private readonly Pool<CallbackQueueElement> _callbackElemPool = new();
    private readonly Queue<CallbackQueueElement> _callbackQueue = new();
    private MatchState _matchState;
    private readonly bool _skipDelay;
    private float _resolveDelay = 0f;
    private bool _isResolving = false;

    public ResolveQueue(MatchState matchState, bool skip)
    {
        _matchState = matchState;
        _skipDelay = skip;
    }

    public virtual void Update(float delta)
    {
        if (!(_resolveDelay > 0f)) return;
        _resolveDelay -= delta;
        if (_resolveDelay <= 0f)
            ResolveAll();
    }

    protected virtual void ResolveAll()
    {
        if (_isResolving)
            return;

        _isResolving = true;
        while (CanResolve())
        {
            Resolve();
        }

        _isResolving = false;
    }

    private void Resolve()
    {
        if (_callbackQueue.Count <= 0) return;
        var elem = _callbackQueue.Dequeue();
        _callbackElemPool.Dispose(elem);
        elem.Callback?.Invoke();
    }

    private bool CanResolve()
    {
        if (_resolveDelay > 0f)
            return false; //Is waiting delay
        return _callbackQueue.Count > 0;
    }

    private class CallbackQueueElement
    {
        public Action? Callback;
    }

    public void SetMatchState(MatchState state)
    {
        _matchState = state;
    }

    public virtual void Clear()
    {
        _callbackElemPool.Clear();
        _callbackQueue.Clear();
    }
}