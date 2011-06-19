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
	public partial class PlayMovie : Form
	{
		//TODO: after browse & update, focus on the movie just added, and show stats
		//This is a modal dialog, implement it as modeless
		// Option to include subdirectories
		// Option to include savestate files (that have an input log)
		//Clicking column headers should sort info
		//AddMovieToList should check for duplicates and not add them

		List<Movie> MovieList = new List<Movie>();
		bool sortReverse;
		string sortedCol;

		public PlayMovie()
		{
			InitializeComponent();
			MovieView.QueryItemText += new QueryItemTextHandler(MovieView_QueryItemText);
			MovieView.QueryItemBkColor += new QueryItemBkColorHandler(MovieView_QueryItemBkColor);
			MovieView.VirtualMode = true;
			sortReverse = false;
			sortedCol = "";
		}

		void MovieView_QueryItemText(int index, int column, out string text)
		{
			text = "";
			if (column == 0) //File
				text = Path.GetFileName(MovieList[index].GetFilePath());
			if (column == 1) //System
				text = MovieList[index].GetSysID();
			if (column == 2) //Game
				text = MovieList[index].GetGameName();
			if (column == 3) //Time
				text = MovieList[index].GetTime(true);
		}

		private void MovieView_QueryItemBkColor(int index, int column, ref Color color)
		{

		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void Run()
		{
			Global.MainForm.StartNewMovie(MovieList[MovieView.SelectedIndices[0]], false);
		}

		private void OK_Click(object sender, EventArgs e)
		{
			Global.MainForm.ReadOnly = ReadOnlyCheckBox.Checked;
			Run();
			this.Close();
		}

		private void BrowseMovies_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.InitialDirectory = PathManager.MakeAbsolutePath(Global.Config.MoviesPath, "");
			ofd.Filter = "Movie files (*.tas)|*.TAS;*.ZIP;*.7z|FCEUX Movies|*.FM2|PCEjin Movies|*.PCE|Archive Files|*.zip;*.7z|All Files|*.*";

			Global.Sound.StopSound();
			var result = ofd.ShowDialog();
			Global.Sound.StartSound();
			if (result == DialogResult.OK)
			{
				var file = new FileInfo(ofd.FileName);
				if (!file.Exists)
					return;
				else
				{
					AddMovieToList(ofd.FileName);
				}
			}
		}

		private void AddMovieToList(string filename)
		{
			var file = new HawkFile(filename);
			if (!file.Exists)
				return;
			else
			{
				PreLoadMovieFile(file);
				MovieView.ItemCount = MovieList.Count;
				UpdateList();
				MovieView.SelectedIndices.Clear();
				MovieView.setSelection(MovieList.Count - 1);
				sortReverse = false;
				sortedCol = "";
			}
		}

		private void PreLoadMovieFile(HawkFile path)
		{
			Movie m = new Movie(path.CanonicalFullPath, MOVIEMODE.INACTIVE);
			m.PreLoadText();
			//m.LoadMovie();
			if (path.Extension.ToUpper() == ".FM2")
				m.SetHeaderLine(MovieHeader.PLATFORM, "NES");
			else if (path.Extension.ToUpper() == ".MC2")
				m.SetHeaderLine(MovieHeader.PLATFORM, "PCE");
			MovieList.Add(m);
		}

		private void UpdateList()
		{
			MovieView.Refresh();
			UpdateMovieCount();
		}

		private void UpdateMovieCount()
		{
			int x = MovieList.Count;
			if (x == 1)
				MovieCount.Text = x.ToString() + " movie";
			else
				MovieCount.Text = x.ToString() + " movies";
		}

		private void PlayMovie_Load(object sender, EventArgs e)
		{
			string d = PathManager.MakeAbsolutePath(Global.Config.MoviesPath, "");
			if (!Directory.Exists(d))
				Directory.CreateDirectory(d);
			foreach (string f in Directory.GetFiles(d, "*.tas"))
				AddMovieToList(f);
			foreach (string f in Directory.GetFiles(d, "*.fm2"))
				AddMovieToList(f);
			foreach (string f in Directory.GetFiles(d, "*.mc2"))
				AddMovieToList(f);
		}

		private void MovieView_SelectedIndexChanged(object sender, EventArgs e)
		{
			DetailsView.Items.Clear();
			int x = MovieView.SelectedIndices[0];
			Dictionary<string, string> h = MovieList[x].GetHeaderInfo();

			foreach (var kvp in h)
			{
				ListViewItem item = new ListViewItem(kvp.Key);
				item.SubItems.Add(kvp.Value);
				DetailsView.Items.Add(item);
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			//TODO: a comments viewer/editor
		}

		private void button2_Click(object sender, EventArgs e)
		{
			//TODO: a subtitle viewer/editor
		}

		private void MovieView_DoubleClick(object sender, EventArgs e)
		{
			Run();
			this.Close();
		}

		private void MovieView_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None; string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
		}

		private void MovieView_DragDrop(object sender, DragEventArgs e)
		{
			string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
			foreach (string path in filePaths)
			{
				if (Path.GetExtension(path) == ".tas" || Path.GetExtension(path) == ".fm2" ||
					Path.GetExtension(path) == ".mc2")
					AddMovieToList(path);
			}
		}

		private void MovieView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			OrderColumn(e.Column);
		}

		private void OrderColumn(int columnToOrder)
		{
			string columnName = MovieView.Columns[columnToOrder].Text;
			if (sortedCol.CompareTo(columnName) != 0)
				sortReverse = false;
			MovieList.Sort((x, y) => x.CompareTo(y, columnName) * (sortReverse ? -1 : 1));
			sortedCol = columnName;
			sortReverse = !(sortReverse);
			MovieView.Refresh();
		}

	}
}
