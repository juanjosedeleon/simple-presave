namespace SimplePresave.Libraries.Model
{
    public class AzureSettings
    {
        public string AzureSqlConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string ServiceBusQueueName { get; set; }
    }
}
