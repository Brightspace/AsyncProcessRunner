using System;

namespace SimpleProcessRunner {

	public sealed class ProcessResult {

		private readonly string m_workingDirectory;
		private readonly string m_process;
		private readonly string m_arguments;
		private readonly int m_exitCode;
		private readonly string m_standardOutput;
		private readonly string m_standardError;
		private readonly TimeSpan m_duration;

		public ProcessResult(
				string workingDirectory,
				string process,
				string arguments,
				int exitCode,
				string standardOutput,
				string standardError,
				TimeSpan duration
			) {

			m_workingDirectory = workingDirectory;
			m_process = process;
			m_arguments = arguments;
			m_exitCode = exitCode;
			m_standardOutput = standardOutput;
			m_standardError = standardError;
			m_duration = duration;
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

		public int ExitCode {
			get { return m_exitCode; }
		}

		public string StandardOutput {
			get { return m_standardOutput; }
		}

		public string StandardError {
			get { return m_standardError; }
		}

		public TimeSpan Duration {
			get { return m_duration; }
		}
	}
}
