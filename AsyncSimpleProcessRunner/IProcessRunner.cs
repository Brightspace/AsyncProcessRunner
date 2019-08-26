using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleProcessRunner {

	public interface IProcessRunner {

		/// <summary>
		/// Runs the process.
		/// </summary>
		/// <param name="workingDirectory">The working directory.</param>
		/// <param name="process">The process name or file path.</param>
		/// <param name="arguments">The process arguments.</param>
		/// <param name="timeout">The execution timeout.</param>
		/// <returns>Returns the process output and exit code.</returns>
		ProcessResult Run(
				string workingDirectory,
				string process,
				string arguments,
				TimeSpan timeout
			);

		/// <summary>
		/// Runs the process asynchronously.
		/// </summary>
		/// <param name="workingDirectory">The working directory.</param>
		/// <param name="process">The process name or file path.</param>
		/// <param name="arguments">The process arguments.</param>
		/// <param name="timeout">The execution timeout.</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Returns the process output and exit code.</returns>
		Task<ProcessResult> RunAsync(
				string workingDirectory,
				string process,
				string arguments,
				TimeSpan timeout,
				CancellationToken cancellationToken = default( CancellationToken )
			);
	}
}
