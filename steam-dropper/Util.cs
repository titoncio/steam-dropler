﻿using System;
using System.IO;
using steam_dropper.Model;
using steam_dropper.Steam;

namespace steam_dropper
{
	public class Util
	{
		private static string DropHistoryFolder;
		public Util(string dropHistoryFolder)
		{
			DropHistoryFolder = dropHistoryFolder;
		}

		public static long GetSystemUnixTime()
		{
			return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
		}

		public static byte[] HexStringToByteArray(string hex)
		{
			int hexLen = hex.Length;
			byte[] ret = new byte[hexLen / 2];
			for (int i = 0; i < hexLen; i += 2)
			{
				ret[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}
			return ret;
		}

		public static void LogDrop(string accountName, uint game, DropResult result)
		{
			if (!Directory.Exists(DropHistoryFolder))
			{
				Directory.CreateDirectory(DropHistoryFolder);
			}

			using (StreamWriter sw = File.CreateText(Path.Combine(DropHistoryFolder, $"{accountName}.txt")))
			{
				sw.WriteLine($"Dropped! {DateTime.Now} - AppID: {game} - Item: {result.ItemDefId} ({result.ItemId})");
			}
		}
	}

	public static class APIEndpoints
	{
		public const string STEAMAPI_BASE = "https://api.steampowered.com";
		public const string COMMUNITY_BASE = "https://steamcommunity.com";
		public const string MOBILEAUTH_BASE = STEAMAPI_BASE + "/IMobileAuthService/%s/v0001";
		public static string MOBILEAUTH_GETWGTOKEN = MOBILEAUTH_BASE.Replace("%s", "GetWGToken");
		public const string TWO_FACTOR_BASE = STEAMAPI_BASE + "/ITwoFactorService/%s/v0001";
		public static string TWO_FACTOR_TIME_QUERY = TWO_FACTOR_BASE.Replace("%s", "QueryTime");
	}
}
