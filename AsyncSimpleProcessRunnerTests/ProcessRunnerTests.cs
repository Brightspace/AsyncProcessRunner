using System;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using AsyncSimpleProcessRunner;
using System.Threading.Tasks;

namespace AsyncSimpleProcessRunnerTests {

	[TestFixture]
	[Category( "Integration" )]
	public sealed class ProcessRunnerTests {

		private const int SixtySeconds = 60000;

		private readonly IProcessRunner m_runner = new AsyncSimpleProcessRunner.ProcessRunner();

		[Test]
		public async Task StandardOutput() {

			ProcessResult result = await m_runner
				.RunAsync(
					Environment.CurrentDirectory,
					@"C:\Windows\System32\cmd.exe",
					"/C echo hello world",
					TimeSpan.FromSeconds( 10 )
				)
				.ConfigureAwait( false );

			Assert.AreEqual( 0, result.ExitCode );
			Assert.AreEqual( "hello world", result.StandardOutput.Trim() );
			Assert.IsEmpty( result.StandardError );
		}

		[Test]
		public async Task StandardError() {

			ProcessResult result = await m_runner
				.RunAsync(
					Environment.CurrentDirectory,
					@"C:\Windows\System32\cmd.exe",
					@"/C dir Boom:\ReallyNotFound",
					TimeSpan.FromSeconds( 10 )
				)
				.ConfigureAwait( false );

			Assert.AreEqual( 1, result.ExitCode );
			Assert.IsEmpty( result.StandardOutput );
			Assert.AreEqual( "The filename, directory name, or volume label syntax is incorrect.", result.StandardError.Trim() );
		}

		[Test]
		public void Timeout_WithChildProcess() {

			string parentProcess = GetTestProcess( "TestParentProcess.exe" );
			string hangingProcess = GetTestProcess( "TestHangingProcess.exe" );

			string args = ProcessArgumentsFormatter.Format(
					hangingProcess,
					SixtySeconds
				);

			Assert.ThrowsAsync<ProcessTimeoutException>(
				() => m_runner.RunAsync(
					Environment.CurrentDirectory,
					parentProcess,
					args,
					TimeSpan.FromSeconds( 2 )
				)
			);
		}

		[Test]
		public void Timeout_WithNestedChildProcess() {

			string parentProcess = GetTestProcess( "TestParentProcess.exe" );
			string hangingProcess = GetTestProcess( "TestHangingProcess.exe" );

			string args = ProcessArgumentsFormatter.Format(
					parentProcess,
					hangingProcess,
					SixtySeconds
				);

			Assert.ThrowsAsync<ProcessTimeoutException>(
				() => m_runner.RunAsync(
					Environment.CurrentDirectory,
					parentProcess,
					args,
					TimeSpan.FromSeconds( 2 )
				)
			);
		}

		[Test]
		public void Timeout_WithDeepNestedChildProcess() {

			string parentProcess = GetTestProcess( "TestParentProcess.exe" );
			string hangingProcess = GetTestProcess( "TestHangingProcess.exe" );

			string args = ProcessArgumentsFormatter.Format(
					parentProcess,
					parentProcess,
					parentProcess,
					parentProcess,
					parentProcess,
					parentProcess,
					parentProcess,
					parentProcess,
					parentProcess,
					hangingProcess,
					SixtySeconds
				);

			Assert.ThrowsAsync<ProcessTimeoutException>(
				() => m_runner.RunAsync(
					Environment.CurrentDirectory,
					parentProcess,
					args,
					TimeSpan.FromSeconds( 2 )
				)
			);
		}

		[Test]
		[Explicit( "Should only be run manually" )]
		public void ContinuousTimeoutTest() {

			for( int i = 0; i < 1000; i++ ) {

				Timeout_WithNestedChildProcess();
				Thread.Sleep( 100 );
			}
		}

		private string GetTestProcess( string relativePath ) {

			Assembly aseembly = this.GetType().Assembly;
			FileInfo assemblyFile = new FileInfo( aseembly.Location );

			string path = Path.Combine( assemblyFile.DirectoryName, relativePath );
			return path;
		}
	}
}
