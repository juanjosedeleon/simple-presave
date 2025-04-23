using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using SimplePresave.Libraries.Model;

namespace SimplePresave.Libraries.Repositories
{
    public class ServiceBusRepository
    {
        private readonly string _queueName;
        private readonly string _connectionString;

        public ServiceBusRepository(IOptions<AzureSettings> settings)
        {
            var connectionStrings = settings.Value;
            _connectionString = connectionStrings.ServiceBusConnectionString;
            _queueName = connectionStrings.ServiceBusQueueName;
        }

        public async Task SendPresaveMessage(string body, DateTimeOffset userSchedule)
        {
            var message = new ServiceBusMessage(body);
            // Añadimos un minuto para dar tiempo a Spotify de publicar la canción:
            message.ScheduledEnqueueTime = userSchedule.AddMinutes(1);

            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            var client = new ServiceBusClient(_connectionString, clientOptions);
            var sender = client.CreateSender(_queueName);

            await sender.SendMessageAsync(message);
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}
