﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Terraria.ModLoader
{
	public class BiomeLoader : Loader<ModBiome>
	{
		public const int VanillaPrimaryBiomeCount = 11;

		public BiomeLoader() => Initialize(VanillaPrimaryBiomeCount);

		// Internal boilerplate

		internal void SetupPlayer(Player player) {
			player.modBiomeFlags = new BitArray(list.Count);
		}

		public void UpdateBiomes(Player player) {
			for (int i = 0; i < player.modBiomeFlags.Length; i++) {
				bool prev = player.modBiomeFlags[i];
				bool value = player.modBiomeFlags[i] = list[i].IsBiomeActive(player);

				if (!prev && value)
					list[i].OnEnter(player);
				else if (!value && prev)
					list[i].OnLeave(player);

				if (value)
					list[i].OnInBiome(player);
			}
		}

		public static bool CustomBiomesMatch(Player player, Player other) {
			for (int i = 0; i < player.modBiomeFlags.Length; i++) {
				if (player.modBiomeFlags[i] != other.modBiomeFlags[i])
					return false;
			}
			return true;
		}

		public static void CopyCustomBiomesTo(Player player, Player other) {
			other.modBiomeFlags = (BitArray)player.modBiomeFlags.Clone();
		}

		public static void SendCustomBiomes(Player player, BinaryWriter writer) {
			Utils.SendBitArray(player.modBiomeFlags, writer);
		}

		public static void ReceiveCustomBiomes(Player player, BinaryReader reader) {
			player.modBiomeFlags = Utils.ReceiveBitArray(player.modBiomeFlags.Length, reader);
		}

		// Hooks
		public int GetPrimaryModBiome(Player player, out SceneEffectPriority priority) {
			int index = 0; float weight = 0;
			priority = SceneEffectPriority.None;

			for (int i = 0; i < list.Count; i++) {
				bool active = player.modBiomeFlags[i] && list[i].IsPrimaryBiome;
				float tst = list[i].GetCorrWeight(player);
				if (active && tst > weight) {
					index = i + VanillaCount;
					priority = list[i].Priority;
					weight = tst;
				}
			}

			return index;
		}
	}
}
