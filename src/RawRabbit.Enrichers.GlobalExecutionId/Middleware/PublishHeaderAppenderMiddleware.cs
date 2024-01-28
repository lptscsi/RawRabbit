using RawRabbit.Configuration;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RawRabbit.Enrichers.GlobalExecutionId.Middleware
{
	public class PublishHeaderAppenderOptions
	{
		public Func<IPipeContext, BasicPropertiesConfiguration> BasicPropsFunc { get; set; }
		public Func<IPipeContext, string> GlobalExecutionIdFunc { get; set; }
		public Action<BasicPropertiesConfiguration, string> AppendHeaderAction { get; set; }
	}

	public class PublishHeaderAppenderMiddleware : StagedMiddleware
	{
		protected Func<IPipeContext, BasicPropertiesConfiguration> BasicPropsFunc;
		protected Func<IPipeContext, string> GlobalExecutionIdFunc;
		protected Action<BasicPropertiesConfiguration, string> AppendAction;
		public override string StageMarker => Pipe.StageMarker.BasicPropertiesCreated;

		public PublishHeaderAppenderMiddleware(PublishHeaderAppenderOptions options = null)
		{
			BasicPropsFunc = options?.BasicPropsFunc ?? (context => context.GetBasicProperties());
			GlobalExecutionIdFunc = options?.GlobalExecutionIdFunc ?? (context => context.GetGlobalExecutionId());
			AppendAction = options?.AppendHeaderAction ?? ((props, id) => props.Headers.TryAdd(PropertyHeaders.GlobalExecutionId, id));
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token = new CancellationToken())
		{
			var props = GetBasicProps(context);
			var id = GetGlobalExecutionId(context);
			AddIdToHeader(props, id);
			return Next.InvokeAsync(context, token);
		}

		protected virtual BasicPropertiesConfiguration GetBasicProps(IPipeContext context)
		{
			return BasicPropsFunc?.Invoke(context);
		}

		protected virtual string GetGlobalExecutionId(IPipeContext context)
		{
			return GlobalExecutionIdFunc?.Invoke(context);
		}

		protected virtual void AddIdToHeader(BasicPropertiesConfiguration props, string globalExecutionId)
		{
			AppendAction?.Invoke(props, globalExecutionId);
		}
	}
}
