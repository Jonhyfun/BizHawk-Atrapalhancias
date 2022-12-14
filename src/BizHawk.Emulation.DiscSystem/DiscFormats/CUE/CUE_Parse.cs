//TODO - object initialization syntax cleanup

using System.Text.RegularExpressions;
using System.IO;

//http://digitalx.org/cue-sheet/index.html "all cue sheet information is a straight 1:1 copy from the cdrwin helpfile"
//http://www.gnu.org/software/libcdio/libcdio.html#Sectors
//this is actually a great reference. they use LSN instead of LBA.. maybe a good idea for us

namespace BizHawk.Emulation.DiscSystem.CUE
{
	/// <summary>
	/// Performs minimum parse processing on a cue file
	/// </summary>
	internal class ParseCueJob : DiscJob
	{
		private readonly string IN_CueString;

		private readonly bool IN_Strict;

		/// <param name="cueString">the cue string to parse</param>
		/// <param name="strict">Indicates whether parsing will be strict or lenient</param>
		public ParseCueJob(string cueString, bool strict = false)
		{
			IN_CueString = cueString;
			IN_Strict = strict;
		}

		/// <summary>
		/// output: the resulting minimally-processed cue file
		/// </summary>
		public CUE_File OUT_CueFile { get; private set; }

		private class CueLineParser
		{
			private int index;
			private readonly string str;
			public bool EOF;
			public CueLineParser(string line)
			{
				str = line;
			}

			public string ReadPath() { return ReadToken(Mode.Quotable); }
			public string ReadToken() { return ReadToken(Mode.Normal); }
			public string ReadLine()
			{
				int len = str.Length;
				string ret = str.Substring(index, len - index);
				index = len;
				EOF = true;
				return ret;
			}

			private enum Mode
			{
				Normal, Quotable
			}

			private string ReadToken(Mode mode)
			{
				if (EOF) return null;

				bool isPath = mode == Mode.Quotable;

				int startIndex = index;
				bool inToken = false;
				bool inQuote = false;
				for (; ; )
				{
					bool done = false;
					char c = str[index];
					bool isWhiteSpace = (c == ' ' || c == '\t');

					if (isWhiteSpace)
					{
						if (inQuote)
							index++;
						else
						{
							if (inToken)
								done = true;
							else
								index++;
						}
					}
					else
					{
						bool startedQuote = false;
						if (!inToken)
						{
							startIndex = index;
							if (isPath && c == '"')
								startedQuote = inQuote = true;
							inToken = true;
						}
						switch (str[index])
						{
							case '"':
								index++;
								if (inQuote && !startedQuote)
								{
									done = true;
								}
								break;
							case '\\':
								index++;
								break;

							default:
								index++;
								break;
						}
					}
					if (index == str.Length)
					{
						EOF = true;
						done = true;
					}
					if (done) break;
				}

				string ret = str.Substring(startIndex, index - startIndex);

				if (mode == Mode.Quotable)
					ret = ret.Trim('"');

				return ret;
			}
		}

