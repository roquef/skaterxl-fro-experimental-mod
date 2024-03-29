﻿using HarmonyLib;
using ModIO.UI;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
	class MultiplayerManagerPatch
	{
		[HarmonyPatch(typeof(MultiplayerManager), nameof(MultiplayerManager.OnPlayerLeftRoom))]
        static void OnPlayerLeftRoom(Player otherPlayer)
		{
			if (otherPlayer.IsLocal)
			{
				return;
			}

			MessageSystem.QueueMessage(MessageDisplayData.Type.Info, string.Format("{0} left the Room", otherPlayer.NickName), MultiplayerManager.PopupDuration);
			Main.multi.OnPlayerLeft(otherPlayer);
		}
	}
}