﻿using Polly;
using RabbitMQ.Client;
using RawRabbit.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RawRabbit.Enrichers.Polly.Services
{
	public class ChannelFactory : Channel.ChannelFactory
	{
		protected Policy CreateChannelPolicy;
		protected Policy ConnectPolicy;
		protected Policy GetConnectionPolicy;

		public ChannelFactory(IConnectionFactory connectionFactory, RawRabbitConfiguration config, ConnectionPolicies policies = null)
			: base(connectionFactory, config)
		{
			CreateChannelPolicy = policies?.CreateChannel ?? Policy.NoOpAsync();
			ConnectPolicy = policies?.Connect ?? Policy.NoOpAsync();
			GetConnectionPolicy = policies?.GetConnection ?? Policy.NoOpAsync();
		}

		public override Task ConnectAsync(CancellationToken token = default(CancellationToken))
		{
			return ConnectPolicy.ExecuteAsync(
				action: (ctx, token) => base.ConnectAsync(token),
				contextData: new Dictionary<string, object>
				{
					[RetryKey.ConnectionFactory] = ConnectionFactory,
					[RetryKey.ClientConfiguration] = ClientConfig
				},
				cancellationToken: token
			);
		}

		protected override Task<IConnection> GetConnectionAsync(CancellationToken token = default(CancellationToken))
		{
			return GetConnectionPolicy.ExecuteAsync(
				action: (ctx, token) => base.GetConnectionAsync(token),
				contextData: new Dictionary<string, object>
				{
					[RetryKey.ConnectionFactory] = ConnectionFactory,
					[RetryKey.ClientConfiguration] = ClientConfig
				},
				cancellationToken: token
			);
		}

		public override Task<IModel> CreateChannelAsync(CancellationToken token = default(CancellationToken))
		{
			return CreateChannelPolicy.ExecuteAsync(
				action: (ctx, token) => base.CreateChannelAsync(token),
				contextData: new Dictionary<string, object>
				{
					[RetryKey.ConnectionFactory] = ConnectionFactory,
					[RetryKey.ClientConfiguration] = ClientConfig
				},
				cancellationToken: token
			);
		}
	}

	public class ConnectionPolicies
	{
		/// <summary>
		/// Used whenever 'CreateChannelAsync' is called.
		/// Expects an async policy.
		/// </summary>
		public Policy CreateChannel { get; set; }

		/// <summary>
		/// Used whenever an existing connection is retrieved.
		/// </summary>
		public Policy GetConnection { get; set; }

		/// <summary>
		/// Used when establishing the initial connection
		/// </summary>
		public Policy Connect { get; set; }
	}
}
