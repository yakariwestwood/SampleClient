using SampleClient.Properties;
using RabbitMQ.Client;


namespace SampleClient.Interfaces
{
    public class RabbitMQService
    {
        public RabbitMQService()
        {
        }

        public IConnection GetRabbitMQConnection()
        {
            ConnectionFactory _conn = new ConnectionFactory()
            {
                HostName = Settings.Default.FeedAdress,
                UserName = Settings.Default.UserName,
                Password = Settings.Default.Password,
                Port = Protocols.DefaultProtocol.DefaultPort
            };

            return _conn.CreateConnection();
        }
    }
}