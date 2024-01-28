using RabbitMQ.Client;
using RawRabbit.Configuration;
using RawRabbit.Serialization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RawRabbit.Pipe.Middleware
{
	public class BasicPropertiesOptions
	{
		public Action<IPipeContext, BasicPropertiesConfiguration> PropertyModier { get; set; }
		public Func<IPipeContext, BasicPropertiesConfiguration> GetOrCreatePropsFunc { get; set; }
		public Action<IPipeContext, BasicPropertiesConfiguration> PostCreateAction { get; set; }
	}

	public class BasicPropertiesMiddleware : Middleware
	{
		protected ISerializer Serializer;
		protected Func<IPipeContext, BasicPropertiesConfiguration> GetOrCreatePropsFunc;
		protected Action<IPipeContext, BasicPropertiesConfiguration> PropertyModifier;
		protected Action<IPipeContext, BasicPropertiesConfiguration> PostCreateAction;

		public BasicPropertiesMiddleware(ISerializer serializer, BasicPropertiesOptions options = null)
		{
			Serializer = serializer;
			PropertyModifier = options?.PropertyModier ?? ((ctx, props) => ctx.Get<Action<BasicPropertiesConfiguration>>(PipeKey.BasicPropertyModifier)?.Invoke(props));
			PostCreateAction = options?.PostCreateAction;
			GetOrCreatePropsFunc = options?.GetOrCreatePropsFunc ?? (Func<IPipeContext, BasicPropertiesConfiguration>)(ctx =>
			{
				BasicPropertiesConfiguration props = ctx.GetBasicProperties() ?? new BasicPropertiesConfiguration();
				props.MessageId = Guid.NewGuid().ToString();
				props.DeliveryMode = ctx.GetClientConfiguration().PersistentDeliveryMode ? Convert.ToByte(2) : Convert.ToByte(1);
				props.ContentType = Serializer.ContentType;
				return props;
			});
		}

		public override Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			BasicPropertiesConfiguration props = GetOrCreateBasicProperties(context);
			ModifyBasicProperties(context, props);
			InvokePostCreateAction(context, props);
			context.Properties.TryAdd(PipeKey.BasicProperties, props);
			return Next.InvokeAsync(context, token);
		}

		protected virtual void ModifyBasicProperties(IPipeContext context, BasicPropertiesConfiguration props)
		{
			PropertyModifier?.Invoke(context, props);
		}

		protected virtual void InvokePostCreateAction(IPipeContext context, BasicPropertiesConfiguration props)
		{
			PostCreateAction?.Invoke(context, props);
		}

		protected virtual BasicPropertiesConfiguration GetOrCreateBasicProperties(IPipeContext context)
		{
			return GetOrCreatePropsFunc(context);
		}
	}
}
