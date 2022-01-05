using Abstractions;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Implementations;

public record FileSystemOutputOptions(string Path);

public class FileSystemOutput : IOutput
{
    private readonly FileSystemOutputOptions _options;
    private readonly ILogger _logger;

    public FileSystemOutput(FileSystemOutputOptions options, ILogger logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task ProcessAsync(ProcessorResult processorResult, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.Path);

        var dateAndTime = $"{processorResult.Metadata.CreationDate ?? processorResult.Metadata.ProcessingDate:yyyyMMdd_hhmmss}";
        var email = processorResult.Metadata.Email.Replace("@", "_at_");
        var fileName = $"{dateAndTime}_{email}_{processorResult.Metadata.Number}.{processorResult.Metadata.FileExtension}";

        var path = Path.Combine(_options.Path, fileName);

        using var fileStream = File.Create(path);
        await processorResult.MemoryStream.CopyToAsync(fileStream);

        _logger.LogInformation("Created new file at path {Path}", path);
    }
}

