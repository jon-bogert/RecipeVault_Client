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

    RecipeViewer _viewer;

    private void Start()
    {
        Manifest.instance.CallOnLoadComplete(OnRecipeLoad);
        _viewer = FindObjectOfType<RecipeViewer>();
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

        foreach (var entry in mealList)
        {
            RecipeButton rb = Instantiate(_recipeButtonPrefab, _mealContent.transform).GetComponent<RecipeButton>();
            rb.Setup(entry.Item2, entry.Item1, this);
        }

        foreach (var entry in dessertList)
        {
            RecipeButton rb = Instantiate(_recipeButtonPrefab, _dessertContent.transform).GetComponent<RecipeButton>();
            rb.Setup(entry.Item2, entry.Item1, this);
        }

        foreach (var entry in cocktailList)
        {
            RecipeButton rb = Instantiate(_recipeButtonPrefab, _cocktailContent.transform).GetComponent<RecipeButton>();
            rb.Setup(entry.Item2, entry.Item1, this);
        }
    }

    public void LoadRecipe(System.UInt32 id)
    {
        _viewer.Activate(id);
        TransitionManager.instance.TransitionLeft(gameObject, _viewer.gameObject);
    }

    public void UnloadRecipe()
    {
        if (TransitionManager.instance.isMoving)
            return;

        TransitionManager.instance.moveComplete += () => { _viewer.Deactivate(); };
        TransitionManager.instance.TransitionRight(_viewer.gameObject, gameObject);
    }
}
