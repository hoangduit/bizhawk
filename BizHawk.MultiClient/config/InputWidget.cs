﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BizHawk.MultiClient
{
	public class InputWidget : TextBox
	{
		public InputWidget()
		{
		}

		public List<IBinding> Bindings = new List<IBinding>();

		void UpdateLabel()
		{
			if (Bindings.Count == 0)
			{
				Text = "";
			}
			else
			{
				Text = Bindings[0].ToString();
			}
			Update();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			e.Handled = true;
			if (e.KeyCode == Keys.ControlKey) return;
			if (e.KeyCode == Keys.ShiftKey) return;
			if (e.KeyCode == Keys.Menu) return;
			KeyboardBinding kb = new KeyboardBinding();
			kb.key = e.KeyCode;
			kb.modifiers = e.Modifiers;
			Bindings.Clear();
			Bindings.Add(kb);
			UpdateLabel();
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			e.Handled = true;
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			BackColor = Color.Pink;
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			BackColor = SystemColors.Window;
		}

	}

	public class KeyboardBinding : IBinding
	{
		public override string ToString()
		{
			string str = "";
			if((modifiers & Keys.Shift)!=0)
				str += "SHIFT+";
			if ((modifiers & Keys.Control) != 0)
				str += "CTRL+";
			if ((modifiers & Keys.Alt) != 0)
				str += "ALT+";
			str += key.ToString();
			return str;
		}
		public Keys key;
		public Keys modifiers;
	}

	public interface IBinding
	{

	}
}