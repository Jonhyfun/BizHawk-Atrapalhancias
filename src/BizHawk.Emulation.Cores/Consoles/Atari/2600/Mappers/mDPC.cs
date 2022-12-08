﻿using System.Linq;
using BizHawk.Common;

namespace BizHawk.Emulation.Cores.Atari.Atari2600
{
	/*
	DPC (Pitfall 2)
	-----

	Back in the day, this game was da shizzle (and IMO still is).  It did its trick via
	a custom chip in the cartridge.  Fortunately for us, there's a patent that describes
	lots of the internal workings of the chip (number 4644495, "video memory system").

	Interestingly, the patent shows the DPC as a *separate* device.  You plug a 
	pass-through cartridge into your 2600, then plug the game cartridge into the
	pass-through.  Apparently, Activision thought that people wouldn't like this, or
	there was some other reasoning behind it and they ditched that idea and went with
	the DPC inside the cartridge.

	Unfortunately for ActiVision, it was filed in January of 1984, during the height of
	the crash.  The inventor is listed as David Crane.

	OK, enough background.  Now onto the meat:

	The DPC chip is just 24 pins, and needs to pass through the chip enable to the
	game ROM on the cartridge, so it can only address 2K of memory.  This means the
	DPC shows up twice in the address space, once at 1000-107F and again at 1800-18FF.

	There's been some discussion about the pitch of the music generated by this chip,
	and how different carts will play the music at different pitches.  Turns out, on the
	cart, the frequency is determined by a resistor (560K ohms) and a capacitor integrated
	onto the die of the DPC chip itself.  The resistor is a 5% tolerance part, and the
	process variations of the DPC itself will control the frequency of the music produced
	by it.

	If you touch the resistor on the cartridge board, the music pitch will drastically 
	change, almost like you were playing it on a theremin!  Lowering the resistance makes
	the music pitch increase, increasing the resistance makes the pitch lower.

	It's extremely high impedance so body effects of you touching the pin makes it 
	vary wildly.

	Thus, I say there's really no "one true" pitch for the music.  The patent, however,
	says that the frequency of this oscillator is 42KHz in the "preferred embodiment".
	The patent says that it can range from 15KHz to 80KHz depending on the application
	and the particular design of the sound generator.  I chose 21KHz (half their preferred
	value) and it sounds fairly close to my actual cartridge.  

	Address map:

	Read Only:
	1000-1003	: random number generator
	1004-1005	: sound value (and MOVAMT value ANDed with draw line carry, with draw line add)
	1006-1007	: sound value (and MOVAMT value ANDed with draw line carry, no draw line add)
	1008-100F	: returned data value for fetcher 0-7
	1010-1017	: returned data value for fetcher 0-7, masked 
	1018-101F	: returned data value for fetcher 0-7, nybble swapped, masked 
	1020-1027	: returned data value for fetcher 0-7, byte reversed, masked
	1028-102F	: returned data value for fetcher 0-7, rotated right one bit, masked
	1030-1037	: returned data value for fetcher 0-7, rotated left one bit, masked
	1038-103F	: fetcher 0-7 mask

	Write Only:
	1040-1047	: fetcher 0-7 start count
	1048-104F	: fetcher 0-7 end count
	1050-1057	: fetcher 0-7 pointer low
	1058-105B	: fetcher 0-3 pointer high
	105C		: fetcher 4 pointer high and draw line enable
	105D-105F	: fetcher 5-7 pointer high and music enable
	1060-1067	: draw line movement value (MOVAMT)
	1068-106F	: not used
	1070-1077	: random number generator reset
	1078-107F	: not used

	random number generator
	-----------------------

	The random number generator is used on Pitfall 2 to make the eel flash between white and
	black, and nothing else.  Failure to emulate this will result in the eel not flashing.

	It's an 8 bit LFSR which can be reset to the all 0's condition by accessing 1070-1077.
	Unlike a regular LFSR, this one uses three XOR gates and an inverter, so the illegal 
	condition is the all 1's condition.  

	There's 255 states and the following code emulates it:

	LFSR = ((LFSR << 1) | (~(((LFSR >> 7) ^ (LFSR >> 5)) ^ ((LFSR >> 4) ^ (LFSR >> 3))) & 1)) & 0xff;

	Bits 3, 4, 5, and 7 are XOR'd together and inverted and fed back into bit 0 each time the
	LFSR is clocked.

	The LFSR is clocked each time it is read.  It wraps after it is read 255 times. (The
	256th read returns the same value as the 1st).

	data fetchers
	-------------

	Internal to the DPC is a 2K ROM containing the graphics and a few other bits and pieces
	(playfield values I think) of data that can be read via the auto-incrementing data
	fetchers.

	Each set of 8 addresses (1008-100F for example) return the data from one of the 8
	data fetcher pointers, returning the data in a slightly different format for each.
	The format for the 6 possible register ranges is as follows:

	For the byte "ABCDEFGH" (bit 7 to bit 0) it is returned:

	1008-100F: ABCDEFGH (never masked)
	1010-1017: ABCDEFGH
	1018-101F: EFGHABCD (nybble swap)
	1020-1027: HGFEDCBA (bit reversed)
	1028-102F: 0ABCDEFG (shifted right)
	1030-1037: BCDEFGH0 (shifted left)

	Reading from each set of locations above returns the byte of data from the DPC's
	internal ROM.  Reading from 1008 accesses data at DF (data fetcher) 0's pointer,
	then decrements the pointer.  Reading from 1009 accesses data at DF1, and so on.

	There is no difference except how the data is returned when reading from 1008,
	1010, 1018, 1020, etc.  All of them return data pointed to by DF0's pointer.  Only
	the order of the bits returned changes.

	I am not sure what purpose returning the data shifted left or right 1 bit serves,
	and it was not used on Pitfall 2, but that's what it does.  I guess you could
	use it to make a sprite appear to "wiggle" left and right a bit, if it were 6 pixels
	wide.

	All of these read ports returns the data masked by an enable signal, except for
	1008-100F.  The data here is never masked. (more about this in a minute)

	To read data out of the chip, first you program in its start address into the
	pointer registers.  These are at 1050-1057 for the lower 8 bits of the pointer
	value, and 1058-105F for the upper 4 bits of the pointer value.  This forms the
	12 bit address which can then be used to index the DPC's ROM.

	A few of the upper bits on 105C-105F are used for a few other purposes, which will be
	described later. 

	Masking the data:
	-----------------

	1038-103F is the read back for the mask value
	1040-1047 is the start count
	1048-104F is the end count


	The mask value can be read via 1038-103F.  It returns 0 when graphics are masked, and
	FFh when they are not masked.  (0 = reset, 1 = set)

	The basic synopsis is thus:

	When the lower 8 bits of the pointer equals the start count, the mask register is set.
	When the lower 8 bits of the pointer equals the end count, the mask register is reset.
	Writing to the start count register also sets the register.

	This allows one to have the sprites only show up on specific scanlines, by programming
	the proper start and end counts, and the proper starting value into the pointer.  This
	way, the sprite can be drawn from top to bottom of the screen, and have it only appear
	where it is desired without having to do anything else in the 2600 code.

	Making Music:
	-------------

	The music is generated by repurposing three of the fetchers, the last three. 
	Each fetcher can be individually selected for music or fetching.

				7       0
				---------
	105D-105F:  xxSM PPPP

	S: Select clock input to fetching counter. 0 = read pulse when the proper returned
	data register is read (i.e. for fetcher 5, 1015 is being read) 1 = music oscillator.

	M: Music mode.  1 = enable music mode, 0 = disable music mode.

	P: upper 4 bits of the 12 bit data fetcher pointer.


	I am not sure why you can separately select the clock source and the music mode,
	but you can.  Maybe they had some plans for externally clocking the chip via some
	logic to bump the pointers.

	Normally you set both the M and P bits to make music.  

	When in music mode, the lower 8 bits of the fetcher pointer is used as an 8 bit down
	counter.  Each time the lower 8 bits equals FFh, it is reloaded from the start count
	register.

	To turn the data fetcher into a square wave generator takes very little hardware. The
	start/end count registers are used as-is to toggle the flag register.

	This means that the duty cycle of the square waves produced can be varied by adjusting
	the end count register relative to the start count register.  I suspect the game simply
	right shifts the start count by one and stuffs it into the end count to produce a 
	50% duty cycle waveform.

	The three flag outputs for fetchers 5 to 7 are fed into a cool little circuit composed
	of a 3 to 8 decoder and four 4 input NAND gates to produce the 4 bit audio output.

	The output is as follows:

	 fetcher   result
	   567      
	---------------------
	   000     0h
	   001     4h
	   010     5h
	   011     9h
	   100     6h
	   101     Ah
	   110     Bh
	   111     Fh


	This is a somewhat nonlinear mixing of the three channels, so the apparent volume of them
	is different relative to each other.

	The final 4 bit output value from the above table is then available to read at address
	1004-1007, in bits 0 to 3.

	Pitfall 2 just reads this location and stuffs it into the audio register every scanline or
	so.  The value read at 1004-1007 is the instantanious value generated by the fetchers and
	mixing hardware.
	 */
	internal sealed class mDPC : MapperBase
	{
		// Table for computing the input bit of the random number generator's
		// shift register (it's the NOT of the EOR of four bits)
		private readonly byte[] _randomInputBits = { 1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1 };

