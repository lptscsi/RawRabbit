using System;

namespace RawRabbit.Configuration.BasicPublish
{
	public class BasicPublishConfigurationBuilder : IBasicPublishConfigurationBuilder
	{
		public BasicPublishConfiguration Configuration { get; }

		public BasicPublishConfigurationBuilder(BasicPublishConfiguration initial)
		{
			Configuration = initial;
		}

		public IBasicPublishConfigurationBuilder OnExchange(string exchange)
		{
			Configuration.ExchangeName = exchange;
			return this;
		}

		public IBasicPublishConfigurationBuilder WithRoutingKey(string routingKey)
		{
			Configuration.RoutingKey = routingKey;
			return this;
		}

		public IBasicPublishConfigurationBuilder AsMandatory(bool mandatory = true)
		{
			Configuration.Mandatory = mandatory;
			return this;
		}

		public IBasicPublishConfigurationBuilder WithProperties(Action<BasicPropertiesConfiguration> propAction)
		{
			Configuration.BasicProperties = Configuration.BasicProperties ?? new BasicPropertiesConfiguration();
			propAction?.Invoke(Configuration.BasicProperties);
			return this;
		}
	}
}
