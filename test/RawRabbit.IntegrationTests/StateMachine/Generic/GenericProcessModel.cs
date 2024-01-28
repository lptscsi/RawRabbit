using RawRabbit.Operations.StateMachine;
using System;

namespace RawRabbit.IntegrationTests.StateMachine.Generic
{
	public class GenericProcessModel : Model<State>
	{
		public string Assignee { get; set; }
		public string Name { get; set; }
		public DateTime Deadline { get; set; }
	}
}
