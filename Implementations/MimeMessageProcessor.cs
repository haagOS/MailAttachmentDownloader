using Abstractions;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Implementations;

public class MimeMessageProcessor : IProcessor
{
    private readonly string[] _AcceptedFileEndings = new[] { "jpg", "png", "jpeg" };
    private readonly ILogger _Logger;

    public MimeMessageProcessor(ILogger logger)
    {
        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async IAsyncEnumerable<ProcessorResult> ProcessMessageAsync(MimeMessage message, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var from = message.From.Mailboxes.First().Address.Replace("@", "_at_");

        var i = 1;
        foreach (var part in message.BodyParts)
        {
            var contentFileName = part.ContentDisposition?.FileName ?? part.ContentType.Name;
            var creationDate = (part.ContentDisposition?.CreationDate ?? message.Date).ToLocalTime().ToString("yyyyMMdd_HHmmss");

            if (contentFileName is null)
            {
                _Logger.LogWarning("Missing file name");
                continue;
            }

            var extension = contentFileName.Split('.').Last();
            var name = contentFileName[..^(extension.Length + 1)];

            if (!_AcceptedFileEndings.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            {
                _Logger.LogWarning("Invalid content type");
                continue;
            }

            var memoryStream = new MemoryStream();

            if (part is MessagePart messagePart)
            {
                await messagePart.Message.WriteToAsync(memoryStream, cancellationToken);
            }
            else if (part is MimePart mimePart)
            {
                await mimePart.Content.DecodeToAsync(memoryStream, cancellationToken);
            }
            else
            {
                _Logger.LogWarning("Unknown message body part");
                continue;
            }

            if (memoryStream.Length > 0)
            {
                memoryStream.Position = 0;
                var metadata = new Metadata
                {
                    Email = message.From.Mailboxes.First().Address,
                    CreationDate = part.ContentDisposition?.CreationDate,
                    FileExtension = extension,
                    Name = name,
                    ProcessingDate = DateTimeOffset.Now,
                    Size = memoryStream.Length,
                    Number = i
                };
                yield return new ProcessorResult(memoryStream, metadata);
            }
            else
            {
                _Logger.LogWarning("Empty attachment");
            }

            i++;
        }
    }
}