		private int[] _counters = new int[8];
		private byte[] _tops = new byte[8];
		private byte[] _flags = new byte[8];
		private byte[] _bottoms = new byte[8];
		private bool[] _musicModes = new bool[3];

		private int _bank4K;
		private byte _currentRandomVal;
		private int _elapsedCycles = 85; // 85 compensates for a slight timing issue when ClockCpu is first run, 85 puts BizHawk back on track with Stella on elapsed timing values
		private float _fractionalClocks; // Fractional DPC music OSC clocks unused during the last update

		private byte[] _dspData;

		public mDPC(Atari2600 core) : base(core)
		{
		}

		public byte[] DspData => _dspData ??= Core.Rom.Skip(8192).Take(2048).ToArray();

		public override void SyncState(Serializer ser)
		{
			base.SyncState(ser);

			ser.Sync("counters", ref _counters, false);
			ser.Sync("tops", ref _tops, false);
			ser.Sync("flags", ref _flags, false);
			ser.Sync("bottoms", ref _bottoms, false);
			ser.Sync("musicMode0", ref _musicModes[0]); // Silly, but I didn't want to support bool[] in Serializer just for this one variable
			ser.Sync("musicMode1", ref _musicModes[1]);
			ser.Sync("musicMode2", ref _musicModes[2]);

			ser.Sync("bank_4k", ref _bank4K);
			ser.Sync("currentRandomVal", ref _currentRandomVal);
			ser.Sync("elapsedCycles", ref _elapsedCycles);
			ser.Sync("fractionalClocks", ref _fractionalClocks);
		}

