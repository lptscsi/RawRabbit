﻿using RawRabbit.Enrichers.GlobalExecutionId.Middleware;
using RawRabbit.Instantiation;
using RawRabbit.Pipe.Middleware;
using System.Collections.Generic;

namespace RawRabbit.Enrichers.GlobalExecutionId
{
	public static class GlobalExecutionIdPlugin
	{
		public static IClientBuilder UseGlobalExecutionId(this IClientBuilder builder)
		{
			builder.Register(pipe => pipe
				// Pulisher
				.Use<AppendGlobalExecutionIdMiddleware>()
				.Use<ExecutionIdRoutingMiddleware>()
				.Use<PublishHeaderAppenderMiddleware>()

				// Subscriber
				.Use<WildcardRoutingKeyMiddleware>()

				// Message Received
				.Use<HeaderDeserializationMiddleware>(new HeaderDeserializationOptions
				{
					HeaderKeyFunc = c => PropertyHeaders.GlobalExecutionId,
					HeaderTypeFunc = c => typeof(string),
					ContextSaveAction = (ctx, id) => ctx.Properties.TryAdd(PipeKey.GlobalExecutionId, id)
				})
				.Use<PersistGlobalExecutionIdMiddleware>()
			);
			return builder;
		}
	}
}
