using RawRabbit.Operations.Request.Configuration.Abstraction;
using RawRabbit.Operations.Request.Context;
using RawRabbit.Pipe;
using System;

namespace RawRabbit
{
	public static class RequestContextExtensions
	{
		public static IRequestContext UseRequestConfiguration(this IRequestContext context, Action<IRequestConfigurationBuilder> configuration)
		{
			context.Properties.Add(PipeKey.ConfigurationAction, configuration);
			return context;
		}
	}
}
