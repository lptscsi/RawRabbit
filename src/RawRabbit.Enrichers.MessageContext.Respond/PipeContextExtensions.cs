using RawRabbit.Pipe;
using System;
using System.Collections.Generic;

namespace RawRabbit.Enrichers.MessageContext.Respond
{
	public static class PipeContextExtensions
	{
		private const string MessageContextType = "Respond:MessageContext:Type";

		public static IPipeContext AddMessageContextType<TMessageContext>(this IPipeContext context)
		{
			context.Properties.TryAdd(MessageContextType, typeof(TMessageContext));
			return context;
		}

		public static Type GetMessageContextType(this IPipeContext context)
		{
			return context.Get(MessageContextType, typeof(object));
		}
	}
}
