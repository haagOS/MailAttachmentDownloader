using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Abstractions;

public interface IProcessor
{
    IAsyncEnumerable<ProcessorResult> ProcessMessageAsync(MimeMessage message, CancellationToken cancellationToken);
}

public sealed class ProcessorResult : IDisposable
{
    public MemoryStream MemoryStream { get; init; } = default!;
    public Metadata Metadata { get; init; } = default!;

    private bool _isDisposed = false;

    public ProcessorResult(MemoryStream memoryStream, Metadata metadata)
    {
        MemoryStream = memoryStream;
        Metadata = metadata;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            MemoryStream.Dispose();
        }

        _isDisposed = true;
    }

    ~ProcessorResult() => Dispose(false);
}
public record Metadata
{
    public string FileExtension { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string FullName => $"{Name}.{FileExtension}";
    public long Size { get; init; } = default!;
    public DateTimeOffset? CreationDate { get; init; }
    public string Email { get; init; } = default!;
    public IReadOnlyDictionary<string, object>? AdditionalData { get; init; }
    public DateTimeOffset ProcessingDate { get; init; } = default!;
    public int Number { get; init; } = default!;
}
