using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// ContinuousMoveProvider that ignores vertical (Y) input.
/// Replace the default ContinuousMoveProvider in Locomotion System.
/// </summary>
[AddComponentMenu("XR/Locomotion/Horizontal Move Provider")]
public class HorizontalMoveProvider : ContinuousMoveProviderBase
{
    // Implement required abstract method
    protected override Vector2 ReadInput()
    {
        // Use keyboard / joystick input
        float h = Input.GetAxis("Horizontal"); // A/D or joystick X
        float v = Input.GetAxis("Vertical");   // W/S or joystick Y
        return new Vector2(h, v);
    }

    // Override move calculation to remove vertical movement
    protected override Vector3 ComputeDesiredMove(Vector2 input)
    {
        Vector3 move = base.ComputeDesiredMove(input);

        // Ignore Y input (Q/E or vertical joystick)
        move.y = 0f;

        return move;
    }
}