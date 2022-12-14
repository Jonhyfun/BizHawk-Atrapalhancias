/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////////
// Copied for Paint.NET PCX Plugin
// Copyright (C) Joshua Bell
/////////////////////////////////////////////////////////////////////////////////

using System.Drawing;

namespace PcxFileTypePlugin.Quantize
{
	public sealed class PaletteTable
	{
		private readonly Color[] _palette;

		public Color this[int index]
		{
			get => _palette[index];
			set => _palette[index] = value;
		}

		private int GetDistanceSquared(Color a, Color b)
		{
			int dsq = 0; // delta squared

			var v = a.B - b.B;
			dsq += v * v;
			v = a.G - b.G;
			dsq += v * v;
			v = a.R - b.R;
			dsq += v * v;

			return dsq;
		}

		public int FindClosestPaletteIndex(Color pixel)
		{
			int dsqBest = int.MaxValue;
			int ret = 0;

			for (int i = 0; i < _palette.Length; ++i)
			{
				int dsq = GetDistanceSquared(_palette[i], pixel);

				if (dsq < dsqBest)
				{
					dsqBest = dsq;
					ret = i;
				}
			}

			return ret;
		}

		public PaletteTable(Color[] palette)
		{
			_palette = (Color[])palette.Clone();
		}
	}
}
