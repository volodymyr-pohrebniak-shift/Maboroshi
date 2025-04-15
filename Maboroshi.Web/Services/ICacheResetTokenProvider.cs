using Microsoft.Extensions.Primitives;

namespace Maboroshi.Web.Services;

public interface ICacheResetTokenProvider
{
    IChangeToken GetChangeToken();
    void CancelAndRefresh();
}
