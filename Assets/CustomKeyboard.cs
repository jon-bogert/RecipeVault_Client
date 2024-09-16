using System;
using UnityEngine;
using UnityEngine.UI;

public class CustomKeyboard : MonoBehaviour
{
    [SerializeField] float _transitionTime = 0.1f;
    [SerializeField] float _cubicWeight = 0.5f;
    [SerializeField] float _doubleTapTime = 0.1f;
    [SerializeField] Color _activeColor = Color.grey;
    [SerializeField] Color _lockColor = Color.white;

    [Header("References")]
    [SerializeField] Image _shiftKey;
    [SerializeField] GameObject _background;

    bool _shift = false;
    bool _caps = false;

    public Action<string> onEdit;
    public Action onBack;
    public Action onEnter;

    enum State { Down, Up, GoingDown, GoingUp }
    State _state = State.Down; 
    RectTransform _transform;
    Vector3 _upPosition;
    Vector3 _downPosition;
    Color _inactiveColor = Color.black;
    float _invTime = 4f;
    float _timer = 0f;
    float _doubleTimer = 0f;

    public bool isActive { get { return _background.activeSelf; } }

    private void Awake()
    {
        _invTime = 1f / _transitionTime;
    }

    private void Start()
    {
        _transform = GetComponent<RectTransform>();
        _upPosition = transform.localPosition;
        _downPosition = transform.localPosition + Vector3.down * _transform.sizeDelta.y;

        _inactiveColor = _shiftKey.color;

        transform.localPosition = _downPosition;
        _background.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_doubleTimer > 0f)
        {
            _doubleTimer -= Time.unscaledDeltaTime;
        }

        if (_state == State.Up || _state == State.Down)
            return;

        if (_timer <= 0f)
        {
            if (_state == State.GoingUp)
            {
                _state = State.Up;
                _transform.localPosition = _upPosition;
                return;
            }
            _state = State.Down;
            _transform.localPosition = _downPosition;
            _background.gameObject.SetActive(false);
        }

        float t = 1f - (_timer * _invTime);

        Vector3 start = (_state == State.GoingUp) ? _downPosition : _upPosition;
        Vector3 end = (_state == State.GoingUp) ? _upPosition : _downPosition;
        Vector3 mid = ((end - start) * _cubicWeight) + start;

        Vector3 a = Vector3.Lerp(start, mid, t);
        Vector3 b = Vector3.Lerp(mid, end, t);
        transform.localPosition = Vector3.Lerp(a, b, t);

        _timer -= Time.deltaTime;
    }

    public void Activate()
    {
        if (_state != State.Down)
            return;

        _background.gameObject.SetActive(true);
        _state = State.GoingUp;
        _timer = _transitionTime;
    }

    public void Deactivate(bool desktopOverride = false)
    {
        if (!Application.isMobilePlatform && !desktopOverride)
            return;

        if (_state != State.Up)
            return;

        _state = State.GoingDown;
        _timer = _transitionTime;

        onEdit = null;
        onBack = null;
        onEnter = null;
    }

    public void PressKey(string letter)
    {
        if (letter.Length != 1)
            return;

        if (_shift && letter[0] >= 'a' && letter[0] <= 'z')
            letter = letter.ToUpper();

        if (!_caps)
        {
            _shift = false;
            _shiftKey.color = _inactiveColor;
        }

        onEdit?.Invoke(letter);
    }

    public void ToggleShift()
    {
        if (_caps)
            _caps = false;

        if (_shift && _doubleTimer > 0f)
        {
            _caps = true;
            _shiftKey.color = _lockColor;
            return;
        }

        _shift = !_shift;
        _shiftKey.color = (_shift) ? _activeColor : _inactiveColor;

        if (_shift && !_caps)
            _doubleTimer = _doubleTapTime;
    }

    public void PressBack()
    {
        onBack?.Invoke();
    }

    public void PressEnter()
    {
        onEnter?.Invoke();
    }
}
