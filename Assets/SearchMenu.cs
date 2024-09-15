using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SearchMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TMP_InputField _searchText;
    [SerializeField] GameObject _scrollView;
    [SerializeField] Transform _scrollContent;
    [SerializeField] GameObject _noResultText;
    [SerializeField] GameObject _searchToBeginText;

    RecipeBrowser _browser;
    List<RecipeButton> _buttons = new();
#if UNITY_WEBGL
    TouchScreenKeyboard _keyboard;
#endif

private void Start()
    {
        _browser = FindObjectOfType<RecipeBrowser>();
        gameObject.SetActive(false);

        _noResultText.SetActive(false);
        _searchToBeginText.SetActive(true);
    }

#if UNITY_WEBGL
    private void Update()
    {
        if (_keyboard == null)
            return;
        if (!TouchScreenKeyboard.visible)
        {
            _keyboard = null;
            return;
        }

        if (_keyboard.text != _searchText.text)
        {
            _searchText.text = _keyboard.text;
            OnStringChange();
        }
    }
#endif

    public void OnBoxSelect()
    {
#if UNITY_WEBGL
        if (TouchScreenKeyboard.isSupported)
        {
            TouchScreenKeyboard.hideInput = true;
            _keyboard = TouchScreenKeyboard.Open(_searchText.text, TouchScreenKeyboardType.Default, true);
        }
#endif
    }

    public void ClearBox()
    {
        _searchText.text = string.Empty;
        DoSearch(_searchText.text);
    }

    public void OnStringChange()
    {
        DoSearch(_searchText.text);
    }

    private void DoSearch(string str)
    {
        for (int i = 0; i < _buttons.Count; ++i)
        {
            Destroy(_buttons[i].gameObject);
        }
        _buttons = new List<RecipeButton>();

        if (str == "")
        {
            _scrollView.SetActive(false);
            _noResultText.SetActive(false);
            _searchToBeginText.SetActive(true);
            return;
        }
        _searchToBeginText.SetActive(false);

        str = str.ToLower();
        List<string> names = new();
        foreach (ManifestEntry entry in Manifest.instance.entries.Values)
        {
            string entryLower = entry.name.ToLower();
            if (entryLower.Contains(str))
            {
                names.Add(entry.name);
            }
        }

        if (names.Count == 0)
        {
            _scrollView.SetActive(false);
            _noResultText.SetActive(true);
            return;
        }

        _scrollView.SetActive(true);
        _noResultText.SetActive(false);
        names.Sort();

        foreach (string name in names)
        {
            RecipeButton oldRB = _browser.buttonReferences[name];
            RecipeButton newRB = Instantiate(oldRB, _scrollContent).GetComponent<RecipeButton>();
            newRB.CopyFrom(oldRB);

            _buttons.Add(newRB);
        }

    }
}
