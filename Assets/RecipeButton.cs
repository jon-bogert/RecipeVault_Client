using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecipeButton : MonoBehaviour
{
    System.UInt32 _id;
    RecipeBrowser _browser;

    public void Setup(ManifestEntry entry, System.UInt32 id, RecipeBrowser browser)
    {
        _id = id;
        _browser = browser;

        TMP_Text text = GetComponentInChildren<TMP_Text>();
        text.text = entry.name;
    }

    public void OnPress()
    {
        if (TransitionManager.instance.isMoving)
            return;

        _browser.LoadRecipe(_id);
    }
}