		private void LoadFromString(ParseCueJob job)
		{
			string cueString = job.IN_CueString;
			TextReader tr = new StringReader(cueString);

			for (; ; )
			{
				job.CurrentLine++;
				string line = tr.ReadLine();
				if (line == null) break;
				line = line.Trim();
				if (line == "") continue;
				var clp = new CueLineParser(line);

				string key = clp.ReadToken().ToUpperInvariant();
				
				//remove nonsense at beginning
				if (!IN_Strict)
				{
					while (key.Length > 0)
					{
						char c = key[0];
						if(c == ';') break;
						if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) break;
						key = key.Substring(1);
					}
				}

				bool startsWithSemicolon = key.StartsWith(";");

				if (startsWithSemicolon)
				{
					clp.EOF = true;
					OUT_CueFile.Commands.Add(new CUE_File.Command.COMMENT(line));
				}
				else switch (key)
				{
					default:
						job.Warn($"Unknown command: {key}");
						break;

					case "CATALOG":
						if (OUT_CueFile.GlobalDiscInfo.Catalog != null)
							job.Warn("Multiple CATALOG commands detected. Subsequent ones are ignored.");
						else if (clp.EOF)
							job.Warn("Ignoring empty CATALOG command");
						else OUT_CueFile.Commands.Add(OUT_CueFile.GlobalDiscInfo.Catalog = new CUE_File.Command.CATALOG(clp.ReadToken()));
						break;

					case "CDTEXTFILE":
						if (OUT_CueFile.GlobalDiscInfo.CDTextFile != null)
							job.Warn("Multiple CDTEXTFILE commands detected. Subsequent ones are ignored.");
						else if (clp.EOF)
							job.Warn("Ignoring empty CDTEXTFILE command");
						else OUT_CueFile.Commands.Add(OUT_CueFile.GlobalDiscInfo.CDTextFile = new CUE_File.Command.CDTEXTFILE(clp.ReadPath()));
						break;

					case "FILE":
						{
							var path = clp.ReadPath();
							CueFileType ft;
							if (clp.EOF)
							{
								job.Error("FILE command is missing file type.");
								ft = CueFileType.Unspecified;
							}
							else
							{
								var strType = clp.ReadToken().ToUpperInvariant();
								switch (strType)
								{
									default:
										job.Error($"Unknown FILE type: {strType}");
										ft = CueFileType.Unspecified;
										break;
									case "BINARY": ft = CueFileType.BINARY; break;
									case "MOTOROLA": ft = CueFileType.MOTOROLA; break;
									case "BINARAIFF": ft = CueFileType.AIFF; break;
									case "WAVE": ft = CueFileType.WAVE; break;
									case "MP3": ft = CueFileType.MP3; break;
								}
							}
							OUT_CueFile.Commands.Add(new CUE_File.Command.FILE(path, ft));
						}
						break;

					case "FLAGS":
						{
							CueTrackFlags flags = default;
							while (!clp.EOF)
							{
								var flag = clp.ReadToken().ToUpperInvariant();
								switch (flag)
								{
									case "DATA":
									default:
										job.Warn($"Unknown FLAG: {flag}");
										break;
									case "DCP": flags |= CueTrackFlags.DCP; break;
									case "4CH": flags |= CueTrackFlags._4CH; break;
									case "PRE": flags |= CueTrackFlags.PRE; break;
									case "SCMS": flags |= CueTrackFlags.SCMS; break;
								}
							}
							if (flags == CueTrackFlags.None)
								job.Warn("Empty FLAG command");
							OUT_CueFile.Commands.Add(new CUE_File.Command.FLAGS(flags));
						}
						break;

					case "INDEX":
						{
							if (clp.EOF)
							{
								job.Error("Incomplete INDEX command");
								break;
							}
							string strindexnum = clp.ReadToken();
							if (!int.TryParse(strindexnum, out var indexnum) || indexnum < 0 || indexnum > 99)
							{
								job.Error($"Invalid INDEX number: {strindexnum}");
								break;
							}
							string str_timestamp = clp.ReadToken();
							var ts = new Timestamp(str_timestamp);
							if (!ts.Valid && !IN_Strict)
							{
								//try cleaning it up
								str_timestamp = Regex.Replace(str_timestamp, "[^0-9:]", "");
								ts = new Timestamp(str_timestamp);
							}
							if (!ts.Valid)
							{
								if (IN_Strict)
									job.Error($"Invalid INDEX timestamp: {str_timestamp}");
								break;
							}
							OUT_CueFile.Commands.Add(new CUE_File.Command.INDEX(indexnum, ts));
						}
						break;

					case "ISRC":
						if (OUT_CueFile.GlobalDiscInfo.ISRC != null)
							job.Warn("Multiple ISRC commands detected. Subsequent ones are ignored.");
						else if (clp.EOF)
							job.Warn("Ignoring empty ISRC command");
						else
						{
							var isrc = clp.ReadToken();
							if (isrc.Length != 12)
								job.Warn($"Invalid ISRC code ignored: {isrc}");
							else
							{
								OUT_CueFile.Commands.Add(OUT_CueFile.GlobalDiscInfo.ISRC = new CUE_File.Command.ISRC(isrc));
							}
						}
						break;

					case "PERFORMER":
						OUT_CueFile.Commands.Add(new CUE_File.Command.PERFORMER(clp.ReadPath() ?? ""));
						break;

					case "POSTGAP":
					case "PREGAP":
						{
							var str_msf = clp.ReadToken();
							var msf = new Timestamp(str_msf);
							if (!msf.Valid)
								job.Error($"Ignoring {{0}} with invalid length MSF: {str_msf}", key);
							else
							{
								if (key == "POSTGAP")
									OUT_CueFile.Commands.Add(new CUE_File.Command.POSTGAP(msf));
								else
									OUT_CueFile.Commands.Add(new CUE_File.Command.PREGAP(msf));
							}
						}
						break;

					case "REM":
						OUT_CueFile.Commands.Add(new CUE_File.Command.REM(clp.ReadLine()));
						break;

					case "SONGWRITER":
						OUT_CueFile.Commands.Add(new CUE_File.Command.SONGWRITER(clp.ReadPath() ?? ""));
						break;

					case "TITLE":
						OUT_CueFile.Commands.Add(new CUE_File.Command.TITLE(clp.ReadPath() ?? ""));
						break;

					case "TRACK":
						{
							if (clp.EOF)
							{
								job.Error("Incomplete TRACK command");
								break;
							}

							string str_tracknum = clp.ReadToken();
							if (!int.TryParse(str_tracknum, out int tracknum) || tracknum < 1 || tracknum > 99)
							{
								job.Error($"Invalid TRACK number: {str_tracknum}");
								break;
							}

							// TODO - check sequentiality? maybe as a warning

							CueTrackType tt;
							var str_trackType = clp.ReadToken();
							switch (str_trackType.ToUpperInvariant())
							{
								default:
									job.Error($"Unknown TRACK type: {str_trackType}");
									tt = CueTrackType.Unknown;
									break;
								case "AUDIO": tt = CueTrackType.Audio; break;
								case "CDG": tt = CueTrackType.CDG; break;
								case "MODE1/2048": tt = CueTrackType.Mode1_2048; break;
								case "MODE1/2352": tt = CueTrackType.Mode1_2352; break;
								case "MODE2/2336": tt = CueTrackType.Mode2_2336; break;
								case "MODE2/2352": tt = CueTrackType.Mode2_2352; break;
								case "CDI/2336": tt = CueTrackType.CDI_2336; break;
								case "CDI/2352": tt = CueTrackType.CDI_2352; break;
							}

							OUT_CueFile.Commands.Add(new CUE_File.Command.TRACK(tracknum, tt));
						}
						break;
				}

				if (!clp.EOF)
				{
					var remainder = clp.ReadLine();
					if (remainder.TrimStart().StartsWith(";"))
					{
						//add a comment
						OUT_CueFile.Commands.Add(new CUE_File.Command.COMMENT(remainder));
					}
					else job.Warn($"Unknown text at end of line after processing command: {key}");
				}

			} //end cue parsing loop

			job.FinishLog();
		} //LoadFromString

		public override void Run()
		{
			OUT_CueFile = new CUE_File();
			LoadFromString(this);
		}
	}



} //namespace