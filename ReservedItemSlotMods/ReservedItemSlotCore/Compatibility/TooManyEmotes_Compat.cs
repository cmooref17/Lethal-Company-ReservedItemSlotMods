using HarmonyLib;
using ReservedItemSlotCore.Data;
using ReservedItemSlotCore.Patches;
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
        public static bool IsEmoteMenuOpen() => TooManyEmotes.UI.EmoteMenu.isMenuOpen;


        [HarmonyPatch(typeof(TooManyEmotes.Patches.ThirdPersonEmoteController), "OnStartCustomEmoteLocal")]
        [HarmonyPostfix]
        public static void OnStartTMEEmote()
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
                            if (!renderer.gameObject.CompareTag("DoNotSet") && !renderer.gameObject.CompareTag("InteractTrigger") && ReservedItemsPatcher.previousObjectLayers.TryGetValue(renderer.gameObject, out int originalLayer))
                            {
                                if (!ReservedItemsPatcher.IsLayerInLocalCameraMask(renderer.gameObject.layer))
                                    renderer.gameObject.layer = originalLayer;
                            }
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(TooManyEmotes.Patches.ThirdPersonEmoteController), "OnStopCustomEmoteLocal")]
        [HarmonyPostfix]
        public static void OnStopTMEEmote()
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
                            if (!renderer.gameObject.CompareTag("DoNotSet") && !renderer.gameObject.CompareTag("InteractTrigger") && ReservedItemsPatcher.previousObjectLayers.TryGetValue(renderer.gameObject, out int originalLayer))
                            {
                                if (ReservedItemsPatcher.IsLayerInLocalCameraMask(renderer.gameObject.layer))
                                    renderer.gameObject.layer = 23;
                            }
                        }
                    }
                }
            }
        }
    }
}