﻿using System.IO;

namespace REEngine_GUI_Investigator
{
    class MurMurHash3
	{
		//Change to suit your needs
		const uint seed = 0xFFFFFFFF;

		public static uint Hash(string str, bool UTF8)
		{
			byte[] res;
			if (UTF8)
			{
				res = new byte[str.Length + 1];
				for (int i = 0; i < str.Length; i++)
				{
					res[i] = (byte)str[i];
				}
				res[str.Length] = 0;
			}
			else
			{
				res = new byte[(str.Length + 0) * 2];
				for (int i = 0; i < str.Length; i++)
				{
					res[i * 2] = (byte)str[i];
					res[i * 2 + 1] = 0;
				}
			}
			//res[res.Length - 1] = 0;
			//res[res.Length - 2] = 0;
			using (MemoryStream ms = new MemoryStream(res))
				return Hash(ms);
		}

		public static uint Hash(Stream stream)
		{
			const uint c1 = 0xcc9e2d51;
			const uint c2 = 0x1b873593;

			uint h1 = seed;
			uint k1 = 0;
			uint streamLength = 0;

			using (BinaryReader reader = new BinaryReader(stream))
			{
				byte[] chunk = reader.ReadBytes(4);
				while (chunk.Length > 0)
				{
					streamLength += (uint)chunk.Length;
					switch (chunk.Length)
					{
						case 4:
							/* Get four bytes from the input into an uint */
							k1 = (uint)
							   (chunk[0]
							  | chunk[1] << 8
							  | chunk[2] << 16
							  | chunk[3] << 24);

							/* bitmagic hash */
							k1 *= c1;
							k1 = rotl32(k1, 15);
							k1 *= c2;

							h1 ^= k1;
							h1 = rotl32(h1, 13);
							h1 = h1 * 5 + 0xe6546b64;
							break;
						case 3:
							k1 = (uint)
							   (chunk[0]
							  | chunk[1] << 8
							  | chunk[2] << 16);
							k1 *= c1;
							k1 = rotl32(k1, 15);
							k1 *= c2;
							h1 ^= k1;
							break;
						case 2:
							k1 = (uint)
							   (chunk[0]
							  | chunk[1] << 8);
							k1 *= c1;
							k1 = rotl32(k1, 15);
							k1 *= c2;
							h1 ^= k1;
							break;
						case 1:
							k1 = (uint)(chunk[0]);
							k1 *= c1;
							k1 = rotl32(k1, 15);
							k1 *= c2;
							h1 ^= k1;
							break;

					}
					chunk = reader.ReadBytes(4);
				}
			}

			// finalization, magic chants to wrap it all up
			h1 ^= streamLength;
			h1 = fmix(h1);

			unchecked //ignore overflow
			{
				return h1;
			}
		}

		private static uint rotl32(uint x, byte r)
		{
			return (x << r) | (x >> (32 - r));
		}

		private static uint fmix(uint h)
		{
			h ^= h >> 16;
			h *= 0x85ebca6b;
			h ^= h >> 13;
			h *= 0xc2b2ae35;
			h ^= h >> 16;
			return h;
		}
	}
}
