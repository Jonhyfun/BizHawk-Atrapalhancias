using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using BizHawk.Common.BufferExtensions;

namespace BizHawk.Common
{
	public interface ISHA1
	{
		byte[] ComputeHash(byte[] buffer);
	}

	public sealed class NETSHA1 : ISHA1
	{
		private readonly SHA1 _sha1Impl;

		public NETSHA1()
		{
			_sha1Impl = SHA1.Create();
			Debug.Assert(_sha1Impl.CanReuseTransform && _sha1Impl.HashSize is SHA1Checksum.EXPECTED_LENGTH);
		}

		public byte[] ComputeHash(byte[] buffer)
			=> _sha1Impl.ComputeHash(buffer);
	}

	public sealed class FastSHA1 : ISHA1
	{
		public unsafe byte[] ComputeHash(byte[] buffer)
		{
			// Set SHA1 start state
			var state = stackalloc uint[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476, 0xC3D2E1F0 };
			// This will use dedicated SHA instructions, which perform 4x faster than a generic implementation
			LibBizHash.BizCalcSha1((IntPtr)state, buffer, buffer.Length);
			// The copy seems wasteful, but pinning the state down actually has a bigger performance impact
			var ret = new byte[20];
			Marshal.Copy((IntPtr)state, ret, 0, 20);
			return ret;
		}
	}

	/// <summary>uses <see cref="SHA1"/> implementation from BCL</summary>
	/// <seealso cref="CRC32Checksum"/>
	/// <seealso cref="MD5Checksum"/>
	/// <seealso cref="SHA256Checksum"/>
	public static class SHA1Checksum
	{
		/// <remarks>in bits</remarks>
		internal const int EXPECTED_LENGTH = 160;

		internal const string PREFIX = "SHA1";

		public /*static readonly*/const string Dummy = "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE";

		public /*static readonly*/const string EmptyFile = "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709";

		public /*static readonly*/const string Zero = "0000000000000000000000000000000000000000";

#if NET6_0
		public static byte[] Compute(ReadOnlySpan<byte> data)
			=> SHA1.HashData(data);

		public static byte[] ComputeConcat(ReadOnlySpan<byte> dataA, ReadOnlySpan<byte> dataB)
		{
			using var impl = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
			impl.AppendData(dataA);
			impl.AppendData(dataB);
			return impl.GetHashAndReset();
		}
#else
		private static ISHA1? _sha1Impl;

		private static ISHA1 SHA1Impl
		{
			get
			{
				if (_sha1Impl == null)
				{
					_sha1Impl = LibBizHash.BizSupportsShaInstructions()
						? new FastSHA1()
						: new NETSHA1();
				}
				return _sha1Impl;
			}
		}

		public static byte[] Compute(byte[] data)
			=> SHA1Impl.ComputeHash(data);

		public static byte[] ComputeConcat(byte[] dataA, byte[] dataB)
		{
			using var impl = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
			impl.AppendData(dataA);
			impl.AppendData(dataB);
			return impl.GetHashAndReset();
		}

		public static string ComputeDigestHex(byte[] data)
			=> Compute(data).BytesToHexString();

		public static string ComputePrefixedHex(byte[] data)
			=> $"{PREFIX}:{ComputeDigestHex(data)}";

		public static byte[] Compute(ReadOnlySpan<byte> data)
			=> Compute(data.ToArray());

		public static byte[] ComputeConcat(ReadOnlySpan<byte> dataA, ReadOnlySpan<byte> dataB)
			=> ComputeConcat(dataA.ToArray(), dataB.ToArray());
#endif

		public static string ComputeDigestHex(ReadOnlySpan<byte> data)
			=> Compute(data).BytesToHexString();

		public static string ComputePrefixedHex(ReadOnlySpan<byte> data)
			=> $"{PREFIX}:{ComputeDigestHex(data)}";
	}
}
