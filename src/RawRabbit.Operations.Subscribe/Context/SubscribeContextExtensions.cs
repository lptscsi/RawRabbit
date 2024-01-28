using RawRabbit.Configuration.Consumer;
using RawRabbit.Operations.Subscribe.Context;
using RawRabbit.Pipe;
using System;

namespace RawRabbit
{
	public static class SubscribeContextExtensions
	{
		public static ISubscribeContext UseSubscribeConfiguration(this ISubscribeContext context, Action<IConsumerConfigurationBuilder> configuration)
		{
			context.Properties.Add(PipeKey.ConfigurationAction, configuration);
			return context;
		}
	}
}
