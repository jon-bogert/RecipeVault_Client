using System.Collections.Generic;
using System.Linq;
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
    CustomKeyboard _keyboard;


private void Start()
    {
        _keyboard = FindObjectOfType<CustomKeyboard>(true);
        _browser = FindObjectOfType<RecipeBrowser>();
        gameObject.SetActive(false);

        _noResultText.SetActive(false);
        _searchToBeginText.SetActive(true);
    }

    void OnManualKey(string letter)
    {
        _searchText.text += letter;
        OnStringChange();
    }
    void OnManualBack()
    {
        if (_searchText.text == "")
            return;

        _searchText.text = _searchText.text.Remove(_searchText.text.Length - 1);
    }
    void OnManualSubmit()
    {
        _keyboard.Deactivate(true);
    }

    public void OnBoxSelect(bool desktop = false)
    {
#if UNITY_WEBGL
        //if (TouchScreenKeyboard.isSupported)
        if (Application.isMobilePlatform || desktop)
        {
            if (_keyboard.isActive)
                return;

            _keyboard.Activate();
            _keyboard.onEdit += OnManualKey;
            _keyboard.onBack += OnManualBack;
            _keyboard.onEnter += OnManualSubmit;
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
        List<string> searchWords = str.Split(' ').ToList();
        searchWords.RemoveAll((s) => { return s == ""; });

        List<string> names = new();
        foreach (ManifestEntry entry in Manifest.instance.entries.Values)
        {
            string[] entryWords = entry.name.ToLower().Split(' ');
            int count = 0;
            foreach (string s in searchWords)
            {
                bool found = false;
                foreach (string e in entryWords)
                {
                    if (!found && e.Contains(s))
                    {
                        found = true;
                        ++count;
                    }
                }
            }
            if (count >= searchWords.Count)
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
            newRB.GetComponentInChildren<TextSizer>().CopyFrom(oldRB.GetComponentInChildren<TextSizer>());
            _buttons.Add(newRB);
        }

    }
}
