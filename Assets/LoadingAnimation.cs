using TMPro;
using UnityEngine;
using XephTools;

public class LoadingAnimation : MonoBehaviour
{
    [SerializeField] float _rate = 0.25f;
    [SerializeField] int _numDots = 3;

    float _timer = 0f;
    int _dotCount = -1;

    TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    //private void OnEnable()
    //{
    //    UpdateAnimation();
    //}

    private void Update()
    {
        if (_timer <= 0f)
        {
            UpdateAnimation();
            return;
        }
        _timer -= Time.deltaTime;
    }

    private void UpdateAnimation()
    {
        _timer = _rate;
        _dotCount = (_dotCount + 1) % _numDots;
        string str = "Loading";
        for (int i = 0; i < _dotCount + 1; i++)
        {
            str += ".";
        }
        _text.text = str;
    }
}
