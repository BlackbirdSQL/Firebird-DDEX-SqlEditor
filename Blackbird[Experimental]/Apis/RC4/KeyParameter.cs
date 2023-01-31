using System;
using BlackbirdSql.Common;
using Org.BouncyCastle.Crypto;

namespace Org.BouncyCastle.Crypto.Parameters
{
	internal class KeyParameter
		: ICipherParameters
	{
		private readonly byte[] key;

		public KeyParameter(
			byte[] key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			this.key = (byte[])key.Clone();
		}

		public KeyParameter(
			byte[] key,
			int keyOff,
			int keyLen)
		{
			ArgumentOutOfRangeException ex;

			if (key == null)
			{
				ArgumentNullException exNull = new ArgumentNullException("key");
				Diag.Dug(exNull);
				throw exNull;
			}
			if (keyOff < 0 || keyOff > key.Length)
			{
				ex = new ArgumentOutOfRangeException("keyOff");
				Diag.Dug(ex);
				throw ex;
			}
			if (keyLen < 0 || keyLen > (key.Length - keyOff))
			{
				ex = new ArgumentOutOfRangeException("keyLen");
				Diag.Dug(ex);
				throw ex;
			}

			this.key = new byte[keyLen];
			Array.Copy(key, keyOff, this.key, 0, keyLen);
		}

		public byte[] GetKey()
		{
			return (byte[])key.Clone();
		}
	}

}
