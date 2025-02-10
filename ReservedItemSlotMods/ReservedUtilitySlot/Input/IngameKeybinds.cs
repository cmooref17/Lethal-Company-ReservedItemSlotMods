using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace ReservedUtilitySlot.Input
{
    internal class IngameKeybinds : LcInputActions
    {
        internal static IngameKeybinds Instance = new IngameKeybinds();
        internal static InputActionAsset GetAsset() => Instance.Asset;

        [InputAction("", Name = "Toggle Utility Slot")]
        public InputAction ToggleUtilitySlotHotkey { get; set; }
    }
}
