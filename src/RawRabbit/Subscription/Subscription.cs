using RabbitMQ.Client;
using RawRabbit.Consumer;
using System;

namespace RawRabbit.Subscription
{
	public interface ISubscription : IDisposable
	{
		string QueueName { get; }
		string[] ConsumerTags { get; }
		bool Active { get; }
	}

	public class Subscription : ISubscription
	{
		public string QueueName { get; }
		public string[] ConsumerTags { get; }
		public bool Active { get; set; }

		private readonly IBasicConsumer _consumer;

		public Subscription(IBasicConsumer consumer, string queueName)
		{
			Active = true;
			_consumer = consumer;
			var basicConsumer = consumer as DefaultBasicConsumer;
			if (basicConsumer == null)
			{
				return;
			}
			QueueName = queueName;
			ConsumerTags = basicConsumer.ConsumerTags;
		}

		public void Dispose()
		{
			if (!_consumer.Model.IsOpen)
			{
				return;
			}
			if (!Active)
			{
				return;
			}
			Active = false;
			_consumer.CancelAsync();
		}
	}
}
