using RawRabbit.Configuration.Consumer;
using RawRabbit.Configuration.Publisher;
using System;

namespace RawRabbit.Operations.Request.Configuration.Abstraction
{
	public interface IRequestConfigurationBuilder
	{
		IRequestConfigurationBuilder PublishRequest(Action<IPublisherConfigurationBuilder> publish);
		IRequestConfigurationBuilder ConsumeResponse(Action<IConsumerConfigurationBuilder> consume);
	}
}