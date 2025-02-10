using HarmonyLib;
using ReservedItemSlotCore.Data;
using TooManyEmotes;
using TooManyEmotes.Patches;
using UnityEngine;

namespace ReservedItemSlotCore.Compatibility
{
    internal static class TooManyEmotes_Compat
    {
        public static bool Enabled { get { return Plugin.IsModLoaded("FlipMods.TooManyEmotes"); } }

        public static bool IsLocalPlayerPerformingCustomEmote()
        {
            if (EmoteControllerPlayer.emoteControllerLocal != null && EmoteControllerPlayer.emoteControllerLocal.IsPerformingCustomEmote())
                return true;
            return false;
        }

        public static bool CanMoveWhileEmoting() => false; // ThirdPersonEmoteController.allowMovingWhileEmoting;

        [HarmonyPatch(typeof(TooManyEmotes.Patches.ThirdPersonEmoteController), "OnStartCustomEmoteLocal")]
        [HarmonyPostfix]
        private static void OnStartTMEEmote()
        {
            var reservedPlayerData = ReservedPlayerData.localPlayerData;
            
            // Loop through every reserved item slot on local player
            for (int i = reservedPlayerData.reservedHotbarStartIndex; i < reservedPlayerData.reservedHotbarEndIndexExcluded; i++)
            {
                if (i >= reservedPlayerData.itemSlots.Length)
                {
                    Plugin.LogWarning("Failed to patch TooManyEmotes OnStartCustomEmoteLocal. Likely a separate mod conflicting, but this should only prevent your held reserved items from displaying during emotes.");
                    continue;
                }
                var grabbableObject = reservedPlayerData.itemSlots[i];
                if (grabbableObject && SessionManager.TryGetUnlockedItemData(grabbableObject, out var reservedItemData) && reservedPlayerData.IsItemInReservedItemSlot(grabbableObject) && reservedItemData.showOnPlayerWhileHolstered)
                {
                    // Only modify visibility if item is holstered
                    if (grabbableObject != reservedPlayerData.currentlySelectedItem)
                    {
                        foreach (var renderer in grabbableObject.GetComponentsInChildren<MeshRenderer>())
                        {
                            if (!renderer.gameObject.CompareTag("DoNotSet") && !renderer.gameObject.CompareTag("InteractTrigger") && renderer.gameObject.layer != 14 && renderer.gameObject.layer != 22)
                                renderer.gameObject.layer = 6;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TooManyEmotes.Patches.ThirdPersonEmoteController), "OnStopCustomEmoteLocal")]
        [HarmonyPostfix]
        private static void OnStopTMEEmote()
        {
            var reservedPlayerData = ReservedPlayerData.localPlayerData;

            // Loop through every reserved item slot on local player
            for (int i = reservedPlayerData.reservedHotbarStartIndex; i < reservedPlayerData.reservedHotbarEndIndexExcluded; i++)
            {
                if (i >= reservedPlayerData.itemSlots.Length)
                    continue;
                var grabbableObject = reservedPlayerData.itemSlots[i];
                if (grabbableObject && SessionManager.TryGetUnlockedItemData(grabbableObject, out var reservedItemData) && reservedPlayerData.IsItemInReservedItemSlot(grabbableObject) && reservedItemData.showOnPlayerWhileHolstered)
                {
                    // Only modify visibility if item is holstered
                    if (grabbableObject != reservedPlayerData.currentlySelectedItem)
                    {
                        foreach (var renderer in grabbableObject.GetComponentsInChildren<MeshRenderer>())
                        {
                            if (!renderer.gameObject.CompareTag("DoNotSet") && !renderer.gameObject.CompareTag("InteractTrigger") && renderer.gameObject.layer != 14 && renderer.gameObject.layer != 22)
                                renderer.gameObject.layer = 23;
                        }
                    }
                }
            }
        }
    }
}