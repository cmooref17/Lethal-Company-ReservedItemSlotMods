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


namespace ReservedItemSlotCore.Patches
{
    [HarmonyPatch]
    internal static class MouseScrollPatcher
    {
        public static PlayerControllerB localPlayerController { get { return StartOfRound.Instance?.localPlayerController; } }

        private static bool scrollingItemSlots = false;


        [HarmonyPatch(typeof(PlayerControllerB), "NextItemSlot")]
        [HarmonyPrefix]
        public static void CorrectReservedScrollDirectionNextItemSlot(ref bool forward)
        {
            if (/*!ConfigSettings.allowScrollingBetweenHotbars.Value && */Keybinds.scrollingReservedHotbar)
                forward = Keybinds.RawScrollAction.ReadValue<float>() > 0;
        }


        [HarmonyPatch(typeof(PlayerControllerB), "SwitchItemSlotsServerRpc")]
        [HarmonyPrefix]
        public static void CorrectReservedScrollDirectionServerRpc(ref bool forward)
        {
            if (/*!ConfigSettings.allowScrollingBetweenHotbars.Value && */Keybinds.scrollingReservedHotbar)
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
                //if (HUDPatcher.reservedItemSlots.Count > 0 && HUDPatcher.reservedItemSlots[1].rectTransform.anchoredPosition.y - HUDPatcher.reservedItemSlots[0].rectTransform.anchoredPosition.y <= 5) return true;
                if (!Keybinds.scrollingReservedHotbar || (ReservedPlayerData.localPlayerData.GetNumHeldReservedItems() == 1 && ReservedPlayerData.localPlayerData.currentlySelectedItem != null && !ReservedHotbarManager.isToggledInReservedSlots))
                    return false;
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
