namespace RawRabbit.Configuration.BasicPublish
{
	public class BasicPublishConfiguration
	{
		public string ExchangeName { get; set; }
		public string RoutingKey { get; set; }
		public bool Mandatory { get; set; }
		public BasicPropertiesConfiguration BasicProperties { get; set; }
		public byte[] Body { get; set; }
	}
}
