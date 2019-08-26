using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleProcessRunner {

	public static class ProcessArgumentsFormatter {

		public static string AsProcessArguments(
				this IEnumerable<string> args
			) {

			return Format( args.ToArray() );
		}

		public static string Format( params object[] args ) {

			StringBuilder str = new StringBuilder();

			for( int i = 0; i < args.Length; i++ ) {

				if( i > 0 ) {
					str.Append( " " );
				}

				str.Append( "\"" );
				str.Append( args[i] );
				str.Append( "\"" );
			}

			return str.ToString();
		}
	}
}
