using System;

namespace AsyncSimpleProcessRunner {

	public sealed class ProcessResult {
		internal ProcessResult(
				string workingDirectory,
				string process,
				string arguments,
				int exitCode,
				string standardOutput,
				string standardError,
				TimeSpan duration
			) {

			WorkingDirectory = workingDirectory;
			Process = process;
			Arguments = arguments;
			ExitCode = exitCode;
			StandardOutput = standardOutput;
			StandardError = standardError;
			Duration = duration;
		}

		public string WorkingDirectory { get; }

		public string Process { get; }

		public string Arguments { get; }

		public int ExitCode { get; }

		public string StandardOutput { get; }

		public string StandardError { get; }

		public TimeSpan Duration { get; }
	}
}
