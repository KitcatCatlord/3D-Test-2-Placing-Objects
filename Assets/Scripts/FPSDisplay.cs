using UnityEngine;

/// <summary>
/// Displays a simple frames-per-second (FPS) counter in the top-left corner of the screen.
/// Attach this script to any GameObject (e.g., Main Camera) to enable the FPS display.
/// </summary>
public class FPSDisplay : MonoBehaviour
{
    /// <summary>
    /// Number of frames to average over for FPS calculation.
    /// Higher values provide smoother readings but are less responsive to sudden changes.
    /// </summary>
    public int frameRange = 60;

    private float[] frameDeltaTimes;
    private int frameIndex = 0;
    private float frameTimeSum = 0f;
    private GUIStyle labelStyle;

    void Start()
    {
        // Validate and clamp frameRange to ensure it's at least 1
        if (frameRange < 1)
        {
            frameRange = 1;
        }
        
        // Initialize the array to store frame delta times
        frameDeltaTimes = new float[frameRange];
        
        // Create the GUI style once for efficiency
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 20;
        labelStyle.normal.textColor = Color.white;
    }

    void Update()
    {
        // Store unscaled delta time to avoid multiple property accesses
        float deltaTime = Time.unscaledDeltaTime;
        
        // Remove the old frame time from the sum and add the new one
        frameTimeSum -= frameDeltaTimes[frameIndex];
        frameDeltaTimes[frameIndex] = deltaTime;
        frameTimeSum += deltaTime;
        
        frameIndex = (frameIndex + 1) % frameRange;
    }

    void OnGUI()
    {
        // Calculate the average frame time using the maintained sum
        float averageFrameTime = frameTimeSum / frameRange;

        // Calculate FPS (avoid division by zero)
        float fps = averageFrameTime > 0 ? 1f / averageFrameTime : 0f;

        // Display the FPS counter in the top-left corner
        GUI.Label(new Rect(10, 10, 100, 30), string.Format("FPS: {0:F1}", fps), labelStyle);
    }
}
