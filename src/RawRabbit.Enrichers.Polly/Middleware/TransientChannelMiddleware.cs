using RabbitMQ.Client;
using RawRabbit.Channel.Abstraction;
using RawRabbit.Pipe;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RawRabbit.Enrichers.Polly.Middleware
{
	public class TransientChannelMiddleware : Pipe.Middleware.TransientChannelMiddleware
	{
		public TransientChannelMiddleware(IChannelFactory factory)
			: base(factory) { }

		protected override Task<IModel> CreateChannelAsync(IPipeContext context, CancellationToken token)
		{
			var policy = context.GetPolicy(PolicyKeys.ChannelCreate);
			return policy.ExecuteAsync(
				action: (ctx, token) => base.CreateChannelAsync(context, token),
				cancellationToken: token,
				contextData: new Dictionary<string, object>
				{
					[RetryKey.PipeContext] = context,
					[RetryKey.CancellationToken] = token,
					[RetryKey.ChannelFactory] = ChannelFactory
				}
			);
		}
	}
}
