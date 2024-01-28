using RawRabbit.Configuration.Publisher;
using RawRabbit.Operations.Publish.Context;
using RawRabbit.Pipe;
using System;

namespace RawRabbit
{
	public static class PublishContextExtensions
	{
		public static IPublishContext UsePublishConfiguration(this IPublishContext context, Action<IPublisherConfigurationBuilder> configuration)
		{
			context.Properties.Add(PipeKey.ConfigurationAction, configuration);
			return context;
		}
	}
}
