using System;
using System.Collections.Generic;
using UnityEngine;

public class RecipeBrowser : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject _recipeButtonPrefab;

    [Header("Scene References")]
    [SerializeField] GameObject _mealContent;
    [SerializeField] GameObject _dessertContent;
    [SerializeField] GameObject _cocktailContent;
    [SerializeField] CanvasGroup _thisCanvasGroup;

    RecipeViewer _viewer;
    CustomKeyboard _keyboard;
    CanvasGroup _search;

    public Dictionary<string, RecipeButton> buttonReferences {  get; private set; }

    private void Start()
    {
        Manifest.instance.CallOnLoadComplete(OnRecipeLoad);
        _viewer = FindObjectOfType<RecipeViewer>();
        _search = FindObjectOfType<SearchMenu>(true).GetComponent<CanvasGroup>();
        _keyboard = FindObjectOfType<CustomKeyboard>(true);
        _search.alpha = 0f;
    }

    private void OnRecipeLoad(bool success)
    {
        List<Tuple<System.UInt32, ManifestEntry>> mealList = new();
        List<Tuple<System.UInt32, ManifestEntry>> dessertList = new();
        List<Tuple<System.UInt32, ManifestEntry>> cocktailList = new();

        foreach (var entry in Manifest.instance.entries)
        {
            switch(entry.Value.category)
            {
                case Category.Meal:
                    mealList.Add(new(entry.Key, entry.Value));
                    break;
                case Category.Dessert:
                    dessertList.Add(new(entry.Key, entry.Value));
                    break;
                case Category.Cocktail:
                    cocktailList.Add(new(entry.Key, entry.Value));
                    break;
                default:
                    Debug.LogError("Unimplemented Enum");
                    break;
            }
        }

        mealList.Sort((a, b) => a.Item2.name.CompareTo(b.Item2.name));
        dessertList.Sort((a, b) => a.Item2.name.CompareTo(b.Item2.name));
        cocktailList.Sort((a, b) => a.Item2.name.CompareTo(b.Item2.name));

        buttonReferences = new();

        foreach (var entry in mealList)
        {
            RecipeButton rb = Instantiate(_recipeButtonPrefab, _mealContent.transform).GetComponent<RecipeButton>();
            rb.Setup(entry.Item2, entry.Item1, this);
            buttonReferences.Add(entry.Item2.name, rb);
        }

        foreach (var entry in dessertList)
        {
            RecipeButton rb = Instantiate(_recipeButtonPrefab, _dessertContent.transform).GetComponent<RecipeButton>();
            rb.Setup(entry.Item2, entry.Item1, this);
            buttonReferences.Add(entry.Item2.name, rb);
        }

        foreach (var entry in cocktailList)
        {
            RecipeButton rb = Instantiate(_recipeButtonPrefab, _cocktailContent.transform).GetComponent<RecipeButton>();
            rb.Setup(entry.Item2, entry.Item1, this);
            buttonReferences.Add(entry.Item2.name, rb);
        }
    }

    public void LoadRecipe(System.UInt32 id)
    {
        _viewer.Activate(id);
        _keyboard.Deactivate();
        TransitionManager.instance.TransitionLeft(gameObject, _viewer.gameObject);
    }

    public void UnloadRecipe()
    {
        if (TransitionManager.instance.isMoving)
            return;

        TransitionManager.instance.moveComplete += () => { _viewer.Deactivate(); };
        TransitionManager.instance.TransitionRight(_viewer.gameObject, gameObject);
    }

    public void ToSearch()
    {
        if (TransitionManager.instance.isMoving)
            return;

        _search.gameObject.SetActive(true);
        TransitionManager.instance.moveComplete += () => { _thisCanvasGroup.gameObject.SetActive(false); };
        TransitionManager.instance.FadeTransition(_thisCanvasGroup, _search);
    }

    public void FromSearch()
    {
        if (TransitionManager.instance.isMoving)
            return;

        _thisCanvasGroup.gameObject.SetActive(true);
        TransitionManager.instance.moveComplete += () => { _search.gameObject.SetActive(false); };
        TransitionManager.instance.FadeTransition(_search, _thisCanvasGroup);
    }
}
