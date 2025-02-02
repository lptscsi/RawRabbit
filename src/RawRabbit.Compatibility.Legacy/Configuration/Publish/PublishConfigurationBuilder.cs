using RabbitMQ.Client.Events;
using RawRabbit.Compatibility.Legacy.Configuration.Exchange;
using RawRabbit.Compatibility.Legacy.Configuration.Request;
using RawRabbit.Configuration;
using System;

namespace RawRabbit.Compatibility.Legacy.Configuration.Publish
{
	public class PublishConfigurationBuilder : IPublishConfigurationBuilder
	{
		private readonly ExchangeConfigurationBuilder _exchange;
		private string _routingKey;
		private Action<BasicPropertiesConfiguration> _properties;
		private const string _oneOrMoreWords = "#";
		private EventHandler<BasicReturnEventArgs> _basicReturn;

		public PublishConfiguration Configuration => new PublishConfiguration
		{
			Exchange = _exchange.Configuration,
			RoutingKey = _routingKey,
			PropertyModifier = _properties ?? (b => { }),
			BasicReturn = _basicReturn
		};

		public PublishConfigurationBuilder(ExchangeConfiguration defaultExchange = null, string routingKey = null)
		{
			_exchange = new ExchangeConfigurationBuilder(defaultExchange);
			_routingKey = routingKey ?? _oneOrMoreWords;
		}

		public PublishConfigurationBuilder(RequestConfiguration defaultConfig)
		{
			_exchange = new ExchangeConfigurationBuilder(defaultConfig.Exchange);
		}

		public IPublishConfigurationBuilder WithExchange(Action<IExchangeConfigurationBuilder> exchange)
		{
			exchange(_exchange);
			Configuration.Exchange = _exchange.Configuration;
			return this;
		}

		public IPublishConfigurationBuilder WithRoutingKey(string routingKey)
		{
			_routingKey = routingKey;
			return this;
		}

		public IPublishConfigurationBuilder WithProperties(Action<BasicPropertiesConfiguration> properties)
		{
			_properties = properties;
			return this;
		}

		public IPublishConfigurationBuilder WithMandatoryDelivery(EventHandler<BasicReturnEventArgs> basicReturn)
		{
			_basicReturn = basicReturn;
			return this;
		}
	}
}
