﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.InputSystem;
using ReservedItemSlotCore.Input;
using ReservedItemSlotCore.Networking;
using ReservedItemSlotCore.Data;
using ReservedItemSlotCore.Config;
using UnityEngine;


namespace ReservedItemSlotCore.Patches
{
    [HarmonyPatch]
    internal static class MouseScrollPatcher
    {
        public static PlayerControllerB localPlayerController { get { return StartOfRound.Instance?.localPlayerController; } }

        private static bool scrollingItemSlots = false;
        private static float timeLoggedPreventedScroll = 0;


        [HarmonyPatch(typeof(PlayerControllerB), "NextItemSlot")]
        [HarmonyPrefix]
        public static void CorrectReservedScrollDirectionNextItemSlot(ref bool forward)
        {
            if (Keybinds.scrollingReservedHotbar)
                forward = Keybinds.RawScrollAction.ReadValue<float>() > 0;
        }


        [HarmonyPatch(typeof(PlayerControllerB), "SwitchItemSlotsServerRpc")]
        [HarmonyPrefix]
        public static void CorrectReservedScrollDirectionServerRpc(ref bool forward)
        {
            if (Keybinds.scrollingReservedHotbar)
                forward = Keybinds.RawScrollAction.ReadValue<float>() > 0;
        }


        [HarmonyPatch(typeof(PlayerControllerB), "ScrollMouse_performed")]
        [HarmonyPrefix]
        public static bool PreventInvertedScrollingReservedHotbar(InputAction.CallbackContext context)
        {
            if (StartOfRound.Instance.localPlayerUsingController || SessionManager.numReservedItemSlotsUnlocked <= 0 || HUDPatcher.reservedItemSlots == null || localPlayerController.inTerminalMenu)
                return true;

            if (ReservedPlayerData.localPlayerData.currentItemSlotIsReserved)
            {
                if (!HUDPatcher.hasReservedItemSlotsAndEnabled)
                    return true;

                float time = Time.time;
                if (!Keybinds.scrollingReservedHotbar)
                    return false;
                if (ReservedPlayerData.localPlayerData.GetNumHeldReservedItems() == 1 && ReservedPlayerData.localPlayerData.currentlySelectedItem != null && !ReservedHotbarManager.isToggledInReservedSlots)
                {
                    if (ConfigSettings.verboseLogs.Value && time - timeLoggedPreventedScroll > 1)
                        timeLoggedPreventedScroll = time;
                    return false;
                }
            }
            return true;
        }


        [HarmonyPatch(typeof(PlayerControllerB), "ScrollMouse_performed")]
        [HarmonyPostfix]
        public static void ScrollReservedItemSlots(InputAction.CallbackContext context)
        {
            scrollingItemSlots = false;
        }
    }
}
