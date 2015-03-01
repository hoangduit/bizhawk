﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BizHawk.Common;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Common.IEmulatorExtensions;

namespace BizHawk.Client.Common
{
	public partial class TasMovie
	{
		public TasMovieChangeLog ChangeLog;

		public override void RecordFrame(int frame, IController source)
		{
			ChangeLog.AddGeneralUndo(frame, frame);

			base.RecordFrame(frame, source);

			LagLog.RemoveFrom(frame);
			LagLog[frame] = Global.Emulator.AsInputPollable().IsLagFrame;

			StateManager.Capture();

			ChangeLog.SetGeneralRedo();
		}

		public override void Truncate(int frame)
		{
			ChangeLog.AddGeneralUndo(frame, InputLogLength - 1);

			if (frame < _log.Count - 1)
			{
				Changes = true;
			}

			base.Truncate(frame);

			LagLog.RemoveFrom(frame);
			StateManager.Invalidate(frame);
			Markers.TruncateAt(frame);

			ChangeLog.SetGeneralRedo();
		}

		public override void PokeFrame(int frame, IController source)
		{
			ChangeLog.AddGeneralUndo(frame, frame);
			
			base.PokeFrame(frame, source);
			InvalidateAfter(frame);

			ChangeLog.SetGeneralRedo();
		}
		public void SetFrame(int frame, string source)
		{
			ChangeLog.AddGeneralUndo(frame, frame);

			base.SetFrameAt(frame, source);
			InvalidateAfter(frame);

			ChangeLog.SetGeneralRedo();
		}

		public override void ClearFrame(int frame)
		{
			ChangeLog.AddGeneralUndo(frame, frame);

			base.ClearFrame(frame);
			InvalidateAfter(frame);

			ChangeLog.SetGeneralRedo();
		}

		public void RemoveFrames(int[] frames)
		{
			if (frames.Any())
			{
				var invalidateAfter = frames.Min();

				ChangeLog.AddGeneralUndo(invalidateAfter, InputLogLength - 1);

				foreach (var frame in frames.OrderByDescending(x => x)) // Removin them in reverse order allows us to remove by index;
				{
					_log.RemoveAt(frame);
				}

				Changes = true;
				InvalidateAfter(invalidateAfter);

				ChangeLog.SetGeneralRedo();
			}
		}
		public void RemoveFrames(int removeStart, int removeUpTo)
		{
			ChangeLog.AddGeneralUndo(removeStart, InputLogLength - 1);

			for (int i = removeUpTo - 1; i >= removeStart; i--)
				_log.RemoveAt(i);

			Changes = true;
			InvalidateAfter(removeStart);

			ChangeLog.SetGeneralRedo();
		}

		public void InsertInput(int frame, IEnumerable<string> inputLog)
		{
			ChangeLog.AddGeneralUndo(frame, InputLogLength + inputLog.Count() - 1);

			_log.InsertRange(frame, inputLog);
			Changes = true;
			InvalidateAfter(frame);

			ChangeLog.SetGeneralRedo();
		}

		public void InsertInput(int frame, IEnumerable<IController> inputStates)
		{
			ChangeLog.AddGeneralUndo(frame, InputLogLength + inputStates.Count() - 1);

			var lg = LogGeneratorInstance();

			var inputLog = new List<string>();

			foreach (var input in inputStates)
			{
				lg.SetSource(input);
				inputLog.Add(lg.GenerateLogEntry());
			}

			InsertInput(frame, inputLog);

			ChangeLog.SetGeneralRedo();
		}

		public void CopyOverInput(int frame, IEnumerable<IController> inputStates)
		{
			ChangeLog.AddGeneralUndo(frame, frame + inputStates.Count() - 1);

			var lg = LogGeneratorInstance();
			var states = inputStates.ToList();
			for (int i = 0; i < states.Count; i++)
			{
				lg.SetSource(states[i]);
				_log[frame + i] = lg.GenerateLogEntry();
			}

			Changes = true;
			InvalidateAfter(frame);

			ChangeLog.SetGeneralRedo();
		}

