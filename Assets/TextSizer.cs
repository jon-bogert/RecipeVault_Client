using TMPro;
using UnityEngine;

public class TextSizer : MonoBehaviour
{
    const float k_sizeFactor = 1.5f;

    TMP_Text _textField;
    float _originalSize = 0f;
    float _largerSize = 0f;

    private void Awake()
    {
        _textField = GetComponent<TMP_Text>();
        if (_originalSize == 0f)
        {
            _originalSize = _textField.fontSize;
            _largerSize = _textField.fontSize * k_sizeFactor;
        }
    }

    private void Start()
    {
        Manifest.instance.textSizeChanged += OnSizeChange;
        OnSizeChange(Manifest.instance.viewingOptions.useLargerText);
    }

    private void OnDestroy()
    {
        Manifest.instance.textSizeChanged -= OnSizeChange;
    }

    private void OnSizeChange(bool isLarger)
    {
        _textField.fontSize = (isLarger) ? _largerSize : _originalSize;
    }

    public void CopyFrom(TextSizer other)
    {
        _originalSize = other._originalSize;
        _largerSize = other._largerSize;
    }
}
