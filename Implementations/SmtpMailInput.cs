using Abstractions;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Implementations;

public record SmtpMailInputOptions(string Username, string Password, string Host, int Port);

public sealed class SmtpMailInput : IInput, IDisposable
{
    private readonly SmtpMailInputOptions _options;
    private readonly ILogger _logger;
    private readonly ImapClient _imapClient;

    private IMailFolder? _inbox;
    private bool _disposed = false;

    public SmtpMailInput(SmtpMailInputOptions options, ILogger logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _imapClient = new ImapClient();
    }

    public Task CompleteMessageAsync(UniqueId uniqueId, CancellationToken cancellationToken = default)
    {
        if (!_imapClient.IsConnected)
        {
            throw new InvalidOperationException("client must be connected");
        }

        if (_inbox is null)
        {
            throw new InvalidOperationException("inbox ist not available");
        }

        return _inbox.AddFlagsAsync(uniqueId, MessageFlags.Seen, true, cancellationToken);
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        return _imapClient.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.Auto, cancellationToken);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _imapClient.DisconnectAsync(true, cancellationToken);
    }

    public async Task<InputResult?> GetNextAsync(CancellationToken cancellationToken = default)
    {
        if (!_imapClient.IsAuthenticated)
        {
            var auth = new SaslMechanismPlain(Encoding.UTF8, _options.Username, _options.Password);
            await _imapClient.AuthenticateAsync(auth, cancellationToken);
        }
        
        _inbox = _imapClient.Inbox;
        await _inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);
        var uids = _imapClient.Inbox.Search(SearchQuery.NotSeen);

        _logger.LogDebug("Found {Count} number of unread messages", uids.Count);

        if (uids.Count == 0)
        {
            return null;
        }

        return new InputResult(uids.First(), await _inbox.GetMessageAsync(uids.First(), cancellationToken));
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
            _imapClient?.Dispose();
        }

        _disposed = true;
    }

    ~SmtpMailInput() => Dispose(false);
}

