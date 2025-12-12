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
    private int frameCount = 0;
    private float cachedFPS = 0f;

    void Start()
    {
        // Initialize the array to store frame delta times
        frameDeltaTimes = new float[frameRange];
    }

    void Update()
    {
        // Ensure frameRange is valid
        if (frameRange < 1) frameRange = 1;
        
        // Resize array if frameRange changed
        if (frameDeltaTimes.Length != frameRange)
        {
            frameDeltaTimes = new float[frameRange];
            frameIndex = 0;
            frameCount = 0;
        }
        
        // Store the current frame's delta time using unscaled time
        // (unaffected by Time.timeScale, useful for pause menus, etc.)
        frameDeltaTimes[frameIndex] = Time.unscaledDeltaTime;
        
        // Track how many frames we've collected
        if (frameCount < frameRange)
        {
            frameCount++;
        }
        
        // Move to the next index, wrapping around when we reach the end
        frameIndex = (frameIndex + 1) % frameRange;
        
        // Calculate the average delta time over populated frames only
        float avgDeltaTime = 0f;
        for (int i = 0; i < frameCount; i++)
        {
            avgDeltaTime += frameDeltaTimes[i];
        }
        avgDeltaTime /= frameCount;

        // Convert to FPS (frames per second) and cache it
        cachedFPS = avgDeltaTime > 0 ? 1f / avgDeltaTime : 0f;
    }

    void OnGUI()
    {
        // Display the cached FPS value in the top-left corner
        GUI.Label(new Rect(10, 10, 100, 20), string.Format("FPS: {0:F1}", cachedFPS));
    }
}
