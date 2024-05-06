﻿using Discord;
using GameNetcodeStuff;
using HarmonyLib;
using ReservedItemSlotCore.Data;
using ReservedItemSlotCore.Input;
using ReservedItemSlotCore.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ReservedItemSlotCore.Patches
{
    [HarmonyPatch]
    internal static class DropReservedItemPatcher
    {
        private static PlayerControllerB localPlayerController { get { return StartOfRound.Instance?.localPlayerController; } }
        private static HashSet<PlayerControllerB> playersDiscardingItems = new HashSet<PlayerControllerB>();


        [HarmonyPatch(typeof(PlayerControllerB), "SetObjectAsNoLongerHeld")]
        [HarmonyPostfix]
        private static void OnSetObjectNoLongerHeld(bool droppedInElevator, bool droppedInShipRoom, Vector3 targetFloorPosition, GrabbableObject dropObject, PlayerControllerB __instance)
        {
            OnDiscardItem(__instance);
        }


        [HarmonyPatch(typeof(PlayerControllerB), "PlaceGrabbableObject")]
        [HarmonyPostfix]
        private static void OnPlaceGrabbableObject(Transform parentObject, Vector3 positionOffset, bool matchRotationOfParent, GrabbableObject placeObject, PlayerControllerB __instance)
        {
            OnDiscardItem(__instance);
        }


        [HarmonyPatch(typeof(PlayerControllerB), "DestroyItemInSlot")]
        [HarmonyPostfix]
        private static void OnDestroyItem(int itemSlot, PlayerControllerB __instance)
        {
            if (__instance == localPlayerController && itemSlot >= ReservedPlayerData.localPlayerData.reservedHotbarStartIndex && itemSlot < ReservedPlayerData.localPlayerData.reservedHotbarEndIndexExcluded)
                HUDPatcher.UpdateUI();
        }


        [HarmonyPatch(typeof(PlayerControllerB), "DespawnHeldObjectOnClient")]
        [HarmonyPostfix]
        private static void OnDespawnItem(PlayerControllerB __instance)
        {
            if (__instance == localPlayerController)
                HUDPatcher.UpdateUI();
        }


        [HarmonyPatch(typeof(PlayerControllerB), "DropAllHeldItems")]
        [HarmonyPostfix]
        private static void OnDropAllHeldItems(PlayerControllerB __instance)
        {
            if (__instance == localPlayerController)
                HUDPatcher.UpdateUI();
        }


        private static void OnDiscardItem(PlayerControllerB playerController)
        {
            if (playerController != null && !playersDiscardingItems.Contains(playerController) && ReservedPlayerData.allPlayerData.TryGetValue(playerController, out var playerData) && playerData.currentItemSlotIsReserved && playerData.currentlySelectedItem == null)
            {
                if (playerData.GetNumHeldReservedItems() > 0)
                {
                    int swapToIndex = playerData.CallGetNextItemSlot(true);
                    if (!playerData.IsReservedItemSlot(swapToIndex))
                    {
                        if (!playerData.IsReservedItemSlot(ReservedHotbarManager.indexInHotbar))
                            swapToIndex = ReservedHotbarManager.indexInHotbar;
                    }
                    playersDiscardingItems.Add(playerController);
                    playerController.StartCoroutine(SwitchToItemSlotAfterDelay(playerController, swapToIndex));
                }
                if (playerController == localPlayerController)
                    HUDPatcher.UpdateUI();
            }
        }


        private static IEnumerator SwitchToItemSlotAfterDelay(PlayerControllerB playerController, int slot)
        {
            if (playerController == localPlayerController)
                yield return new WaitUntil(() => playerController.currentlyHeldObjectServer == null);
            yield return new WaitForEndOfFrame();
            playersDiscardingItems.Remove(playerController);
            if (playerController.currentItemSlot != slot && ReservedPlayerData.allPlayerData.TryGetValue(playerController, out var playerData))
                playerData.CallSwitchToItemSlot(slot);
        }


        [HarmonyPatch(typeof(PlayerControllerB), "ScrollMouse_performed")]
        [HarmonyPrefix]
        public static bool PreventItemSwappingDroppingItem(InputAction.CallbackContext context, PlayerControllerB __instance)
        {
            if (__instance == localPlayerController && playersDiscardingItems.Contains(__instance))
                return false;
            return true;
        }
    }
}
