using RawRabbit.Configuration.Queue;
using System;

namespace RawRabbit.Configuration.Consume
{
	public static class ConsumeConfigExtensions
	{
		public static bool IsDirectReplyTo(this ConsumeConfiguration cfg)
		{
			return string.Equals(cfg.QueueName, QueueDecclarationExtensions.DirectQueueName, StringComparison.CurrentCultureIgnoreCase);
		}
	}
}
