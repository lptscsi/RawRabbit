﻿using RawRabbit.Common;
using RawRabbit.Configuration.Exchange;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RawRabbit.Enrichers.Polly.Middleware
{
	public class ExchangeDeclareMiddleware : Pipe.Middleware.ExchangeDeclareMiddleware
	{
		public ExchangeDeclareMiddleware(ITopologyProvider topologyProvider, ExchangeDeclareOptions options = null)
			: base(topologyProvider, options) { }

		protected override Task DeclareExchangeAsync(ExchangeDeclaration exchange, IPipeContext context, CancellationToken token)
		{
			var policy = context.GetPolicy(PolicyKeys.ExchangeDeclare);
			return policy.ExecuteAsync(
				action: (ctx, token) => base.DeclareExchangeAsync(exchange, context, token),
				cancellationToken: token,
				contextData: new Dictionary<string, object>
				{
					[RetryKey.TopologyProvider] = TopologyProvider,
					[RetryKey.ExchangeDeclaration] = exchange,
					[RetryKey.PipeContext] = context,
					[RetryKey.CancellationToken] = token,
				});
		}
	}
}
