using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using YamlDotNet.RepresentationModel;

public enum Category
{
    Meal,
    Dessert,
    Cocktail
};

public class ManifestEntry
{
    public string name;
    public Category category;
    public List<string> tags;
}

[System.Serializable]
public class ViewingOptions
{
    public float quantity = 1f;
    public bool massImperial = false;
    public bool volumeImperial = true;
    public bool useOz = false;

    public bool useLargerText = false;
}

public class Manifest : MonoBehaviour
{
    public const string k_repositoryRoot = "https://raw.githubusercontent.com/jon-bogert/RecipeVault_Database/main/";

    public static Manifest instance;

    private Dictionary<System.UInt32, ManifestEntry> _entries = new();
    public Dictionary<System.UInt32, ManifestEntry> entries { get { return _entries; } }

    public ViewingOptions viewingOptions = new();

    Action<bool> _onLoadComplete;
    bool _initialized = false;
    bool _loadSuccessful = false;

    public Action<bool> textSizeChanged;

    public bool useLargerText
    {
        get { return viewingOptions.useLargerText; }
        set
        {
            textSizeChanged?.Invoke(value);
            viewingOptions.useLargerText = value;
            FindObjectOfType<RecipeViewer>().Refresh();
        }
    }

    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
            return;
        }

        instance = this;
        Load(k_repositoryRoot + "_manifest.yaml");
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
                LoadComplete(false);
            }
            else
            {
                string content = webRequest.downloadHandler.text;
                Parse(content);
                LoadComplete(true);
            }
        }
    }

    private void Parse(string content)
    {
        YamlStream yamlStream = new YamlStream();
        using (TextReader reader = new StringReader(content))
        {
            yamlStream.Load(reader);
        }

        YamlMappingNode root = (YamlMappingNode)yamlStream.Documents[0].RootNode;
        YamlSequenceNode contentNode = (YamlSequenceNode)root["content"];

        for (int i = 0; i < contentNode.Children.Count; ++i)
        {
            System.UInt32 id = Convert.ToUInt32(contentNode[i]["id"].ToString(), 16);
            ManifestEntry entry = new ManifestEntry();
            entry.name = contentNode[i]["name"].ToString();
            if (contentNode.Children.Contains("tags"))
            {
                YamlSequenceNode tags = (YamlSequenceNode)contentNode[i]["tags"];
                for (int j = 0; j < tags.Children.Count; ++j)
                {
                    entry.tags.Add(tags[j].ToString());
                }
            }
            entry.category = Enum.Parse<Category>(contentNode[i]["category"].ToString());
            _entries.Add(id, entry);
        }
    }

    public void CallOnLoadComplete(Action<bool> onLoadComplete)
    {
        if (_initialized)
        {
            onLoadComplete(_loadSuccessful);
            return;
        }

        _onLoadComplete += onLoadComplete;
    }

    private void LoadComplete(bool success)
    {
        _initialized = true;
        _loadSuccessful = success;
        if (_onLoadComplete != null)
        {
            _onLoadComplete(success);
            _onLoadComplete = null;
        }
    }
}
