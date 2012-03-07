using System;
using System.IO;
using System.Diagnostics;

namespace BizHawk.Emulation.Consoles.Nintendo
{
	//mapper 011

	//Crystal Mines
	//Metal Fighter

	public class IC_74x377 : NES.NESBoardBase
	{
		//configuration
		int prg_bank_mask_32k, chr_bank_mask_8k;
		bool bus_conflict = true;

		//state
		int prg_bank_32k, chr_bank_8k;

		public override bool Configure(NES.EDetectionOrigin origin)
		{
			switch (Cart.board_type)
			{
				case "Discrete_74x377-FLEX":
					break;
				case "COLORDREAMS-74*377":
					AssertPrg(32,64,128); AssertChr(16,32,64,128); AssertVram(0); AssertWram(0);
					break;

				default:
					return false;
			}

			prg_bank_mask_32k = Cart.prg_size / 32 - 1;
			chr_bank_mask_8k = Cart.chr_size / 8 - 1;

			return true;
		}
		public override byte ReadPRG(int addr)
		{
			return ROM[addr + (prg_bank_32k << 15)];
		}

		public override byte ReadPPU(int addr)
		{
			if (addr < 0x2000)
			{
				return VROM[addr + (chr_bank_8k << 13)];
			}
			else return base.ReadPPU(addr);
		}

		public override void WritePRG(int addr, byte value)
		{
			if (bus_conflict)
			{
				byte old_value = value;
				value &= ReadPRG(addr);
				//Bible Adventures (Unl) (V1.3) [o1].nes will exercise this bus conflict, but not really test it. (works without bus conflict emulation
				Debug.Assert(old_value == value, "Found a test case of Discrete_74x377 bus conflict. please report.");
			}

			prg_bank_32k = (value & 3) & prg_bank_mask_32k;
			chr_bank_8k = ((value >> 4) & 0xF) & chr_bank_mask_8k;
		}

		public override void SyncState(Serializer ser)
		{
			base.SyncState(ser);
			ser.Sync("prg_bank_32k", ref prg_bank_32k);
			ser.Sync("chr_bank_8k", ref chr_bank_8k);
		}

	}
}