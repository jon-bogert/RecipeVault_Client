using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryToggle : MonoBehaviour
{
    [SerializeField] Color _activeColor = Color.grey;
    [SerializeField] Color _inactiveColor = Color.black;
    [SerializeField] Image[] buttons = new Image[0];
    [SerializeField] CanvasGroup[] _scrollViews = new CanvasGroup[0];

    int _tabIndex = 0;

    private void Start()
    {
        for (int i = 0; i < buttons.Length; ++i)
        {
            if (i == _tabIndex)
            {
                SetButtonActive(buttons[i]);
                _scrollViews[i].alpha = 1f;
            }
            else
            { 
                SetButtonInactive(buttons[i]);
                _scrollViews[i].alpha = 0f;
                _scrollViews[i].gameObject.SetActive(false);
            }
        }
    }

    public void ChangeCategory(int index)
    {
        if (index == _tabIndex || TransitionManager.instance.isMoving)
            return;

        for (int i = 0; i < buttons.Length; ++i)
        {
            _scrollViews[i].gameObject.SetActive(true);
            if (i == index)
                SetButtonActive(buttons[i]);
            else
                SetButtonInactive(buttons[i]);
        }

        TransitionManager.instance.moveComplete += () =>
        {
            for (int i = 0; i < buttons.Length; ++i)
            {
                if (i != index)
                    _scrollViews[i].gameObject.SetActive(false);
            }
        };
        TransitionManager.instance.FadeTransition(_scrollViews[_tabIndex], _scrollViews[index]);
        _tabIndex = index;
    }

    private void SetButtonActive(Image b)
    {
        b.color = _activeColor;
        b.GetComponentInChildren<TMP_Text>().fontStyle |= FontStyles.Bold;
    }

    private void SetButtonInactive(Image b)
    {
        b.color = _inactiveColor;
        b.GetComponentInChildren<TMP_Text>().fontStyle &= ~FontStyles.Bold;
    }
}
