﻿using RawRabbit.Configuration.Exchange;
using System;

namespace RawRabbit.Enrichers.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ExchangeAttribute : Attribute
	{
		internal bool? NullableDurability;
		internal bool? NullableAutoDelete;

		public string Name { get; set; }
		public ExchangeType Type { get; set; }
		public bool Durable { set { NullableDurability = value; } }
		public bool AutoDelete { set { NullableAutoDelete = value; } }
	}
}
