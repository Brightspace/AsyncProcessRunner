using System;

namespace SimpleProcessRunner {

	partial class ProcessRunner {

		private sealed class ChildProcess {

			public readonly int ProcessId;
			public readonly DateTime StartTime;

			public ChildProcess(
					int processId,
					DateTime startTime
				) {

				this.ProcessId = processId;
				this.StartTime = startTime;
			}
		}
	}
}
