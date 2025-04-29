using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ProtelScannerServerSide.Helpers;

public static class EncryptionHelper
{
	private static string _pstr = "s@rvic@sH!t";

	private static byte[] _saltstr = new byte[13]
	{
		181, 241, 0, 97, 109, 47, 101, 100, 112, 101,
		100, 118, 3
	};

	public static string LastVersion { get; } = "1.0.22.0";

	public static string Encrypt(string clearText, string encryptKey = "")
	{
		string EncryptionKey = _pstr;
		if (!string.IsNullOrWhiteSpace(encryptKey))
		{
			EncryptionKey = encryptKey;
		}
		byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
		using Aes encryptor = Aes.Create();
		Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, _saltstr);
		encryptor.Key = pdb.GetBytes(32);
		encryptor.IV = pdb.GetBytes(16);
		using MemoryStream ms = new MemoryStream();
		using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
		{
			cs.Write(clearBytes, 0, clearBytes.Length);
			cs.Close();
		}
		clearText = Convert.ToBase64String(ms.ToArray());
		return clearText;
	}

	public static string Decrypt(string cipherText)
	{
		try
		{
			string EncryptionKey = _pstr;
			byte[] cipherBytes = Convert.FromBase64String(cipherText.Replace('\0', '0'));
			using (Aes encryptor = Aes.Create())
			{
				Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, _saltstr);
				encryptor.Key = pdb.GetBytes(32);
				encryptor.IV = pdb.GetBytes(16);
				using MemoryStream ms = new MemoryStream();
				using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
				{
					cs.Write(cipherBytes, 0, cipherBytes.Length);
					cs.Close();
				}
				cipherText = Encoding.Unicode.GetString(ms.ToArray());
			}
			return cipherText;
		}
		catch
		{
			return cipherText;
		}
	}
}