		public override void HardReset()
		{
			_counters = new int[8];
			_tops = new byte[8];
			_flags = new byte[8];
			_bottoms = new byte[8];
			_musicModes = new bool[3];
			_bank4K = 0;
			_currentRandomVal = 0;
			_elapsedCycles = 85;
			_fractionalClocks = 0;
		}

		public override void ClockCpu()
		{
			_elapsedCycles++;
		}

		public override byte ReadMemory(ushort addr) => ReadMem(addr, false);

		public override byte PeekMemory(ushort addr) => ReadMem(addr, true);

		public override void WriteMemory(ushort addr, byte value)
			=> WriteMem(addr, value, false);

		public override void PokeMemory(ushort addr, byte value)
			=> WriteMem(addr, value, true);

		private byte ReadMem(ushort addr, bool peek)
		{
			if (addr < 0x1000)
			{
				return base.ReadMemory(addr);
			}

			if (!peek)
			{
				Address(addr);
				ClockRandomNumberGenerator();
			}

			if (addr < 0x1040)
			{
				byte result;

				// Get the index of the data fetcher that's being accessed
				var index = addr & 0x07;
				var function = (addr >> 3) & 0x07;

				// Update flag register for selected data fetcher
				if ((_counters[index] & 0x00ff) == _tops[index])
				{
					_flags[index] = 0xff;
				}
				else if ((_counters[index] & 0x00ff) == _bottoms[index])
				{
					_flags[index] = 0x00;
				}

				switch (function)
				{
					case 0x00:
						if (index < 4)
						{
							result = _currentRandomVal;
						}
						else // No, it's a music read
						{
							var musicAmplitudes = new byte[] {
							  0x00, 0x04, 0x05, 0x09, 0x06, 0x0a, 0x0b, 0x0f
							};

							// Update the music data fetchers (counter & flag)
							UpdateMusicModeDataFetchers();

							byte i = 0;
							if (_musicModes[0] && _flags[5] > 0)
							{
								i |= 0x01;
							}

							if (_musicModes[1] && _flags[6] > 0)
							{
								i |= 0x02;
							}

							if (_musicModes[2] && _flags[7] > 0)
							{
								i |= 0x04;
							}

							result = musicAmplitudes[i];
						}

						break;

					// DFx display data read
					case 0x01:
						result = DspData[2047 - _counters[index]];
						break;

					// DFx display data read AND'd w/flag
					case 0x02:
						result = (byte)(DspData[2047 - _counters[index]] & _flags[index]);
						break;

					// DFx flag
					case 0x07:
						result = _flags[index];
						break;

					default:
						result = 0;
						break;
				}

				// Clock the selected data fetcher's counter if needed
				if ((index < 5) || ((index >= 5) && (!_musicModes[index - 5])))
				{
					_counters[index] = (_counters[index] - 1) & 0x07ff;
				}

				return result;
			}

			return Core.Rom[(_bank4K << 12) + (addr & 0xFFF)];
		}

