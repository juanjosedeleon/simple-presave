using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimplePresave.Libraries.Model;
using SimplePresave.Libraries.Services;
using System.Text.Json;

namespace SimplePresave.Functions
{
    public class AddToLibraryFunction
    {
        private readonly AzureSettings _azureSettings;
        private readonly ILogger<AddToLibraryFunction> _logger;
        private readonly SpotifyService _spotifyService;

        public AddToLibraryFunction(
            ILogger<AddToLibraryFunction> logger,
            IOptions<AzureSettings> azureSettings,
            SpotifyService spotifyService)
        {
            _azureSettings = azureSettings.Value;
            _logger = logger;
            _spotifyService = spotifyService;
        }

        [Function(nameof(AddToLibraryFunction))]
        public async Task Run(
            [ServiceBusTrigger("presaves", Connection = "AzureWebJobsServiceBus")]
                ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            try
            {
                _logger.LogInformation($"Recibido el mensaje {message.MessageId}= {message.Body}");
                var presaveMessage = JsonSerializer.Deserialize<PresaveMessage>(message.Body.ToString());

                if (presaveMessage == null)
                {
                    _logger.LogError($"El mensaje {message.MessageId} no es válido.");
                    await messageActions.AbandonMessageAsync(message);

                    return;
                }
                await _spotifyService.AddSongToLibrary(presaveMessage);
                await messageActions.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar el mensaje {message.MessageId}");

                await messageActions.AbandonMessageAsync(message);
            }
        }
    }
}
