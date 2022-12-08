﻿using System;
using System.Collections.Generic;
using BizHawk.Emulation.Common;

namespace BizHawk.Emulation.Cores.Computers.SinclairSpectrum
{
	/// <summary>
	/// ZXHawk: Core Class
	/// * IDebuggable *
	/// </summary>
	public partial class ZXSpectrum : IDebuggable
	{
		public IDictionary<string, RegisterValue> GetCpuFlagsAndRegisters() => _cpu.GetCpuFlagsAndRegisters();

		public void SetCpuRegister(string register, int value) => _cpu.SetCpuRegister(register, value);

		public IMemoryCallbackSystem MemoryCallbacks { get; } = new MemoryCallbackSystem(new[] { "System Bus" });

		public bool CanStep(StepType type) => false;

		[FeatureNotImplemented]
		public void Step(StepType type) => throw new NotImplementedException();

		public long TotalExecutedCycles => _cpu.TotalExecutedCycles;
	}
}
