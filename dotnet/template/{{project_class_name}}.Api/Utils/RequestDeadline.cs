namespace {{project_class_name}}.Api.Utils;

using System;
using System.Threading;

/// <summary>
/// Bounds the database work of one request. A failing or unreachable database ends in
/// the generic 500 within <see cref="Timeout"/> instead of running into the platform
/// timeout while the SDK retries.
/// </summary>
public static class RequestDeadline
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    /// <summary>A token that is cancelled when the request is aborted or the deadline passes.</summary>
    public static CancellationTokenSource Start(CancellationToken requestAborted)
    {
        var deadline = CancellationTokenSource.CreateLinkedTokenSource(requestAborted);
        deadline.CancelAfter(Timeout);
        return deadline;
    }
}
