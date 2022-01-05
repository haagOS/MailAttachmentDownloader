using System.Threading.Tasks;
using Implementations;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MailAttachmentToAzureShare;

public class Function
{
    private readonly IConfiguration _configuration;

    public Function(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [FunctionName("Function")]
    public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger logger)
    {
        var username = _configuration.GetValue<string>("IMAP:Username");
        var password = _configuration.GetValue<string>("IMAP:Password");
        var host = _configuration.GetValue<string>("IMAP:Host");
        var port = _configuration.GetValue<int>("IMAP:Port");

        var smtpMailInputOptions = new SmtpMailInputOptions(username, password, host, port);
        var input = new SmtpMailInput(smtpMailInputOptions, logger);
        var processor = new MimeMessageProcessor(logger);
        var azureShareOutputOptions = new AzureShareOutputOptions(_configuration.GetValue<string>("StorageAccount:ConnectionString"), _configuration.GetValue<string>("StorageAccount:ShareName"));
        var output = new AzureShareOutput(azureShareOutputOptions, logger);

        var orchestrator = new Orchestrator(input, processor, output, logger);
        await orchestrator.ExecuteAsync();
    }
}
