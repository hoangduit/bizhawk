﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BizHawk.MultiClient
{
	public partial class RecordMovie : Form
	{
		//Allow relative paths in record textbox
		//Hook up RecordFrom box
		//PAL vs NTSC when appropriate (by SystemID)
		//Other platform specific header lines

		Movie MovieToRecord;

		public RecordMovie()
		{
			InitializeComponent();
		}

		private string MakePath()
		{
			if (RecordBox.Text.Length == 0)
				return "";
			string path = RecordBox.Text;
			int x = path.LastIndexOf('\\');
			if (path.LastIndexOf('\\') == -1)
			{
				if (path[0] != '\\')
					path = path.Insert(0, "\\");
				path = PathManager.MakeAbsolutePath(Global.Config.MoviesPath, "") + path;

				if (path[path.Length - 4] != '.') //If no file extension, add .tas
					path += ".tas";
				return path;
			}
			else
				return path;
		}

		private void OK_Click(object sender, EventArgs e)
		{
			string path = MakePath();

			if (path.Length > 0)
			{
				MovieToRecord = new Movie(path, MOVIEMODE.RECORD);

				//Header
				MovieToRecord.SetHeaderLine(MovieHeader.AUTHOR, AuthorBox.Text);
				MovieToRecord.SetHeaderLine(MovieHeader.EMULATIONVERSION, MainForm.EMUVERSION);
				MovieToRecord.SetHeaderLine(MovieHeader.MOVIEVERSION, MovieHeader.MovieVersion);
				MovieToRecord.SetHeaderLine(MovieHeader.GUID, MovieHeader.MakeGUID());
				MovieToRecord.SetHeaderLine(MovieHeader.PLATFORM, Global.Emulator.SystemId);
				if (Global.Game != null)
					MovieToRecord.SetHeaderLine(MovieHeader.GAMENAME, Global.Game.FilesystemSafeName);
				else
					MovieToRecord.SetHeaderLine(MovieHeader.GAMENAME, "NULL");

				Global.MainForm.StartNewMovie(MovieToRecord, true);
				this.Close();
			}
			else
				MessageBox.Show("Please select a movie to record", "File selection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.InitialDirectory = PathManager.MakeAbsolutePath(Global.Config.MoviesPath, "");
			sfd.DefaultExt = ".tas";
			sfd.FileName = Global.Game.FilesystemSafeName;
			sfd.Filter = "Movie files (*.tas)|*.TAS";

			Global.Sound.StopSound();
			var result = sfd.ShowDialog();
			Global.Sound.StartSound();
			if (result == DialogResult.OK)
			{
				RecordBox.Text = sfd.FileName;
			}
		}

		private void RecordMovie_Load(object sender, EventArgs e)
		{
			StartFromCombo.SelectedIndex = 0;
			//TODO: populate combo with savestate slots that currently exist
		}

		private void RecordBox_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None; string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
		}

		private void RecordBox_DragDrop(object sender, DragEventArgs e)
		{
			string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
			RecordBox.Text = filePaths[0];
		}
	}
}
