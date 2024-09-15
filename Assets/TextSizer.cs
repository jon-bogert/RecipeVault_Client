using TMPro;
using UnityEngine;

public class TextSizer : MonoBehaviour
{
    const float k_sizeFactor = 1.5f;

    TMP_Text _textField;
    float _originalSize = 0f;
    float _largerSize = 0f;

    private void Start()
    {
        _textField = GetComponent<TMP_Text>();
        _originalSize = _textField.fontSize;
        _largerSize = _textField.fontSize * k_sizeFactor;

        Manifest.instance.textSizeChanged += OnSizeChange;
    }

    private void OnDestroy()
    {
        Manifest.instance.textSizeChanged -= OnSizeChange;
    }

    private void OnSizeChange(bool isLarger)
    {
        _textField.fontSize = (isLarger) ? _largerSize : _originalSize;
    }
}
