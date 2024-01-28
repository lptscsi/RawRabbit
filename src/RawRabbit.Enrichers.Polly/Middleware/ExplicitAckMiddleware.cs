using RawRabbit.Channel.Abstraction;
using RawRabbit.Common;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RawRabbit.Enrichers.Polly.Middleware
{
	public class ExplicitAckMiddleware : Pipe.Middleware.ExplicitAckMiddleware
	{
		public ExplicitAckMiddleware(INamingConventions conventions, ITopologyProvider topology, IChannelFactory channelFactory, ExplicitAckOptions options = null)
				: base(conventions, topology, channelFactory, options) { }

		protected override async Task<Acknowledgement> AcknowledgeMessageAsync(IPipeContext context)
		{
			var policy = context.GetPolicy(PolicyKeys.MessageAcknowledge);
			var result = await policy.ExecuteAsync(
				action: (ctx) => Task.FromResult(base.AcknowledgeMessageAsync(context)),
				contextData: new Dictionary<string, object>
				{
					[RetryKey.PipeContext] = context
				});
			return await result;
		}
	}
}
