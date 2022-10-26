using System.Threading;

public class CancellationTokenProvider
{
    private CancellationTokenSource _cts;

    public CancellationToken GetCancellationToken()
    {
        if (_cts == null)
        {
            _cts = new CancellationTokenSource();
        }

        return _cts.Token;
    }

    public void Cancel()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}