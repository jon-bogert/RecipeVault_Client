using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using YamlDotNet.RepresentationModel;

public class RecipeViewer : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] float _backgroundPadding = 25f;
    [SerializeField] Color _activeColor = Color.grey;
    [SerializeField] Color _inactiveColor = Color.black;

    [Header("References")]
    [SerializeField] GameObject _loading;
    [SerializeField] GameObject _failText;
    [SerializeField] GameObject _contentAll;
    [Space]
    [SerializeField] TMP_Text _recipeTitle;
    [SerializeField] RectTransform _scrollContent;
    [SerializeField] TMP_Text _ingredientText;
    [SerializeField] RectTransform _ingredientBox;
    [SerializeField] TMP_Text _directionsText;
    [SerializeField] RectTransform _directionsBox;
    [SerializeField] RectTransform _directionsTitle;
    [SerializeField] TMP_Text _notesText;
    [SerializeField] RectTransform _notesBox;
    [SerializeField] RectTransform _notesTitle;
    [Space]
    [SerializeField] GameObject _utilityMenu;
    [SerializeField] Image _utilityButton;

    string _content;
    System.UInt32 _id;

    private void Start()
    {
        Deactivate();
    }

    public void Activate(System.UInt32 id)
    {
        _loading.SetActive(true);
        _utilityButton.gameObject.SetActive(true);
        string path = Manifest.k_repositoryRoot + id.ToString("X8") + "/recipe.yaml";
        _id = id;
        Manifest.instance.viewingOptions.useOz = (Manifest.instance.entries[id].category == Category.Cocktail);
        Load(path);
    }

    public void Deactivate()
    {
        _notesTitle.gameObject.SetActive(false);
        _notesBox.gameObject.SetActive(false);
        _loading.SetActive(false);
        _failText.SetActive(false);
        _contentAll.SetActive(false);
        _utilityMenu.SetActive(false);
        _utilityButton.gameObject.SetActive(false);
        _utilityButton.color = _inactiveColor;
    }

    public void ToggleUtilityMenu()
    {
        _utilityMenu.SetActive(!_utilityMenu.activeSelf);
        if (_utilityMenu.activeSelf)
        {
            _utilityButton.color = _activeColor;
            _utilityButton.GetComponentInChildren<TMP_Text>().fontStyle |= FontStyles.Bold;
        }
        else
        {
            _utilityButton.color = _inactiveColor;
            _utilityButton.GetComponentInChildren<TMP_Text>().fontStyle &= ~FontStyles.Bold;
        }
    }

    public void Load(string url)
    {
        StartCoroutine(GetRequest(url));
    }

    private IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            //webRequest.SetRequestHeader("Cache-Control", "no-cache");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Request error: {webRequest.error}");
                OnLoadComplete(false);
            }
            else
            {
                _content = webRequest.downloadHandler.text;
                OnLoadComplete(true);
            }
        }
    }

    public void Refresh()
    {
        if (_id == 0)
            return;

        OnLoadComplete(true);
    }

    void OnLoadComplete(bool success)
    {
        _loading.SetActive(false);
        if (!success)
        {
            _failText.SetActive(true);
            return;
        }

        _recipeTitle.text = Manifest.instance.entries[_id].name;

        YamlStream yamlStream = new YamlStream();
        using (TextReader reader = new StringReader(_content))
        {
            yamlStream.Load(reader);
        }

        YamlMappingNode root = (YamlMappingNode)yamlStream.Documents[0].RootNode;

        float blockOffset = Mathf.Abs(_directionsTitle.localPosition.y)
            - (Mathf.Abs(_ingredientBox.localPosition.y) + _ingredientBox.sizeDelta.y);

        YamlSequenceNode ingrNode = (YamlSequenceNode)root["ingredients"];
        LoadIngredients(ingrNode);
        float textHeight = _ingredientText.preferredHeight;
        _ingredientBox.sizeDelta = new Vector2(_ingredientBox.sizeDelta.x, textHeight + _backgroundPadding * 2);

        float moveDelta = (Mathf.Abs(_ingredientBox.localPosition.y) + _ingredientBox.sizeDelta.y + blockOffset) - Mathf.Abs(_directionsTitle.localPosition.y);
        _directionsTitle.localPosition += Vector3.down * moveDelta;
        _directionsBox.localPosition += Vector3.down * moveDelta;

        YamlSequenceNode dirNode = (YamlSequenceNode)root["directions"];
        LoadDirections(dirNode);
        textHeight = _directionsText.preferredHeight;
        _directionsBox.sizeDelta = new Vector2(_directionsBox.sizeDelta.x, textHeight + _backgroundPadding * 2);

        RectTransform lastElement = _directionsBox;

        if (root.Children.ContainsKey("notes")) // Notes is empty
        {
            _notesTitle.gameObject.SetActive(true);
            _notesBox.gameObject.SetActive(true);

            moveDelta = (Mathf.Abs(_directionsBox.localPosition.y) + _directionsBox.sizeDelta.y + blockOffset) - Mathf.Abs(_notesTitle.localPosition.y);
            _notesTitle.localPosition += Vector3.down * moveDelta;
            _notesBox.localPosition += Vector3.down * moveDelta;

            YamlScalarNode notesNode = (YamlScalarNode)root["notes"];
            LoadNotes(notesNode);
            textHeight = _notesText.preferredHeight;
            _notesBox.sizeDelta = new Vector2(_notesBox.sizeDelta.x, textHeight + _backgroundPadding * 2);

            lastElement = _notesBox;
        }

        float scrollHeight = Mathf.Abs(lastElement.localPosition.y) + lastElement.sizeDelta.y + _backgroundPadding * 2;
        _scrollContent.sizeDelta = new Vector2(_scrollContent.sizeDelta.x, scrollHeight);

        _contentAll.gameObject.SetActive(true);
    }

    private void LoadIngredients(YamlSequenceNode ingrNode)
    {
        _ingredientText.text = "";
        IngredientFormatter formatter = new();

        for (int i = 0; i < ingrNode.Children.Count; ++i)
        {
            string line = "- ";

            YamlMappingNode current = (YamlMappingNode)ingrNode[i];
            string type = current["type"].ToString();
            float amount = float.Parse(current["amount"].ToString()) * Manifest.instance.viewingOptions.quantity;

            if (type == "Mass")
            {
                line += (Manifest.instance.viewingOptions.massImperial) ?
                    formatter.ImperialMass(amount, true) :
                    formatter.MetricMass(amount);

                line += " ";
            }
            else if (type == "Volume")
            {
                line += (Manifest.instance.viewingOptions.volumeImperial) ?
                    formatter.ImperialVolume(amount, Manifest.instance.viewingOptions.useOz, true) :
                    formatter.MetricVolume(amount);

                line += " ";
            }
            else if(amount > 0f)
            {
                line += amount.ToString() + " ";
            }

            line += current["name"].ToString();

            if ( type != "Volume" && type != "Mass" && type != "")
            {
                line += " " + type;
            }

            string note = "";
            if (current.Children.ContainsKey("note"))
            {
                note = current["note"].ToString();
            }

            if (note != "")
            {
                if (note[0] == '(')
                {
                    line += " " + note;
                }
                else
                {
                    line += ", " + note;
                }
            }
            _ingredientText.text += line + "\n";
        }
        _ingredientText.text.TrimEnd('\n');
    }

    private void LoadDirections(YamlSequenceNode dirNode)
    {
        _directionsText.text = "";
        for (int i = 0; i < dirNode.Children.Count; ++i)
        {
            _directionsText.text += (i + 1).ToString() + ".   ";
            string dir = dirNode[i].ToString();
            dir = dir.Replace(" deg-", "°");
            _directionsText.text += dir + "\n";
        }
        _directionsText.text.TrimEnd('\n');
    }

    private void LoadNotes(YamlScalarNode notesNode)
    {
        _notesText.text = notesNode.ToString();
    }
}
