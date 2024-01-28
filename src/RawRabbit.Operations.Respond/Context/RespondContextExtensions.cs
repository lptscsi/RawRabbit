using RawRabbit.Operations.Respond.Configuration;
using RawRabbit.Operations.Respond.Context;
using RawRabbit.Pipe;
using System;

namespace RawRabbit
{
	public static class RespondContextExtensions
	{
		public static IRespondContext UseRespondConfiguration(this IRespondContext context, Action<IRespondConfigurationBuilder> configuration)
		{
			context.Properties.Add(PipeKey.ConfigurationAction, configuration);
			return context;
		}
	}
}
