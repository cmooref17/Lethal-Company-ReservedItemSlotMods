using BepInEx.Bootstrap;
using LethalCompanyInputUtils.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace ReservedFlashlightSlot.Input
{
    internal class IngameKeybinds : LcInputActions
    {
        internal static IngameKeybinds Instance = new IngameKeybinds();
        internal static InputActionAsset GetAsset() => Instance.Asset;

        [InputAction("<Keyboard>/f", Name = "Toggle On/Off Flashlight")]
        public InputAction ToggleFlashlightHotkey { get; set; }

        [InputAction("", Name = "Toggle Flashlight Slot")]
        public InputAction ToggleFlashlightSlotHotkey { get; set; }
    }
}
