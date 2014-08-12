using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DynareRemoveLocalVariables
{
	class Program
	{
		static void Main( string[ ] lasArgs )
		{
			if( lasArgs.Length == 0 )
			{
				Console.WriteLine( "\nCommand line:\n\nDynareRemoveLocalVariables inputfile.mod [outputfile.mod] [1]\n" );
				Console.WriteLine( "The optional final argument (if present) tells the parser to convert lines starting with @ to comments.\nThis is useful if running on the output from Dynare's preprocessor." );
				return;
			}
			Console.WriteLine( "\nStarting." );
			string lsInput = File.ReadAllText( lasArgs[ 0 ] );
			bool lbConvertDefines = ( lasArgs.Length > 2 ) && ( lasArgs[ 2 ].Trim( ) == "1" );
			if( lbConvertDefines )
				Console.WriteLine( "Converting defines into comments." );
			else
				Console.WriteLine( "Not converting defines into comments." );
			bool lbChangeMade;
			Dictionary< Regex, string > ldsReplacements = new Dictionary<Regex,string>( );
			const string lksCharachterClass = @"[\`\¬\!\""\£\$\%\^\&\*\(\)\-\=\+\[\]\{\}\;\'\#\:\@\~\\\|\,\.\/\<\>\?\ \t\n\r\s]";
			do 
			{
				StringBuilder lOutput = new StringBuilder( );
				string[] lasInputLines = lsInput.Split( "\n\r".ToCharArray( ), StringSplitOptions.RemoveEmptyEntries );

				string lsLineInfo = "";
				int lnTabDepth = 0;
				int lnSpaceDepth = 0;
				lbChangeMade = false;

				foreach( string lsInputLine in lasInputLines )
				{
					string lsLine = lsInputLine.Trim( );
					if( lsLine.Length > 0 )
					{
						switch( lsLine[ 0 ] )
						{
						case '#':
							string lsTemp = lsLine.TrimStart( "# \t".ToCharArray( ) );
							string lsLocalVariableName = lsTemp.Remove( lsTemp.IndexOfAny( "= \t".ToCharArray( ) ) );
							Regex lNewRegex = new Regex( "(?<=(^|" + lksCharachterClass + "))" + lsLocalVariableName + "(?=($|" + lksCharachterClass + "))" );
							string lsReplacement = lsTemp.Substring( lsTemp.IndexOf( '=' ) + 1 );
							lsReplacement = " ( " + lsReplacement.Remove( lsReplacement.IndexOf( ';' ) ).Trim( ) + " ) ";
							foreach( Regex lRegex in ldsReplacements.Keys.ToArray( ) )
							{
								string lsValue = ldsReplacements[ lRegex ];
								if( lNewRegex.IsMatch( lsValue ) )
									ldsReplacements[ lRegex ] = lNewRegex.Replace( lsValue, lsReplacement );
							}
							ldsReplacements.Add( lNewRegex, lsReplacement );
							lbChangeMade = true;
							break;
						case '@':
							if( lbConvertDefines )
								lsLineInfo = lsLine.Substring( 2 );
							else
								lOutput.Append( lsInputLine ).Append( "\n" );
							break;
						default:
							string lsComment = "";
							if( lsLineInfo.Length > 0 )
							{
								lsComment = "\t//\t" + lsLineInfo;
								lsLineInfo = "";
							}
							int lnNewTabDepth = 0;
							int lnNewSpaceDepth = 0;
							for( int i = 0; i < lsInputLine.Length; ++i )
							{
								if( lsInputLine[ i ] == '\t' )
									++lnNewTabDepth;
								else if( lsInputLine[ i ] == ' ' )
									++lnNewSpaceDepth;
								else
									break;
							}

							bool lbNewLineNeeded = lnNewTabDepth != lnTabDepth || lnNewSpaceDepth != lnSpaceDepth;
							lnTabDepth = lnNewTabDepth;
							lnSpaceDepth = lnNewSpaceDepth;
							string lsTempLine = lsInputLine;
							foreach( KeyValuePair<Regex,string> lKVP in ldsReplacements )
							{
								if( lKVP.Key.IsMatch( lsTempLine ) )
								{
									lsTempLine = lKVP.Key.Replace( lsTempLine, lKVP.Value );
									lbChangeMade = true;
								}
							}
							lOutput.Append( lbNewLineNeeded ? "\n" : "" ).Append( lsTempLine ).Append( lsComment ).Append( "\n" );
							break;
						}
					}
				}
				lsInput = lOutput.ToString( );
				if( lbChangeMade )
					Console.WriteLine( "Changes made, rescanning." );
				else
					Console.WriteLine( "No changes made, saving output." );
			} while ( lbChangeMade );

			string lsOutputFileName;
			if( lasArgs.Length > 1 && lasArgs[ 1 ].Length > 0 )
				lsOutputFileName = lasArgs[ 1 ];
			else
				lsOutputFileName = lasArgs[ 0 ] + ".parsed.mod";
			File.WriteAllText( lsOutputFileName, lsInput );
			Console.WriteLine( "Done." );
		}
	}
}
