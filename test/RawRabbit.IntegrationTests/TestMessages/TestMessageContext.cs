﻿using RawRabbit.Enrichers.MessageContext.Context;
using System;

namespace RawRabbit.IntegrationTests.TestMessages
{
	public class TestMessageContext : IMessageContext
	{
		public string Prop { get; set; }
		public Guid GlobalRequestId { get; set; }
	}
}
