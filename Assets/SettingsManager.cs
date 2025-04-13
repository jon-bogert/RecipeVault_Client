using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 120;
    }

    private void Start()
    {
#if UNITY_ANDROID
        GetComponent<CanvasScaler>().matchWidthOrHeight = 0.436f;
#endif
    }
}
