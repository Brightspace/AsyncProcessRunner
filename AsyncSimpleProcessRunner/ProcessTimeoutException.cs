using System;

namespace SimpleProcessRunner {

	public sealed class ProcessTimeoutException : TimeoutException {

		private readonly string m_workingDirectory;
		private readonly string m_process;
		private readonly string m_arguments;
		private readonly string m_standardOutput;
		private readonly string m_standardError;

		public ProcessTimeoutException(
				string message,
				string workingDirectory,
				string process,
				string arguments,
				string standardOutput,
				string standardError
			)
			: base( message ) {

			m_workingDirectory = workingDirectory;
			m_process = process;
			m_arguments = arguments;
			m_standardOutput = standardOutput;
			m_standardError = standardError;
		}

		public string WorkingDirectory {
			get { return m_workingDirectory; }
		}

		public string Process {
			get { return m_process; }
		}

		public string Arguments {
			get { return m_arguments; }
		}

		public string StandardOutput {
			get { return m_standardOutput; }
		}

		public string StandardError {
			get { return m_standardError; }
		}

	}
}
