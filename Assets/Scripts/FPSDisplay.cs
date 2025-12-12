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

    void Start()
    {
        // Initialize the array to store frame delta times
        frameDeltaTimes = new float[frameRange];
    }

    void Update()
    {
        // Record the unscaled delta time for this frame
        frameDeltaTimes[frameIndex] = Time.unscaledDeltaTime;
        frameIndex = (frameIndex + 1) % frameRange;
    }

    void OnGUI()
    {
        // Calculate the average frame time
        float sum = 0f;
        foreach (float deltaTime in frameDeltaTimes)
        {
            sum += deltaTime;
        }
        float averageFrameTime = sum / frameRange;

        // Calculate FPS (avoid division by zero)
        float fps = averageFrameTime > 0 ? 1f / averageFrameTime : 0f;

        // Display the FPS counter in the top-left corner
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 100, 30), string.Format("FPS: {0:F1}", fps), style);
    }
}
