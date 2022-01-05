using Abstractions;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Implementations;

public class AzureShareOutput : IOutput
{
    private readonly ShareClient _shareClient;
    private readonly ILogger _logger;

    public AzureShareOutput(AzureShareOutputOptions options, ILogger logger)
    {
        _shareClient = new ShareClient(options.ConnectionString, options.ShareName);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessAsync(ProcessorResult processorResult, CancellationToken cancellationToken)
    {
        var root = _shareClient.GetRootDirectoryClient();
        var fileClient = await root.CreateFileAsync(processorResult.Metadata.FullName, processorResult.Metadata.Size);
        await fileClient.Value.UploadAsync(processorResult.MemoryStream);
    }
}

public record AzureShareOutputOptions(string ConnectionString, string ShareName);
