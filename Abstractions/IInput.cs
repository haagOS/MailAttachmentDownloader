using MailKit;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Abstractions;

public interface IInput
{
    /// <summary>
    /// Connect to the mail provider
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Disconnect from the mail provider
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get the next message
    /// </summary>
    Task<InputResult?> GetNextAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Mark a message as completed
    /// </summary>
    Task CompleteMessageAsync(UniqueId uniqueId, CancellationToken cancellationToken);
}

public sealed class InputResult : IDisposable
{
    public UniqueId UniqueId { get; init; }
    public MimeMessage MimeMessage { get; init; } = default!;

    private bool _disposed;

    public InputResult(UniqueId uniqueId, MimeMessage mimeMessage)
    {
        UniqueId = uniqueId;
        MimeMessage = mimeMessage;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            MimeMessage.Dispose();
        }

        _disposed = true;
    }

    ~InputResult() => Dispose(false);
}
