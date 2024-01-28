using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;
using System.Threading;
using System.Threading.Tasks;

namespace RawRabbit.Enrichers.HttpContext
{
	public class NetFxHttpContextMiddleware : StagedMiddleware
	{
		public override string StageMarker => Pipe.StageMarker.Initialized;

		public override Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
		{ 
			return Next.InvokeAsync(context, token);
		}
	}
}
