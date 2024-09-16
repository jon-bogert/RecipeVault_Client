using UnityEngine;

public class WebGLDesktopOnly : MonoBehaviour
{
    private void Awake()
    {
        if (enabled)
        {
#if UNITY_WEBGL
            if (Application.isMobilePlatform)
                Destroy(gameObject);
#else
            Destroy(gameObject);
#endif
        }
    }

    private void Start()
    {
        
    }
}
