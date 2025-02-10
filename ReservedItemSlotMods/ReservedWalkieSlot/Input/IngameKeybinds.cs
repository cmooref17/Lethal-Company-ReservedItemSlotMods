﻿using BepInEx.Bootstrap;
using LethalCompanyInputUtils.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace ReservedWalkieSlot.Input
{
    internal class IngameKeybinds : LcInputActions
    {
        internal static IngameKeybinds Instance = new IngameKeybinds();
        internal static InputActionAsset GetAsset() => Instance.Asset;

        [InputAction("<Keyboard>/x", Name = "Activate Walkie")]
        public InputAction ActivateWalkieHotkey { get; set; }

        [InputAction("", Name = "Toggle Walkie Slot")]
        public InputAction ToggleWalkieSlotHotkey { get; set; }
    }
}
