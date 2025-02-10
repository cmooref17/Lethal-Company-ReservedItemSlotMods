using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine.InputSystem;
using ReservedItemSlotCore.Patches;
using ReservedItemSlotCore.Networking;
using ReservedItemSlotCore;
using ReservedItemSlotCore.Input;
using UnityEngine;
using ReservedItemSlotCore.Data;


namespace ReservedUtilitySlot.Input
{
	[HarmonyPatch]
	internal static class Keybinds
    {
        public static PlayerControllerB localPlayerController { get { return StartOfRound.Instance?.localPlayerController; } }
        public static ReservedPlayerData localPlayerData { get { return ReservedPlayerData.localPlayerData; } }

        public static InputActionAsset Asset;
        public static InputActionMap ActionMap;

        static InputAction ToggleUtilitySlotAction;


        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        public static void AddToKeybindMenu()
        {
            Plugin.Log("Initializing hotkeys.");
            if (InputUtilsCompat.Enabled)
            {
                Asset = InputUtilsCompat.Asset;
                ActionMap = Asset.actionMaps[0];
                ToggleUtilitySlotAction = InputUtilsCompat.ToggleUtilitySlotHotkey;
            }
            else
            {
                Asset = ScriptableObject.CreateInstance<InputActionAsset>();
                ActionMap = new InputActionMap("ReservedItemSlots");
                Asset.AddActionMap(ActionMap);

                ToggleUtilitySlotAction = ActionMap.AddAction("ReservedItemSlots.ToggleUtilitySlot", binding: "");
            }
        }


        [HarmonyPatch(typeof(StartOfRound), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable()
        {
            Asset.Enable();
            ToggleUtilitySlotAction.performed += OnSwapToUtilitySlot;
            ToggleUtilitySlotAction.canceled += OnSwapToUtilitySlot;
        }


        [HarmonyPatch(typeof(StartOfRound), "OnDisable")]
        [HarmonyPostfix]
        public static void OnDisable()
        {
            Asset.Disable();
            ToggleUtilitySlotAction.performed -= OnSwapToUtilitySlot;
            ToggleUtilitySlotAction.canceled -= OnSwapToUtilitySlot;
        }




        private static void OnSwapToUtilitySlot(InputAction.CallbackContext context)
        {
            if (localPlayerController == null || localPlayerData == null || !localPlayerController.isPlayerControlled || (localPlayerController.IsServer && !localPlayerController.isHostPlayerObject))
                return;

            if (Plugin.allUtilitySlotData != null)
            {
                var focusUtilitySlotData = new List<ReservedItemSlotData>();
                foreach (var slotData in Plugin.allUtilitySlotData)
                {
                    if (SessionManager.unlockedReservedItemSlotsDict.TryGetValue(slotData.slotName, out var unlockedSlotData))
                    {
                        focusUtilitySlotData.Add(unlockedSlotData);
                    }
                }
                if (focusUtilitySlotData.Count > 0)
                    ReservedHotbarManager.ForceToggleReservedHotbar(focusUtilitySlotData.ToArray());
            }
        }
    }
}