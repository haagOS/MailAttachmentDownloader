using Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Trace);
});

var username = configuration.GetValue<string>("IMAP:Username");
var password = configuration.GetValue<string>("IMAP:Password");
var host = configuration.GetValue<string>("IMAP:Host");
var port = configuration.GetValue<int>("IMAP:Port");

var smtpMailInputOptions = new SmtpMailInputOptions(username, password, host, port);
var input = new SmtpMailInput(smtpMailInputOptions, loggerFactory.CreateLogger<SmtpMailInput>());
var processor = new MimeMessageProcessor(loggerFactory.CreateLogger<MimeMessageProcessor>());
var fileSystemOutputOptions = new FileSystemOutputOptions(configuration.GetValue<string>("Output:Path"));
var output = new FileSystemOutput(fileSystemOutputOptions, loggerFactory.CreateLogger<FileSystemOutput>());

var orchestrator = new Orchestrator(input, processor, output, loggerFactory.CreateLogger<Orchestrator>());

while (true)
{
    await orchestrator.ExecuteAsync();
    await Task.Delay(TimeSpan.FromSeconds(10));
}