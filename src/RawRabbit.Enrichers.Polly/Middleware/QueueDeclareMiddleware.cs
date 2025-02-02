﻿using RawRabbit.Common;
using RawRabbit.Configuration.Queue;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RawRabbit.Enrichers.Polly.Middleware
{
	public class QueueDeclareMiddleware : Pipe.Middleware.QueueDeclareMiddleware
	{
		public QueueDeclareMiddleware(ITopologyProvider topology, QueueDeclareOptions options = null)
				: base(topology, options)
		{
		}

		protected override Task DeclareQueueAsync(QueueDeclaration queue, IPipeContext context, CancellationToken token)
		{
			var policy = context.GetPolicy(PolicyKeys.QueueDeclare);
			return policy.ExecuteAsync(
				action: (ctx, token) => base.DeclareQueueAsync(queue, context, token),
				cancellationToken: token,
				contextData: new Dictionary<string, object>
				{
					[RetryKey.TopologyProvider] = Topology,
					[RetryKey.QueueDeclaration] = queue,
					[RetryKey.PipeContext] = context,
					[RetryKey.CancellationToken] = token,
				});
		}
	}
}
