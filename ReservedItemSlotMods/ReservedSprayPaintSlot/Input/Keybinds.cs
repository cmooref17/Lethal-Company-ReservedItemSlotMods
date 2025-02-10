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


namespace ReservedSprayPaintSlot.Input
{
	[HarmonyPatch]
	internal static class Keybinds
    {
        public static PlayerControllerB localPlayerController { get { return StartOfRound.Instance?.localPlayerController; } }
        public static ReservedPlayerData localPlayerData { get { return ReservedPlayerData.localPlayerData; } }

        public static InputActionAsset Asset;
        public static InputActionMap ActionMap;

        static InputAction ToggleSprayPaintSlotAction;


        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        public static void AddToKeybindMenu()
        {
            Plugin.Log("Initializing hotkeys.");
            if (InputUtilsCompat.Enabled)
            {
                Asset = InputUtilsCompat.Asset;
                ActionMap = Asset.actionMaps[0];
                ToggleSprayPaintSlotAction = InputUtilsCompat.ToggleSprayPaintSlotHotkey;
            }
            else
            {
                Asset = ScriptableObject.CreateInstance<InputActionAsset>();
                ActionMap = new InputActionMap("ReservedItemSlots");
                Asset.AddActionMap(ActionMap);

                ToggleSprayPaintSlotAction = ActionMap.AddAction("ReservedItemSlots.ToggleSprayPaintSlot", binding: "");
            }
        }


        [HarmonyPatch(typeof(StartOfRound), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable()
        {
            Asset.Enable();
            ToggleSprayPaintSlotAction.performed += OnSwapToSprayPaintSlot;
            ToggleSprayPaintSlotAction.canceled += OnSwapToSprayPaintSlot;
        }


        [HarmonyPatch(typeof(StartOfRound), "OnDisable")]
        [HarmonyPostfix]
        public static void OnDisable()
        {
            Asset.Disable();
            ToggleSprayPaintSlotAction.performed -= OnSwapToSprayPaintSlot;
            ToggleSprayPaintSlotAction.canceled -= OnSwapToSprayPaintSlot;
        }




        private static void OnSwapToSprayPaintSlot(InputAction.CallbackContext context)
        {
            if (localPlayerController == null || localPlayerData == null || !localPlayerController.isPlayerControlled || (localPlayerController.IsServer && !localPlayerController.isHostPlayerObject))
                return;

            if (SessionManager.unlockedReservedItemSlotsDict.TryGetValue(Plugin.sprayPaintSlotData.slotName, out var sprayPaintSlotData))
                ReservedHotbarManager.ForceToggleReservedHotbar(new ReservedItemSlotData[] { sprayPaintSlotData });
        }
    }
}