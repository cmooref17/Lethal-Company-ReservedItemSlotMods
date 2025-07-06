using GameNetcodeStuff;
using HarmonyLib;
//using LCVR.Player;
using LCVR.Managers;
using ReservedItemSlotCore.Data;
using ReservedItemSlotCore.Input;
using ReservedItemSlotCore.Patches;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ReservedItemSlotCore.Compatibility
{
    [HarmonyPatch]
    internal static class LCVR_Compat
    {
        internal static bool Loaded { get { return Plugin.IsModLoaded("io.daxcess.lcvr"); } }
        internal static bool VRModeEnabled { get { return VRSession.InVR; } }
        public static bool LoadedAndEnabled { get { return Loaded && VRModeEnabled; } }

        internal static bool vrPlayerScrollingBetweenHotbars = false;
        internal static int vrPlayerNextItemSlot = -1;

        private static PlayerControllerB localPlayerController { get { return StartOfRound.Instance?.localPlayerController; } }


        [HarmonyPatch(typeof(PlayerControllerB), "ScrollMouse_performed")]
        [HarmonyPrefix]
        public static bool OnVRPlayerScrollPre(InputAction.CallbackContext context, PlayerControllerB __instance)
        {
            if (vrPlayerScrollingBetweenHotbars)
                return false;

            if (!LCVR_Compat.LoadedAndEnabled || __instance != localPlayerController)
                return true;

            if (PlayerPatcher.reservedHotbarSize <= 0 || !HUDPatcher.hasReservedItemSlotsAndEnabled || !ReservedPlayerData.allPlayerData.TryGetValue(__instance, out var playerData))
                return true;

            LCVR_Compat.vrPlayerScrollingBetweenHotbars = true;
            LCVR_Compat.vrPlayerNextItemSlot = -1;
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "ScrollMouse_performed")]
        [HarmonyPostfix]
        public static void OnVRPlayerScrollPost(InputAction.CallbackContext context, PlayerControllerB __instance)
        {
            if (__instance == localPlayerController && LCVR_Compat.vrPlayerScrollingBetweenHotbars)
            {
                LCVR_Compat.vrPlayerScrollingBetweenHotbars = false;
                if (LCVR_Compat.LoadedAndEnabled && LCVR_Compat.vrPlayerNextItemSlot != -1 && ReservedPlayerData.allPlayerData.TryGetValue(__instance, out var playerData))
                {
                    Keybinds.pressedToggleKey = true;
                    ReservedHotbarManager.FocusReservedHotbarSlots(!playerData.inReservedHotbarSlots, LCVR_Compat.vrPlayerNextItemSlot);
                    LCVR_Compat.vrPlayerNextItemSlot = -1;
                }
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "NextItemSlot")]
        [HarmonyPostfix]
        public static void OnNextItemSlot(ref int __result, bool forward, PlayerControllerB __instance)
        {
            LCVR_Compat.vrPlayerNextItemSlot = -1;
            if (PlayerPatcher.reservedHotbarSize <= 0 || !HUDPatcher.hasReservedItemSlotsAndEnabled)
                return;

            if (!ReservedPlayerData.allPlayerData.TryGetValue(__instance, out var playerData))
                return;

            // Prevent scrolling other hotbar
            // Skip empty item slots (in reserved hotbar)
            bool currentlyInReservedSlots = playerData.inReservedHotbarSlots;
            bool resultInReservedSlots = playerData.IsReservedItemSlot(__result);

            if (LCVR_Compat.LoadedAndEnabled && __instance == localPlayerController && LCVR_Compat.vrPlayerScrollingBetweenHotbars)
            {
                if (currentlyInReservedSlots != resultInReservedSlots)
                {
                    LCVR_Compat.vrPlayerNextItemSlot = __result;
                    return;
                }
                LCVR_Compat.vrPlayerScrollingBetweenHotbars = false;
                return;
            }
        }


        [HarmonyPatch(typeof(PlayerControllerB), "SwitchToItemSlot")]
        [HarmonyPatch(typeof(PlayerControllerB), "SwitchItemSlotsServerRpc")]
        [HarmonyPrefix]
        public static bool OnSwitchToItemSlot(PlayerControllerB __instance)
        {
            if (LCVR_Compat.LoadedAndEnabled && __instance == localPlayerController && vrPlayerScrollingBetweenHotbars)
                return false;
            return true;
        }
    }
}