		public void InsertEmptyFrame(int frame, int count = 1)
		{
			ChangeLog.AddGeneralUndo(frame, InputLogLength + count - 1);

			var lg = LogGeneratorInstance();
			lg.SetSource(Global.MovieSession.MovieControllerInstance());

			for (int i = 0; i < count; i++)
			{
				_log.Insert(frame, lg.EmptyEntry);
			}

			Changes = true;
			InvalidateAfter(frame - 1);

			ChangeLog.SetGeneralRedo();
		}

		public void ToggleBoolState(int frame, string buttonName)
		{
			if (frame < _log.Count)
			{
				var adapter = GetInputState(frame) as Bk2ControllerAdapter;
				adapter[buttonName] = !adapter.IsPressed(buttonName);

				var lg = LogGeneratorInstance();
				lg.SetSource(adapter);
				_log[frame] = lg.GenerateLogEntry();
				Changes = true;
				InvalidateAfter(frame);

				ChangeLog.AddBoolToggle(frame, buttonName, !adapter[buttonName]);
	}
		}

		public void SetBoolState(int frame, string buttonName, bool val)
		{
			if (frame < _log.Count)
			{
				var adapter = GetInputState(frame) as Bk2ControllerAdapter;
				var old = adapter[buttonName];
				adapter[buttonName] = val;

				var lg = LogGeneratorInstance();
				lg.SetSource(adapter);
				_log[frame] = lg.GenerateLogEntry();

				if (old != val)
				{
					InvalidateAfter(frame);
					Changes = true;
					ChangeLog.AddBoolToggle(frame, buttonName, old);
				}
			}
		}
		public void SetBoolStates(int frame, int count, string buttonName, bool val)
		{
			if (frame < _log.Count)
			{
				if (frame + count >= _log.Count)
					count = _log.Count - frame - 1;

				ChangeLog.AddGeneralUndo(frame, frame + count - 1);

				int changed = -1;
				for (int i = 0; i < count; i++)
				{
					var adapter = GetInputState(frame + i) as Bk2ControllerAdapter;
					bool old = adapter[buttonName];
					adapter[buttonName] = val;

					var lg = LogGeneratorInstance();
					lg.SetSource(adapter);
					_log[frame + i] = lg.GenerateLogEntry();

					if (changed == -1 && old != val)
						changed = frame + i;
				}

				if (changed != -1)
				{
					InvalidateAfter(changed);
					Changes = true;
				}

				ChangeLog.SetGeneralRedo();
			}
		}

		public void SetFloatState(int frame, string buttonName, float val)
		{
			if (frame < _log.Count)
			{
				var adapter = GetInputState(frame) as Bk2ControllerAdapter;
				var old = adapter.GetFloat(buttonName);
				adapter.SetFloat(buttonName, val);

				var lg = LogGeneratorInstance();
				lg.SetSource(adapter);
				_log[frame] = lg.GenerateLogEntry();

				if (old != val)
				{
					InvalidateAfter(frame);
					Changes = true;
					ChangeLog.AddFloatChange(frame, buttonName, old, val);
				}
			}
		}
		public void SetFloatStates(int frame, int count, string buttonName, float val)
		{
			if (frame < _log.Count)
			{
				if (frame + count >= _log.Count)
					count = _log.Count - frame - 1;

				ChangeLog.AddGeneralUndo(frame, frame + count - 1);

				int changed = -1;
				for (int i = 0; i < count; i++)
				{
					var adapter = GetInputState(frame + i) as Bk2ControllerAdapter;
					float old = adapter.GetFloat(buttonName);
					adapter.SetFloat(buttonName, val);

					var lg = LogGeneratorInstance();
					lg.SetSource(adapter);
					_log[frame + i] = lg.GenerateLogEntry();

					if (changed == -1 && old != val)
						changed = frame + i;
				}

				if (changed != -1)
				{
					InvalidateAfter(changed);
					Changes = true;
				}

				ChangeLog.SetGeneralRedo();
			}
		}

	}
}
