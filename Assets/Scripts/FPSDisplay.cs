using UnityEngine;

/// <summary>
/// A simple FPS counter that displays frames per second in the top-left corner of the game view.
/// Attach this script to any active GameObject (e.g., Main Camera) to enable the FPS display.
/// </summary>
public class FPSDisplay : MonoBehaviour
{
    /// <summary>
    /// Number of frames to average over for smoother FPS display.
    /// Higher values = smoother but less responsive to FPS changes.
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
        // Store the current frame's delta time using unscaled time
        // (unaffected by Time.timeScale, useful for pause menus, etc.)
        frameDeltaTimes[frameIndex] = Time.unscaledDeltaTime;
        
        // Move to the next index, wrapping around when we reach the end
        frameIndex = (frameIndex + 1) % frameRange;
    }

    void OnGUI()
    {
        // Calculate the average delta time over our frame range
        float avgDeltaTime = 0f;
        for (int i = 0; i < frameDeltaTimes.Length; i++)
        {
            avgDeltaTime += frameDeltaTimes[i];
        }
        avgDeltaTime /= frameDeltaTimes.Length;

        // Convert to FPS (frames per second)
        float fps = avgDeltaTime > 0 ? 1f / avgDeltaTime : 0f;

        // Display FPS in the top-left corner with a simple label
        GUI.Label(new Rect(10, 10, 100, 20), string.Format("FPS: {0:F1}", fps));
    }
}
