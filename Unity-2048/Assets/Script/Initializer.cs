using UnityEngine;

public class Initializer
{
    [RuntimeInitializeOnLoadMethod]
    static void RuntimeInitializeOnLoad()
    {
#if UNITY_STANDALONE
        Screen.SetResolution(432, 768, FullScreenMode.Windowed, 60);
#endif

        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = false;
    }
}