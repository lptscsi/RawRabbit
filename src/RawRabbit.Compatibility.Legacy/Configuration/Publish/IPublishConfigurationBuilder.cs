using RabbitMQ.Client.Events;
using RawRabbit.Compatibility.Legacy.Configuration.Exchange;
using RawRabbit.Configuration;
using System;

namespace RawRabbit.Compatibility.Legacy.Configuration.Publish
{
	public interface IPublishConfigurationBuilder
	{
		IPublishConfigurationBuilder WithExchange(Action<IExchangeConfigurationBuilder> exchange);
		IPublishConfigurationBuilder WithRoutingKey(string routingKey);
		IPublishConfigurationBuilder WithProperties(Action<BasicPropertiesConfiguration> properties);
		IPublishConfigurationBuilder WithMandatoryDelivery(EventHandler<BasicReturnEventArgs> basicReturn);
	}
}
