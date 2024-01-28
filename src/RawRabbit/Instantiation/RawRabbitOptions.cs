using RawRabbit.Configuration;
using RawRabbit.DependencyInjection;
using System;

namespace RawRabbit.Instantiation
{
	public class RawRabbitOptions
	{
		public RawRabbitConfiguration ClientConfiguration { get; set; }
		public Action<IDependencyRegister> DependencyInjection { get; set; }
		public Action<IClientBuilder> Plugins { get; set; }
	}
}