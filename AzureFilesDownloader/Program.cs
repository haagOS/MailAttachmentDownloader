using Azure.Storage.Files.Shares;
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

var shareClient = new ShareClient(
    configuration.GetValue<string>("StorageAccount:ConnectionString"),
    configuration.GetValue<string>("StorageAccount:ShareName"));

var directoryClient = shareClient.GetRootDirectoryClient();

var localDirectoryPath = Path.Combine(Environment.CurrentDirectory, "SyncFolder");
Directory.CreateDirectory(localDirectoryPath);

var logger = loggerFactory.CreateLogger<Program>();

while (true)
{
    logger.LogDebug("Checking for new uploads...");
    try
    {
        await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
        {
            if (item.IsDirectory)
            {
                continue;
            }

            var localFileName = Path.Combine(localDirectoryPath, item.Name);
            if (File.Exists(localFileName))
            {
                continue;
            }

            using var fileStream = File.Create(localFileName);
            var fileClient = directoryClient.GetFileClient(item.Name);
            using var stream = await fileClient.OpenReadAsync();
            await stream.CopyToAsync(fileStream);

            logger.LogDebug("Downloaded {Name}", item.Name);
        }
    }
    catch (Exception e)
    {
        logger.LogError(e, null);
    }

    await Task.Delay(TimeSpan.FromSeconds(10));
}