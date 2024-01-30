using RabbitMQ.Client;
using RawRabbit.Common;
using RawRabbit.Exceptions;
using RawRabbit.Logging;
using RawRabbit.Operations.Publish.Context;
using RawRabbit.Operations.Publish.Utils;
using RawRabbit.Pipe;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RawRabbit.Operations.Publish.Middleware
{
	public class PublishAcknowledgeOptions
	{
		public Func<IPipeContext, TimeSpan> TimeOutFunc { get; set; }
		public Func<IPipeContext, IModel> ChannelFunc { get; set; }
		public Func<IPipeContext, bool> EnabledFunc { get; set; }
	}

	public class PublishAcknowledgeMiddleware : Pipe.Middleware.Middleware
	{
		private readonly IExclusiveLock _exclusive;
		private readonly ILog _logger = LogProvider.For<PublishAcknowledgeMiddleware>();
		protected Func<IPipeContext, TimeSpan> TimeOutFunc;
		protected Func<IPipeContext, IModel> ChannelFunc;
		protected Func<IPipeContext, bool> EnabledFunc;

		record ChannelDeliveryTagEntry(ulong deliveryTag, TaskCompletionSource<ulong> tcs, DateTime createdTime);

		record ChannelEntry(IModel channel, object syncLock, SortedSet<ChannelDeliveryTagEntry> sortedByTag, SortedSet<ChannelDeliveryTagEntry> sortedByTime);

		/// <summary>
		/// Processess Publisher confirmations
		/// </summary>
		static class AcknowledgementsBookKeeper
		{
			private class SortByTagComparer : IComparer<ChannelDeliveryTagEntry>
			{
				public int Compare(ChannelDeliveryTagEntry x, ChannelDeliveryTagEntry y)
				{
					return x.deliveryTag.CompareTo(y.deliveryTag);
				}
			}
			private class SortByTimeComparer : IComparer<ChannelDeliveryTagEntry>
			{
				public int Compare(ChannelDeliveryTagEntry x, ChannelDeliveryTagEntry y)
				{
					return x.createdTime.CompareTo(y.createdTime);
				}
			}

			static ConcurrentDictionary<IModel, ChannelEntry> _channelsDictionary = new ConcurrentDictionary<IModel, ChannelEntry>();

			static JobTimer _jobTimer;

			static Lazy<ILog> _logger;

			/// <summary>
			/// Static constructor
			/// </summary>
			static AcknowledgementsBookKeeper()
			{
				_logger = new Lazy<ILog>(() =>
				{
					return LogProvider.For<PublishAcknowledgeMiddleware>();
				}, true);

				_jobTimer = new JobTimer(TimeSpan.FromMilliseconds(2000),
				() => {
					OnTimer();
				},
				() => _channelsDictionary.Count,
				CancellationToken.None);
			}

			#region Private Methods

			/// <summary>
			/// Timer event handler
			/// </summary>
			private static void OnTimer()
			{
				foreach (ChannelEntry channelEntry in _channelsDictionary.Values.ToList())
				{
					try
					{
						IEnumerable<ChannelDeliveryTagEntry> tagEntries;

						if (channelEntry.channel.IsClosed)
						{
							int count = 0;
							lock (channelEntry.syncLock)
							{
								count = channelEntry.sortedByTag.Count;
							}

							if (count == 0)
							{
								_channelsDictionary.TryRemove(channelEntry.channel, out _);
								continue;
							}
						}

						lock (channelEntry.syncLock)
						{
							// get entries with timeouts
							tagEntries = GetEntriesByTime(channelEntry, DateTime.UtcNow);

							foreach (var tagEntry in tagEntries)
							{
								RemoveTagEntry(channelEntry, tagEntry);
							}
						}

						foreach (var tagEntry in tagEntries)
						{
							tagEntry.tcs.TrySetException(new PublishConfirmException($"The broker did not confirm publishing for message {tagEntry.deliveryTag}."));
						}
					}
					catch (Exception ex)
					{
						_logger.Value.Error(ex.Message, ex);
					}
				}
			}

			static bool RemoveTagEntry(ChannelEntry channelEntry, ChannelDeliveryTagEntry tagEntry)
			{
				// no need to lock here. This method call is always locked
				return channelEntry.sortedByTag.Remove(tagEntry) &&	channelEntry.sortedByTime.Remove(tagEntry);
			}

			static IEnumerable<ChannelDeliveryTagEntry> GetEntriesByTag(ChannelEntry channelEntry, ulong tag, bool multiple)
			{
				ChannelDeliveryTagEntry tagEntry = new ChannelDeliveryTagEntry(tag, null, new DateTime(0));
				if (multiple) {
					ChannelDeliveryTagEntry minTagEntry = new ChannelDeliveryTagEntry(0, null, new DateTime(0));
					return channelEntry.sortedByTag.GetViewBetween(minTagEntry, tagEntry).ToList();
				}
				else
				{
					if (channelEntry.sortedByTag.TryGetValue(tagEntry, out var res))
					{
						return new ChannelDeliveryTagEntry[] { res };
					}
					else
					{
						return new ChannelDeliveryTagEntry[0];
					}
				}
			}

			static IEnumerable<ChannelDeliveryTagEntry> GetEntriesByTime(ChannelEntry channelEntry, DateTime time)
			{
				ChannelDeliveryTagEntry tagEntry = new ChannelDeliveryTagEntry(0, null, time);

				ChannelDeliveryTagEntry minTagEntry = new ChannelDeliveryTagEntry(0, null, new DateTime(0));
				return channelEntry.sortedByTime.GetViewBetween(minTagEntry, tagEntry).ToList();
			}

			#endregion

			public static bool ProcessAcknowledgement(
				IModel channel,
				ulong deliveryTag,
				bool multiple,
				bool isOk,
				CancellationToken token)
			{
				bool isFound = _channelsDictionary.TryGetValue(channel, out ChannelEntry channelEntry);
				if (!isFound)
				{
					return false;
				}
				IEnumerable<ChannelDeliveryTagEntry> tagEntries;

				lock (channelEntry.syncLock)
				{
					tagEntries = GetEntriesByTag(channelEntry, deliveryTag, multiple);
					foreach (var tagEntry in tagEntries)
					{
						if (!RemoveTagEntry(channelEntry, tagEntry))
						{
							_logger.Value.Warn($"Did not find entry for deliveryTag: {tagEntry.deliveryTag}");
						}
					}
				}


				foreach (var tagEntry in tagEntries)
				{
					if (token.IsCancellationRequested)
					{
						return true;
					}

					_ = isOk ?
								tagEntry.tcs.TrySetResult(tagEntry.deliveryTag) :
								tagEntry.tcs.TrySetException(new PublishConfirmException($"The broker sent a NACK publish acknowledgement for message {deliveryTag}."));

				}

				return true;
			}

			public static ChannelEntry GetOrAddChannel(IModel channel)
			{
				ChannelEntry result = _channelsDictionary.GetOrAdd(channel, (channel) =>
				{
					ChannelEntry entry = new ChannelEntry(
						channel,
						new object(),
						new SortedSet<ChannelDeliveryTagEntry>(new SortByTagComparer()),
						new SortedSet<ChannelDeliveryTagEntry>(new SortByTimeComparer())
						);
					return entry;
				});

				_jobTimer.StartIfStopped();

				return result;
			}

			public static void AddTagEntry(ChannelEntry channelEntry, ChannelDeliveryTagEntry tagEntry)
			{
				lock (channelEntry.syncLock)
				{
					channelEntry.sortedByTag.Add(tagEntry);
					channelEntry.sortedByTime.Add(tagEntry);
				}
			}
		}

		public PublishAcknowledgeMiddleware(IExclusiveLock exclusive, PublishAcknowledgeOptions options = null)
		{
			_exclusive = exclusive;
			TimeOutFunc = options?.TimeOutFunc ?? (context => context.GetPublishAcknowledgeTimeout());
			ChannelFunc = options?.ChannelFunc ?? (context => context.GetTransientChannel());
			EnabledFunc = options?.EnabledFunc ?? (context => context.GetPublishAcknowledgeTimeout() != TimeSpan.MaxValue);
		}

		public override async Task InvokeAsync(IPipeContext context, CancellationToken token)
		{
			bool enabled = GetEnabled(context);
			if (!enabled)
			{
				_logger.Debug("Publish Acknowledgement is disabled.");
				await Next.InvokeAsync(context, token);
				return;
			}
			IModel channel = GetChannel(context);

			if (!PublishAcknowledgeEnabled(channel))
			{
				EnableAcknowledgement(channel, token);
			}

			TaskCompletionSource<ulong> ackTcs = new TaskCompletionSource<ulong>(TaskCreationOptions.RunContinuationsAsynchronously);
			ChannelEntry channelEntry = AcknowledgementsBookKeeper.GetOrAddChannel(channel);

			await _exclusive.ExecuteAsync(channelEntry, o =>
			{
				ulong sequence = channel.NextPublishSeqNo;
				TimeSpan timeout = GetAcknowledgeTimeOut(context);
				ChannelDeliveryTagEntry tagEntry = new ChannelDeliveryTagEntry(sequence, ackTcs, DateTime.UtcNow + timeout);
				AcknowledgementsBookKeeper.AddTagEntry(channelEntry, tagEntry);
				_logger.Info("Sequence {sequence} added to dictionary", sequence);

				return Next.InvokeAsync(context, token);
			}, token);

			await ackTcs.Task;
		}

		protected virtual TimeSpan GetAcknowledgeTimeOut(IPipeContext context)
		{
			return TimeOutFunc(context);
		}

		protected virtual bool PublishAcknowledgeEnabled(IModel channel)
		{
			return channel.NextPublishSeqNo != 0UL;
		}

		protected virtual IModel GetChannel(IPipeContext context)
		{
			return ChannelFunc(context);
		}

		protected virtual bool GetEnabled(IPipeContext context)
		{
			return EnabledFunc(context);
		}

		protected virtual void EnableAcknowledgement(IModel channel, CancellationToken token)
		{
			_logger.Info("Setting 'Publish Acknowledge' for channel '{channelNumber}'", channel.ChannelNumber);
			_exclusive.Execute(channel, c =>
			{
				if (PublishAcknowledgeEnabled(c))
				{
					return;
				}
				c.ConfirmSelect();

				c.BasicAcks += (sender, args) =>
				{
					AcknowledgementsBookKeeper.ProcessAcknowledgement(channel, args.DeliveryTag, args.Multiple, true, token);
				};

				c.BasicNacks += (sender, args) =>
				{
					AcknowledgementsBookKeeper.ProcessAcknowledgement(channel, args.DeliveryTag, args.Multiple, false, token);
				};
			}, token);
		}
	}

	public static class PublishAcknowledgePipeGetExtensions
	{
		public static TimeSpan GetPublishAcknowledgeTimeout(this IPipeContext context)
		{
			var fallback = context.GetClientConfiguration().PublishConfirmTimeout;
			return context.Get(PublishKey.PublishAcknowledgeTimeout, fallback);
		}
	}
}

namespace RawRabbit
{
	public static class PublishAcknowledgePipeUseExtensions
	{
		public static IPublishContext UsePublishAcknowledge(this IPublishContext context, TimeSpan timeout)
		{
			context.Properties.TryAdd(Operations.Publish.PublishKey.PublishAcknowledgeTimeout, timeout);
			return context;
		}

		public static IPublishContext UsePublishAcknowledge(this IPublishContext context, bool use = true)
		{
			return !use
				? context.UsePublishAcknowledge(TimeSpan.MaxValue)
				: context;
		}
	}
}

