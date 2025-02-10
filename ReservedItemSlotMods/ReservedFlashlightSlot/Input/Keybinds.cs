using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine.InputSystem;
using UnityEngine;
using ReservedItemSlotCore.Patches;
using ReservedFlashlightSlot.Patches;
using ReservedFlashlightSlot.Config;
using ReservedItemSlotCore.Data;
using ReservedItemSlotCore;

namespace ReservedFlashlightSlot.Input
{
	[HarmonyPatch]
	internal static class Keybinds
    {
        public static PlayerControllerB localPlayerController { get { return StartOfRound.Instance?.localPlayerController; } }
        public static ReservedPlayerData localPlayerData { get { return ReservedPlayerData.localPlayerData; } }

        public static InputActionAsset Asset;
        public static InputActionMap ActionMap;

        static InputAction ActivateFlashlightAction;
        static InputAction ToggleFlashlightSlotAction;


        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        public static void AddToKeybindMenu()
        {
            Plugin.Log("Initializing hotkeys.");
            if (InputUtilsCompat.Enabled)
            {
                Asset = InputUtilsCompat.Asset;
                ActionMap = Asset.actionMaps[0];
                ActivateFlashlightAction = InputUtilsCompat.ToggleFlashlightHotkey;
                ToggleFlashlightSlotAction = InputUtilsCompat.ToggleFlashlightSlotHotkey;
            }
            else
            {
                Asset = ScriptableObject.CreateInstance<InputActionAsset>();
                ActionMap = new InputActionMap("ReservedItemSlots");
                Asset.AddActionMap(ActionMap);

                ActivateFlashlightAction = ActionMap.AddAction("ReservedItemSlots.ToggleFlashlight", binding: "<keyboard>/f");
                ToggleFlashlightSlotAction = ActionMap.AddAction("ReservedItemSlots.ToggleFlashlightSlot", binding: "");
            }
        }


        [HarmonyPatch(typeof(StartOfRound), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable()
        {
            Asset.Enable();
            ActivateFlashlightAction.performed += OnActivateFlashlightPerformed;

            ToggleFlashlightSlotAction.performed += OnSwapToFlashlightSlot;
            ToggleFlashlightSlotAction.canceled += OnSwapToFlashlightSlot;
        }


        [HarmonyPatch(typeof(StartOfRound), "OnDisable")]
        [HarmonyPostfix]
        public static void OnDisable()
        {
            Asset.Disable();
            ActivateFlashlightAction.performed -= OnActivateFlashlightPerformed;

            ToggleFlashlightSlotAction.performed -= OnSwapToFlashlightSlot;
            ToggleFlashlightSlotAction.canceled -= OnSwapToFlashlightSlot;
        }





        private static void OnActivateFlashlightPerformed(InputAction.CallbackContext context)
        {
            if (localPlayerController == null || !localPlayerController.isPlayerControlled || (localPlayerController.IsServer && !localPlayerController.isHostPlayerObject))
                return;

            if (!context.performed || ShipBuildModeManager.Instance.InBuildMode || localPlayerController.inTerminalMenu)
                return;

            if (ReservedPlayerData.localPlayerData.timeSinceSwitchingSlots < 0.075f)
                return;

            if (localPlayerController.isTypingChat || localPlayerController.inTerminalMenu || localPlayerController.quickMenuManager.isMenuOpen || localPlayerController.isPlayerDead || localPlayerController.isGrabbingObjectAnimation || ReservedPlayerData.localPlayerData.isGrabbingReservedItem)
                return;

            FlashlightItem mainFlashlight = FlashlightPatcher.GetMainFlashlight(localPlayerController);
            if (!mainFlashlight)
            {
                mainFlashlight = FlashlightPatcher.GetFirstFlashlightItem(localPlayerController);
                if (!mainFlashlight)
                    return;
            }

            bool activate = !mainFlashlight.isBeingUsed;
            if (activate && mainFlashlight != FlashlightPatcher.GetCurrentlySelectedFlashlight(localPlayerController))
                localPlayerController.pocketedFlashlight = mainFlashlight;

            mainFlashlight.UseItemOnClient(activate);
            Traverse.Create(localPlayerController).Field("timeSinceSwitchingSlots").SetValue(0);
        }


        private static void OnSwapToFlashlightSlot(InputAction.CallbackContext context)
        {
            if (localPlayerController == null || localPlayerData == null || !localPlayerController.isPlayerControlled || (localPlayerController.IsServer && !localPlayerController.isHostPlayerObject))
                return;

            if (SessionManager.unlockedReservedItemSlotsDict.TryGetValue(Plugin.flashlightSlotData.slotName, out var flashlightSlotData))
                ReservedHotbarManager.ForceToggleReservedHotbar(new ReservedItemSlotData[] { flashlightSlotData });
        }
    }
}