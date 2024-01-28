using RabbitMQ.Client.Events;
using RawRabbit.Compatibility.Legacy.Configuration.Exchange;
using RawRabbit.Configuration;
using System;

namespace RawRabbit.Compatibility.Legacy.Configuration.Publish
{
	public class PublishConfiguration
	{
		public ExchangeConfiguration Exchange { get; set; }
		public string RoutingKey { get; set; }
		public Action<BasicPropertiesConfiguration> PropertyModifier { get; set; }
		public EventHandler<BasicReturnEventArgs> BasicReturn { get; set; }
	}
}