		private void WriteMem(ushort addr, byte value, bool poke)
		{
			if (addr < 0x1000)
			{
				base.WriteMemory(addr, value);
				return;
			}

			if (poke)
			{
				return;
			}

			if (addr >= 0x1040 && addr < 0x1080)
			{
				var index = addr & 0x07;
				var function = (addr >> 3) & 0x07;

				switch (function)
				{
					// DFx top count
					case 0x00:
						_tops[index] = value;
						_flags[index] = 0x00;
						break;

					// DFx bottom count
					case 0x01:
						_bottoms[index] = value;
						break;

					// DFx counter low
					case 0x02:
						if ((index >= 5) && _musicModes[index - 5])
						{
							// Data fetcher is in music mode so its low counter value
							// should be loaded from the top register not the poked value
							_counters[index] = (_counters[index] & 0x0700) |
								_tops[index];
						}
						else
						{
							// Data fetcher is either not a music mode data fetcher or it
							// isn't in music mode so it's low counter value should be loaded
							// with the poked value
							_counters[index] = (_counters[index] & 0x0700) | value;
						}

						break;

					// DFx counter high
					case 0x03:
						_counters[index] = (ushort)(((value & 0x07) << 8) |
							(_counters[index] & 0x00ff));

						// Execute special code for music mode data fetchers
						if (index >= 5)
						{
							_musicModes[index - 5] = (value & 0x10) > 0;

							// NOTE: We are not handling the clock source input for
							// the music mode data fetchers.  We're going to assume
							// they always use the OSC input.
						}

						break;

					// Random Number Generator Reset
					case 0x06:
						_currentRandomVal = 1;
						break;
				}
			}
		}

		private void Address(ushort addr)
		{
			if (addr == 0x1FF8)
			{
				_bank4K = 0;
			}
			else if (addr == 0x1FF9)
			{
				_bank4K = 1;
			}
		}

		private void ClockRandomNumberGenerator()
		{
			// Using bits 7, 5, 4, & 3 of the shift register compute the input
			// bit for the shift register
			var bit = _randomInputBits[((_currentRandomVal >> 3) & 0x07)
				| ((_currentRandomVal & 0x80) > 0 ? 0x08 : 0x00)];

			// Update the shift register 
			_currentRandomVal = (byte)((_currentRandomVal << 1) | bit);
		}

		private void UpdateMusicModeDataFetchers()
		{
			// Calculate the number of cycles since the last update
			var cycles = _elapsedCycles;
			_elapsedCycles = 0;

			// Calculate the number of DPC OSC clocks since the last update
			var clocks = ((20000.0 * cycles) / 1193191.66666667) + _fractionalClocks;
			var wholeClocks = (int)clocks;
			_fractionalClocks = (float)(clocks - wholeClocks);

			if (wholeClocks <= 0)
			{
				return;
			}

			// Let's update counters and flags of the music mode data fetchers
			for (var x = 5; x <= 7; ++x)
			{
				// Update only if the data fetcher is in music mode
				if (_musicModes[x - 5])
				{
					var top = _tops[x] + 1;
					var newLow = _counters[x] & 0x00ff;

					if (_tops[x] != 0)
					{
						newLow -= wholeClocks % top;
						if (newLow < 0)
						{
							newLow += top;
						}
					}
					else
					{
						newLow = 0;
					}

					// Update flag register for this data fetcher
					if (newLow <= _bottoms[x])
					{
						_flags[x] = 0x00;
					}
					else if (newLow <= _tops[x])
					{
						_flags[x] = 0xff;
					}

					_counters[x] = (_counters[x] & 0x0700) | (ushort)newLow;
				}
			}
		}
	}
}
