using System;

namespace AsyncSimpleProcessRunner {

	public sealed class ProcessTimeoutException : TimeoutException {
		internal ProcessTimeoutException(
				string message,
				string workingDirectory,
				string process,
				string arguments,
				string standardOutput,
				string standardError
			)
			: base( message ) {

			WorkingDirectory = workingDirectory;
			Process = process;
			Arguments = arguments;
			StandardOutput = standardOutput;
			StandardError = standardError;
		}

		public string WorkingDirectory { get; }

		public string Process { get; }

		public string Arguments { get; }

		public string StandardOutput { get; }

		public string StandardError { get; }

	}
}
