﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace BizHawk.MultiClient
{
	public enum MOVIEMODE { INACTIVE, PLAY, RECORD, FINISHED };
	public class Movie
	{
		public MovieHeader Header = new MovieHeader();
		public SubtitleList Subtitles = new SubtitleList();
		public bool MakeBackup = true; //make backup before altering movie

		public bool IsText { get; private set; }
		public string Filename { get; private set; }
		public MOVIEMODE Mode { get; private set; }
		public int Rerecords { get; private set; }
		private int Frames;

		private MovieLog Log = new MovieLog();
		private int lastLog;

		public bool StartsFromSavestate { get; private set; }
		public bool Loaded { get; private set; }

		public Movie(string filename, MOVIEMODE m)
		{
			Mode = m;
			lastLog = 0;
			Rerecords = 0;
			this.Filename = filename;
			IsText = true;
			Frames = 0;
			StartsFromSavestate = false;
			if (filename.Length > 0)
				Loaded = true;
		}

		public Movie()
		{
			Filename = "";
			Mode = MOVIEMODE.INACTIVE;
			IsText = true;
			Frames = 0;
			StartsFromSavestate = false;
			Loaded = false;
		}

		public string SysID()
		{
			return Header.GetHeaderLine(MovieHeader.PLATFORM);
		}

		public string GUID()
		{
			return Header.GetHeaderLine(MovieHeader.GUID);
		}

		public string GetGameName()
		{
			return Header.GetHeaderLine(MovieHeader.GAMENAME);
		}
		public int Length()
		{
			if (Loaded)
				return Log.Length();
			else
				return Frames;
		}

		public void StopMovie()
		{
			if (Mode == MOVIEMODE.RECORD)
				WriteMovie();
			Mode = MOVIEMODE.INACTIVE;
		}

		public void StartNewRecording() { StartNewRecording(true); }
		public void StartNewRecording(bool truncate)
		{
			Mode = MOVIEMODE.RECORD;
			if (Global.Config.EnableBackupMovies && MakeBackup && Log.Length() > 0)
			{
				WriteBackup();
				MakeBackup = false;
			}
			if(truncate) Log.Clear();
		}

		public void StartPlayback()
		{
			Mode = MOVIEMODE.PLAY;
		}

		public void CommitFrame(int frameNum, IController source)
		{
			//if (Global.Emulator.Frame < Log.Length())
			//{
			//    Log.Truncate(Global.Emulator.Frame);
			//}

			//Note: Truncation here instead of loadstate will make VBA style loadstates
			//(Where an entire movie is loaded then truncated on the next frame
			//this allows users to restore a movie with any savestate from that "timeline"

			MnemonicsGenerator mg = new MnemonicsGenerator();
			
			mg.SetSource(source);
			Log.SetFrameAt(frameNum, mg.GetControllersAsMnemonic());
		}

		public string GetInputFrame(int frame)
		{
			lastLog = frame;
			if (frame < Log.Length())
				return Log.GetFrame(frame);
			else
				return "";
		}

		public void AppendFrame(string record)
		{
			Log.AddFrame(record);
		}

		public void InsertFrame(string record, int frame)
		{
			Log.SetFrameAt(frame, record);
		}

		public void WriteMovie()
		{
			if (!Loaded) return;
			if (Filename == "") return;
			Directory.CreateDirectory(new FileInfo(Filename).Directory.FullName);
			if (IsText)
				WriteText(Filename);
			else
				WriteBinary(Filename);
		}

		public void WriteBackup()
		{
			if (!Loaded) return;
			if (Filename == "") return;
			Directory.CreateDirectory(new FileInfo(Filename).Directory.FullName);
			string BackupName = Filename;
			BackupName = BackupName.Insert(Filename.LastIndexOf("."), String.Format(".{0:yyyy-MM-dd HH.mm.ss}", DateTime.Now));
			Global.RenderPanel.AddMessage("Backup movie saved to " + BackupName);
			if (IsText)
				WriteText(BackupName);
			else
				WriteBinary(BackupName);

		}

		private void WriteText(string file)
		{
			if (file.Length == 0) return;	//Nothing to write
			int length = Log.Length();

			using (StreamWriter sw = new StreamWriter(file))
			{
				Header.WriteText(sw);
				Subtitles.WriteText(sw);
				Log.WriteText(sw);
			}
		}

		private void WriteBinary(string file)
		{

		}

		private bool LoadText()
		{
			var file = new FileInfo(Filename);

			if (file.Exists == false)
			{
				Loaded = false;
				return false;
			}
			else
			{
				Header.Clear();
				Log.Clear();
			}

			using (StreamReader sr = file.OpenText())
			{
				string str = "";

				while ((str = sr.ReadLine()) != null)
				{
					if (str == "")
					{
						continue;
					}
					
					
					if (str.Contains(MovieHeader.RERECORDS))
					{
						str = ParseHeader(str, MovieHeader.RERECORDS);
						try
						{
							Rerecords = int.Parse(str);
						}
						catch
						{
							Rerecords = 0;
						}
					}
					else if (str.Contains(MovieHeader.STARTSFROMSAVESTATE))
					{
						str = ParseHeader(str, MovieHeader.STARTSFROMSAVESTATE);
						if (str == "1")
							StartsFromSavestate = true;
					}

					if (Header.AddHeaderFromLine(str))
						continue;

					if (str.StartsWith("subtitle") || str.StartsWith("sub"))
					{
						Subtitles.AddSubtitle(str);
					}
					else if (str[0] == '|')
					{
						Log.AddFrame(str);
					}
					else
					{
						Header.Comments.Add(str);
					}
				}
			}
			Loaded = true;
			return true;

		}

		/// <summary>
		/// Load Header information only for displaying file information in dialogs such as play movie
		/// </summary>
		/// <returns></returns>
		public bool PreLoadText()
		{
			Loaded = false;
			var file = new FileInfo(Filename);

			if (file.Exists == false)
				return false;
			else
			{
				Header.Clear();
				Log.Clear();
			}

			using (StreamReader sr = file.OpenText())
			{
				string str = "";
				int length = 0;
				while ((str = sr.ReadLine()) != null)
				{
					length += str.Length + 1;
					if (str == "")
					{
						continue;
					}
					else if (Header.AddHeaderFromLine(str)) 
						continue;
					
					if (str.StartsWith("subtitle") || str.StartsWith("sub"))
					{
						Subtitles.AddSubtitle(str);
					}
					else if (str[0] == '|')
					{
						int line = str.Length + 1;
						length -= line;
						int lines = (int)file.Length - length;
						this.Frames = lines / line;
						break;
					}
					else
						Header.Comments.Add(str);
				}
				sr.Close();
			}

			return true;
		}

		private bool LoadBinary()
		{
			return true;
		}

		public bool LoadMovie()
		{
			var file = new FileInfo(Filename);
			if (file.Exists == false)
			{
				Loaded = false;
				return false;
			}

			return LoadText();
		}


		public void DumpLogIntoSavestateText(TextWriter writer)
		{
			writer.WriteLine("[Input]");
			string s = MovieHeader.GUID + " " + Header.GetHeaderLine(MovieHeader.GUID);
			writer.WriteLine(s);
			for (int x = 0; x < Log.Length(); x++)
				writer.WriteLine(Log.GetFrame(x));
			writer.WriteLine("[/Input]");
		}

		public void LoadLogFromSavestateText(TextReader reader)
		{
			//We are in record mode so replace the movie log with the one from the savestate
			Global.MovieSession.MultiTrack.IsActive = false; //adelikat: Hack because this is causing crashes by being true when it shouldn't!
			if (!Global.MovieSession.MultiTrack.IsActive)
			{
				if (Global.Config.EnableBackupMovies && MakeBackup && Log.Length() > 0)
				{
					WriteBackup();
					MakeBackup = false;
				}
				Log.Clear();
				while (true)
				{
					string line = reader.ReadLine();
					if (line == null) break;
					if (line.Trim() == "") continue;
					if (line == "[Input]") continue;
					if (line == "[/Input]") break;
					if (line[0] == '|')
						Log.AddFrame(line);
				}
			}
			else
			{
				int i = 0;
				while (true)
				{
					string line = reader.ReadLine();
					if (line == null) break;
					if (line.Trim() == "") continue;
					if (line == "[Input]") continue;
					if (line == "[/Input]") break;
					if (line[0] == '|')
					{
						Log.SetFrameAt(i, line);
						i++;
					}
				}
			}
			if (Global.Emulator.Frame < Log.Length())
			{
				Log.Truncate(Global.Emulator.Frame);
			}
			IncrementRerecords();
		}

		public void IncrementRerecords()
		{
			Rerecords++;
			Header.UpdateRerecordCount(Rerecords);
		}

		public void	SetRerecords(int value)
		{
			Rerecords = value;
			Header.SetHeaderLine(MovieHeader.RERECORDS, Rerecords.ToString());
		}

		public void SetMovieFinished()
		{
			if (Mode == MOVIEMODE.PLAY)
				Mode = MOVIEMODE.FINISHED;
		}

		public string GetTime(bool preLoad)
		{
			string time = "";

			double seconds;
			if (preLoad)
				seconds = GetSeconds(Frames);
			else
				seconds = GetSeconds(Log.Length());
			int hours = ((int)seconds) / 3600;
			int minutes = (((int)seconds) / 60) % 60;
			double sec = seconds % 60;
			if (hours > 0)
				time += MakeDigits(hours) + ":";
			time += MakeDigits(minutes) + ":";
			time += Math.Round((decimal)sec, 2).ToString();
			return time;
		}

		private string MakeDigits(decimal num)
		{
			return MakeDigits((int)num);
		}

		private string MakeDigits(int num)
		{
			if (num < 10)
				return "0" + num.ToString();
			else
				return num.ToString();
		}

		private double GetSeconds(int frameCount)
		{
			const double NES_PAL = 50.006977968268290849;
			const double NES_NTSC = (double)60.098813897440515532;
			const double PCE = (7159090.90909090 / 455 / 263); //~59.826
			const double SMS_NTSC = (3579545 / 262.0 / 228.0);
			const double SMS_PAL = (3546893 / 313.0 / 228.0);
			const double NGP = (6144000.0 / (515 * 198));
			const double VBOY = (20000000 / (259 * 384 * 4));  //~50.273
			const double LYNX = 59.8;
			const double WSWAN = (3072000.0 / (159 * 256));
			double seconds = 0;
			double frames = (double)frameCount;
			if (frames < 1)
				return seconds;

			bool pal = false; //TODO: pal flag

			switch (Header.GetHeaderLine(MovieHeader.PLATFORM))
			{
				case "GG":
				case "SG":
				case "SMS":
					if (pal)
						return frames / SMS_PAL;
					else
						return frames / SMS_NTSC;
				case "FDS":
				case "NES":
				case "SNES":
					if (pal)
						return frames / NES_PAL;
					else
						return frames / NES_NTSC;
				case "PCE":
					return frames / PCE;

				//One Day!
				case "VBOY":
					return frames / VBOY;
				case "NGP":
					return frames / NGP;
				case "LYNX":
					return frames / LYNX;
				case "WSWAN":
					return frames / WSWAN;
				//********

				case "":
				default:
					if (pal)
						return frames / 50.0;
					else
						return frames / 60.0;
			}
		}

		public int CheckTimeLines(StreamReader reader)
		{
			return -1; //Hack
			//This function will compare the movie data to the savestate movie data to see if they match
			//TODO: Will eventually check header data too such as GUI
			/*
			MovieLog l = new MovieLog();
			string line;
			while (true)
			{
				line = reader.ReadLine();
				if (line.Trim() == "") continue;
				else if (line == "[Input]") continue;
				else if (line == "[/Input]") break;
				else if (line[0] == '|')
					l.AddFrame(line);
			}

			for (int x = 0; x < Log.Length(); x++)
			{
				string xs = Log.GetFrame(x);
				string ys = l.GetFrame(x);
				//if (Log.GetFrame(x) != l.GetFrame(x))
				if (xs != ys)
					return x;
			}
			return -1;
			 */
		}

		public int CompareTo(Movie Other, string parameter)
		{
			int compare = 0;
			if (parameter == "File")
			{
				compare = CompareFileName(Other);
				if (compare == 0)
				{
					compare = CompareSysID(Other);
					if (compare == 0)
					{
						compare = CompareGameName(Other);
						if (compare == 0)
							compare = CompareLength(Other);
					}
				}
			}
			else if (parameter == "SysID")
			{
				compare = CompareSysID(Other);
				if (compare == 0)
				{
					compare = CompareFileName(Other);
					if (compare == 0)
					{
						compare = CompareGameName(Other);
						if (compare == 0)
							compare = CompareLength(Other);
					}
				}
			}
			else if (parameter == "Game")
			{
				compare = CompareGameName(Other);
				if (compare == 0)
				{
					compare = CompareFileName(Other);
					if (compare == 0)
					{
						compare = CompareSysID(Other);
						if (compare == 0)
							compare = CompareLength(Other);
					}
				}
			}
			else if (parameter == "Length")
			{
				compare = CompareLength(Other);
				if (compare == 0)
				{
					compare = CompareFileName(Other);
					if (compare == 0)
					{
						compare = CompareSysID(Other);
						if (compare == 0)
							compare = CompareGameName(Other);
					}
				}
			}
			return compare;
		}

		private int CompareFileName(Movie Other)
		{
			string otherName = Path.GetFileName(Other.Filename);
			string thisName = Path.GetFileName(this.Filename);

			return thisName.CompareTo(otherName);
		}

		private int CompareSysID(Movie Other)
		{
			string otherSysID = Other.SysID();
			string thisSysID = this.SysID();

			if (thisSysID == null && otherSysID == null)
				return 0;
			else if (thisSysID == null)
				return -1;
			else if (otherSysID == null)
				return 1;
			else
				return thisSysID.CompareTo(otherSysID);
		}

		private int CompareGameName(Movie Other)
		{
			string otherGameName = Other.GetGameName();
			string thisGameName = this.GetGameName();

			if (thisGameName == null && otherGameName == null)
				return 0;
			else if (thisGameName == null)
				return -1;
			else if (otherGameName == null)
				return 1;
			else
				return thisGameName.CompareTo(otherGameName);
		}

		private int CompareLength(Movie Other)
		{
			int otherLength = Other.Frames;
			int thisLength = this.Frames;

			if (thisLength < otherLength)
				return -1;
			else if (thisLength > otherLength)
				return 1;
			else
				return 0;
		}

		private string ParseHeader(string line, string headerName)
		{
			string str;
			int x = line.LastIndexOf(headerName) + headerName.Length;
			str = line.Substring(x + 1, line.Length - x - 1);
			return str;
		}
	}
}
