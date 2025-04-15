using Microsoft.Extensions.Primitives;

namespace Maboroshi.Web.Services;

public class CacheResetTokenProvider : ICacheResetTokenProvider
{
    private CancellationTokenSource _cts = new();

    public IChangeToken GetChangeToken() => new CancellationChangeToken(_cts.Token);

    public void CancelAndRefresh()
    {
        var oldCts = _cts;
        oldCts.Cancel();
        oldCts.Dispose();
        _cts = new CancellationTokenSource();
    }
}
