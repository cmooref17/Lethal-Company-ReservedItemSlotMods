﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;
using ReservedItemSlotCore.Patches;
using ReservedWalkieSlot.Patches;
using ReservedWalkieSlot.Config;
using ReservedItemSlotCore.Data;
using ReservedItemSlotCore;


namespace ReservedWalkieSlot.Input
{
	[HarmonyPatch]
	internal static class Keybinds
    {
        public static PlayerControllerB localPlayerController { get { return StartOfRound.Instance?.localPlayerController; } }
        public static ReservedPlayerData localPlayerData { get { return ReservedPlayerData.localPlayerData; } }

        public static InputActionAsset Asset;
        public static InputActionMap ActionMap;

        static InputAction ActivateWalkieAction;
        static InputAction ToggleWalkieSlotAction;


        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        public static void AddToKeybindMenu()
        {
            Plugin.Log("Initializing hotkeys.");
            if (InputUtilsCompat.Enabled)
            {
                Asset = InputUtilsCompat.Asset;
                ActionMap = Asset.actionMaps[0];
                ActivateWalkieAction = InputUtilsCompat.ActivateWalkieHotkey;
                ToggleWalkieSlotAction = InputUtilsCompat.ToggleWalkieSlotHotkey;
            }
            else
            {
                Asset = ScriptableObject.CreateInstance<InputActionAsset>();
                ActionMap = new InputActionMap("ReservedItemSlots");
                Asset.AddActionMap(ActionMap);

                ActivateWalkieAction = ActionMap.AddAction("ReservedItemSlots.ActivateWalkie", binding: "<keyboard>/x");
                ToggleWalkieSlotAction = ActionMap.AddAction("ReservedItemSlots.ToggleWalkieSlot", binding: "");
            }
        }


        [HarmonyPatch(typeof(StartOfRound), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable()
        {
            Asset.Enable();
            ActivateWalkieAction.performed += OnPressWalkieButtonPerformed;
            ActivateWalkieAction.canceled += OnReleaseWalkieButtonPerformed;

            ToggleWalkieSlotAction.performed += OnSwapToWalkieSlot;
            ToggleWalkieSlotAction.canceled += OnSwapToWalkieSlot;
        }


        [HarmonyPatch(typeof(StartOfRound), "OnDisable")]
        [HarmonyPostfix]
        public static void OnDisable()
        {
            Asset.Disable();
            ActivateWalkieAction.performed -= OnPressWalkieButtonPerformed;
            ActivateWalkieAction.canceled -= OnReleaseWalkieButtonPerformed;

            ToggleWalkieSlotAction.performed -= OnSwapToWalkieSlot;
            ToggleWalkieSlotAction.canceled -= OnSwapToWalkieSlot;
        }




        private static void OnPressWalkieButtonPerformed(InputAction.CallbackContext context)
        {
            if (localPlayerController == null || !localPlayerController.isPlayerControlled || (localPlayerController.IsServer && !localPlayerController.isHostPlayerObject))
                return;

            WalkieTalkie mainWalkie = WalkiePatcher.GetMainWalkie(localPlayerController);
            if (!context.performed || mainWalkie == null || !mainWalkie.isBeingUsed || ShipBuildModeManager.Instance.InBuildMode)
                return;

            if (localPlayerController.isTypingChat || localPlayerController.quickMenuManager.isMenuOpen || localPlayerController.isPlayerDead || ReservedPlayerData.localPlayerData.isGrabbingReservedItem)
                return;

            float timeSinceSwitchingSlots = (float)Traverse.Create(localPlayerController).Field("timeSinceSwitchingSlots").GetValue();
            if (timeSinceSwitchingSlots < 0.075f)
                return;

            mainWalkie.UseItemOnClient(true);
            Traverse.Create(localPlayerController).Field("timeSinceSwitchingSlots").SetValue(0);
        }


        private static void OnReleaseWalkieButtonPerformed(InputAction.CallbackContext context)
        {
            if (localPlayerController == null || !localPlayerController.isPlayerControlled || (localPlayerController.IsServer && !localPlayerController.isHostPlayerObject))
                return;

            WalkieTalkie mainWalkie = WalkiePatcher.GetMainWalkie(localPlayerController);
            if (!context.canceled || mainWalkie == null)
                return;

            mainWalkie.UseItemOnClient(false);
        }


        private static void OnSwapToWalkieSlot(InputAction.CallbackContext context)
        {
            if (localPlayerController == null || localPlayerData == null || !localPlayerController.isPlayerControlled || (localPlayerController.IsServer && !localPlayerController.isHostPlayerObject))
                return;

            if (SessionManager.unlockedReservedItemSlotsDict.TryGetValue(Plugin.walkieSlotData.slotName, out var walkieSlotData))
                ReservedHotbarManager.ForceToggleReservedHotbar(new ReservedItemSlotData[]{ walkieSlotData });
        }
    }
}