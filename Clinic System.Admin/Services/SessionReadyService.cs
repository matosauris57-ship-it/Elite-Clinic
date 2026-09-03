namespace DentalCare.Admin.Services;

public class SessionReadyService
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private volatile bool _isReady;

    public bool IsReady => _isReady;

    public bool HasToken { get; private set; }

    public Task WaitUntilReadyAsync() => _ready.Task;

    public void MarkReady(bool hasToken)
    {
        HasToken = hasToken;
        _isReady = true;
        _ready.TrySetResult();
    }
}
