namespace KittenClaws.Api.Utils;

using System;
using System.Threading;

// Stops SDK retries against a failing database well before the platform timeout.
public static class RequestDeadline
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    public static CancellationTokenSource Start(CancellationToken requestAborted)
    {
        var deadline = CancellationTokenSource.CreateLinkedTokenSource(requestAborted);
        deadline.CancelAfter(Timeout);
        return deadline;
    }
}
