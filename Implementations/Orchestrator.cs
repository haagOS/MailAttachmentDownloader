using Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Implementations;

public class Orchestrator
{
    private readonly IInput _input;
    private readonly IProcessor _processor;
    private readonly IOutput _output;
    private readonly ILogger _logger;

    public Orchestrator(IInput input, IProcessor processor, IOutput output, ILogger logger)
    {
        _input = input;
        _processor = processor;
        _output = output;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Start processing");

        try
        {
            await _input.ConnectAsync(cancellationToken);
            _logger.LogDebug("Successfully connected to the input");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while connecting. Cancel.");
            return;
        }

        while (true)
        {
            InputResult result;
            try
            {
                var tmp = await _input.GetNextAsync(cancellationToken);
                if (tmp is null)
                {
                    _logger.LogDebug("There is no message at the moment");
                    break;
                }

                _logger.LogDebug("Retrieved the next message with id {Id}", tmp.UniqueId);

                result = tmp;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Retrieving the next message failed. Cancel.");
                break;
            }

            try
            {
                await foreach (var item in _processor.ProcessMessageAsync(result.MimeMessage, cancellationToken))
                {
                    try
                    {
                        await _output.ProcessAsync(item, cancellationToken);
                        _logger.LogDebug("Successfully processed message with id {Id}", result.UniqueId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while dispatching an item of the processed message with id {Id}.", result.UniqueId);
                    }

                    item.Dispose();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while processing the message.");
            }
            finally
            {
                try
                {
                    await _input.CompleteMessageAsync(result.UniqueId, cancellationToken);
                    _logger.LogDebug("Message with id {Id} completed", result.UniqueId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while completing the processed message. This may result in duplicates.");
                }

                result.Dispose();
            }
        }

        try
        {
            await _input.DisconnectAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while disconnecting.");
        }
    }
}

