using RawRabbit.Operations.MessageSequence.Model;
using RawRabbit.Operations.StateMachine;
using System.Collections.Generic;

namespace RawRabbit.Operations.MessageSequence.StateMachine
{
	public class SequenceModel : Model<SequenceState>
	{
		public bool Aborted { get; set; }
		public List<ExecutionResult> Completed { get; set; }
		public List<ExecutionResult> Skipped { get; set; }
	}
}
