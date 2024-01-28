using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RawRabbit.Enrichers.Polly.Middleware
{
	public class HandlerInvocationMiddleware : Pipe.Middleware.HandlerInvocationMiddleware
	{
		public HandlerInvocationMiddleware(HandlerInvocationOptions options = null)
			: base(options) { }

		protected override Task InvokeMessageHandler(IPipeContext context, CancellationToken token)
		{
			var policy = context.GetPolicy(PolicyKeys.HandlerInvocation);
			return policy.ExecuteAsync(
				action: (ctx, token) => base.InvokeMessageHandler(context, token),
				cancellationToken: token,
				contextData: new Dictionary<string, object>
				{
					[RetryKey.PipeContext] = context,
					[RetryKey.CancellationToken] = token
				});
		}
	}
}
