using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;
using XephTools;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance;

    [SerializeField] float _transitionTime = 0.5f;

    RectTransform _fromTransform = null;
    RectTransform _toTransform = null;

    RectTransform _canvasTransform;
    float _canvasScale = 1f;

    bool _isMoving = false;

    public bool isMoving { get { return _isMoving; } }
    public Action moveComplete;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;
    }

    private void Start()
    {
        _canvasTransform = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
        _canvasScale = Screen.width / _canvasTransform.GetComponent<CanvasScaler>().referenceResolution.x;
    }

    public void TransitionLeft(GameObject from, GameObject to)
    {
        if (_isMoving)
        {
            Debug.LogError("Transition already in effect");
            return;
        }

        _fromTransform = from.GetComponent<RectTransform>();
        _toTransform = to.GetComponent<RectTransform>();

        if (_fromTransform == null || _toTransform == null)
        {
            Debug.LogError("No rect transform found on one or both GameObjects");
            return;
        }

        DoMove(Vector3.left);
    }

    public void TransitionRight(GameObject from, GameObject to)
    {
        if (_isMoving)
        {
            Debug.LogError("Transition already in effect");
            return;
        }

        _fromTransform = from.GetComponent<RectTransform>();
        _toTransform = to.GetComponent<RectTransform>();

        if (_fromTransform == null || _toTransform == null)
        {
            Debug.LogError("No rect transform found on one or both GameObjects");
            return;
        }

        DoMove(Vector3.right);
    }

    private void OnComplete()
    {
        _isMoving = false;
        moveComplete?.Invoke();
        moveComplete = null;
    }

    private void DoMove(Vector3 v)
    {
        Vector3 direction = v * _canvasTransform.rect.width;
        Vector3 home = _fromTransform.localPosition;
        Vector3Lerp fromLerp =
            new(home, home + direction,
            _transitionTime,
            (Vector3 val) => { _fromTransform.localPosition = val; }
        );

        Vector3Lerp toLerp =
            new(home - direction, home,
            _transitionTime,
            (Vector3 val) => { _toTransform.localPosition = val; }
        );

        _isMoving = true;
        fromLerp.OnComplete(OnComplete);

        OverTime.Add(fromLerp);
        OverTime.Add(toLerp);
    }

    public void FadeTransition(CanvasGroup from, CanvasGroup to)
    {
        if (_isMoving)
        {
            Debug.LogError("Transition already in effect");
            return;
        }

        OverTime.LerpModule fromLerp = new(1f, 0f, _transitionTime * 0.5f, (float v) => { from.alpha = v; });
        OverTime.LerpModule toLerp = new(0f, 1f, _transitionTime * 0.5f, (float v) => { to.alpha = v; });

        toLerp.OnComplete(OnComplete);
        fromLerp.OnComplete(() => { OverTime.Add(toLerp); });
        OverTime.Add(fromLerp);

        _isMoving = true;
    }
}
