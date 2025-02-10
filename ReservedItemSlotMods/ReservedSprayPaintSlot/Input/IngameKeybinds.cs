using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace ReservedSprayPaintSlot.Input
{
    internal class IngameKeybinds : LcInputActions
    {
        internal static IngameKeybinds Instance = new IngameKeybinds();
        internal static InputActionAsset GetAsset() => Instance.Asset;

        [InputAction("", Name = "Toggle Spray Paint Slot")]
        public InputAction ToggleSprayPaintSlotHotkey { get; set; }
    }
}
