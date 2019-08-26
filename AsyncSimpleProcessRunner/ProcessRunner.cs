using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncSimpleProcessRunner {

	public static partial class ProcessRunner {

		/// <summary>
		/// Runs the process asynchronously.
		/// </summary>
		/// <param name="workingDirectory">The working directory.</param>
		/// <param name="process">The process name or file path.</param>
		/// <param name="arguments">The process arguments.</param>
		/// <param name="timeout">The execution timeout.</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Returns the process output and exit code.</returns>
		public static async Task<ProcessResult> RunAsync(
				string workingDirectory,
				string process,
				string arguments,
				TimeSpan timeout,
				CancellationToken cancellationToken = default
			) {

			int exitCode;
			StringBuilder standardOutput = new StringBuilder();
			StringBuilder standardError = new StringBuilder();
			Stopwatch watch = new Stopwatch();

			CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );
			cts.CancelAfter( timeout );

			TaskCompletionSource<bool> exitTcs = new TaskCompletionSource<bool>();

			using( cts )
			using( cts.Token.Register( () => exitTcs.TrySetCanceled() ) )
			using( ManualResetEvent standardOutputEndEvent = new ManualResetEvent( false ) )
			using( ManualResetEvent standardErrorEndEvent = new ManualResetEvent( false ) )
			using( Process p = new Process() ) {

				ProcessStartInfo psi = p.StartInfo;
				psi.FileName = process;
				psi.Arguments = arguments;
				psi.WorkingDirectory = workingDirectory;

				psi.CreateNoWindow = true;
				psi.UseShellExecute = false;
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardError = true;

				p.EnableRaisingEvents = true;

				p.OutputDataReceived +=
					delegate ( object sender, DataReceivedEventArgs @event ) {

						if( @event.Data == null ) {
							try {
								standardOutputEndEvent.Set();
							} catch( ObjectDisposedException ) {
							}

						} else if( @event.Data.Length > 0 ) {
							lock( standardOutput ) {
								standardOutput.AppendLine( @event.Data );
							}
						}
					};

				p.ErrorDataReceived +=
					delegate ( object sender, DataReceivedEventArgs @event ) {

						if( @event.Data == null ) {
							try {
								standardErrorEndEvent.Set();
							} catch( ObjectDisposedException ) {
							}

						} else if( @event.Data.Length > 0 ) {
							lock( standardError ) {
								standardError.AppendLine( @event.Data );
							}
						}
					};

				p.Exited +=
					delegate ( object sender, EventArgs @event ) {
						exitTcs.TrySetResult( true );
					};

				watch.Start();

				p.Start();

				int processId = p.Id;
				DateTime startTime = p.StartTime;

				try {
					p.BeginOutputReadLine();
					p.BeginErrorReadLine();

					await exitTcs.Task.ConfigureAwait( false );

					// Call the blocking WaitForExit to ensure that the asynchronous output and error
					// streams have been received
					// http://msdn.microsoft.com/en-us/library/fb4aw7b8%28v=vs.110%29.aspx
					p.WaitForExit();

					watch.Stop();

					exitCode = p.ExitCode;

					string standardOutputTxt;
					lock( standardOutput ) {
						standardOutputTxt = standardOutput.ToString();
					}

					string standardErrorTxt;
					lock( standardError ) {
						standardErrorTxt = standardError.ToString();
					}

					ProcessResult result = new ProcessResult(
							workingDirectory: workingDirectory,
							process: process,
							arguments: arguments,
							exitCode: exitCode,
							standardOutput: standardOutputTxt,
							standardError: standardErrorTxt,
							duration: watch.Elapsed
						);

					return result;

				} catch( TaskCanceledException ) {
					p.CancelOutputRead();
					p.CancelErrorRead();

					if( cancellationToken.IsCancellationRequested ) {
						// Assume this means we didn't hit our timeout
						// and that the cts was cancelled do to a user's passed-in cancellation
						throw;
					}

					KillChildProcesses( processId, startTime );

					string timeoutMsg = String.Format(
							CultureInfo.InvariantCulture,
							"Timed out waiting for process {0} ( {1} ) to exit",
							process,
							arguments
						);

					string standardOutputTxt;
					lock( standardOutput ) {
						standardOutputTxt = standardOutput.ToString();
					}

					string standardErrorTxt;
					lock( standardError ) {
						standardErrorTxt = standardError.ToString();
					}

					throw new ProcessTimeoutException(
						message: timeoutMsg,
						workingDirectory: workingDirectory,
						process: process,
						arguments: arguments,
						standardOutput: standardOutputTxt,
						standardError: standardErrorTxt
					);
				} finally {

					p.Refresh();

					if( !p.HasExited ) {
						p.Kill();
					}
				}
			}
		}

		private static void KillChildProcesses(
				int parentProcessId,
				DateTime startTime
			) {

			if( parentProcessId <= 0 ) {
				return;
			}

			ChildProcess[] childProcesses = GetChildProcesses( parentProcessId )
				.Where( child => child.StartTime >= startTime )
				.ToArray();

			foreach( ChildProcess childProcess in childProcesses ) {

				KillChildProcesses(
						childProcess.ProcessId,
						childProcess.StartTime
					);

				try {
					using( Process proc = Process.GetProcessById( childProcess.ProcessId ) ) {
						proc.Kill();
					}
				} catch {
				}
			}
		}

		private const string QueryTempalte = @"
SELECT
	ProcessId,
	CreationDate

FROM Win32_Process
WHERE (
	ParentProcessId = {0}
)";

		private static IEnumerable<ChildProcess> GetChildProcesses( int parentProcessId ) {

			string query = String.Format(
					CultureInfo.InvariantCulture,
					QueryTempalte,
					parentProcessId
				);

			using( ManagementObjectSearcher searcher = new ManagementObjectSearcher( query ) )
			using( ManagementObjectCollection moc = searcher.Get() ) {

				foreach( ManagementObject mo in moc ) {

					using( mo ) {

						int childProcessId = Convert.ToInt32( mo[ "ProcessId" ] );

						string creationDate = mo[ "CreationDate" ].ToString();
						DateTime childStartTime = ManagementDateTimeConverter.ToDateTime( creationDate );

						yield return new ChildProcess( childProcessId, childStartTime );
					}
				}
			}
		}
	}
}
