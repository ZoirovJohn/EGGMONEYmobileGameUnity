using UnityEngine;
using UnityEngine.InputSystem;

public class TouchFXInput : MonoBehaviour
{
    void Update()
    {
        // 📱 Touch (mobile)
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                FXManager.Instance?.PlayTouchFX(
                    touch.position.ReadValue()
                );
            }
        }

        // 🖱 Mouse (Editor / PC)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            FXManager.Instance?.PlayTouchFX(
                Mouse.current.position.ReadValue()
            );
        }
    }
}
