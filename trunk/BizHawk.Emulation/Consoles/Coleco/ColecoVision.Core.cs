﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BizHawk.Emulation.CPUs.Z80;
using BizHawk.Emulation.Sound;
using BizHawk.Emulation.Consoles.Sega;

namespace BizHawk.Emulation.Consoles.Coleco
{
	public partial class ColecoVision : IEmulator
	{
		public byte[] rom;
		public Z80A cpu;
		public VDP Vdp; //adelikat: Using the SMS one for now

		public byte ReadMemory(ushort addr)
		{
			return 0xFF;
		}

		public void WriteMemory(ushort addr, byte value)
		{
			return;
		}

		public void HardReset()
		{
			_lagcount = 0;
			cpu = new Z80A();
			cpu.ReadMemory = ReadMemory;
			cpu.WriteMemory = WriteMemory;
		}

		public void FrameAdvance(bool render)
		{
			_frame++;
			_islag = true;

			if (render == false) return;
			for (int i = 0; i < 256 * 192; i++)
				frameBuffer[i] = 0; //black

			if (_islag)
				_lagcount++;
		}
	}
}
