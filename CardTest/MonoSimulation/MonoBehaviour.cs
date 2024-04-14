using System.Diagnostics;

namespace CardTest.MonoSimulation;

public abstract class MonoBehaviour
{
    private bool _running = false;
    private Stopwatch _stopwatch;

    public virtual void Start()
    {
        if (_running) return;
        _running = true;
        _stopwatch = Stopwatch.StartNew();
        // Start a new thread for the update loop
        var updateThread = new Thread(UpdateLoop);
        updateThread.Start();
    }


    public void Stop()
    {
        _running = false;
    }

    private void UpdateLoop()
    {
        while (_running)
        {
            // Calculate deltaTime
            var deltaTime = (float)_stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();
            
            Update(deltaTime);
            // Sleep to approximate frame rate
            var waitTime = (int)(1000f / 200f - deltaTime * 1000);
            // If the frame was processed faster than the target frame time, wait for the remaining time
            if (waitTime > 0)
            {
                Thread.Sleep(waitTime);
            }
        }
    }

    protected abstract void Update(float deltaTime);
}