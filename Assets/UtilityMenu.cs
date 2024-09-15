using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UtilityMenu : MonoBehaviour
{
    [SerializeField] float _increment = 0.25f;
    [SerializeField] Color _activeColor = Color.grey;
    [SerializeField] Color _inactiveColor = Color.black;

    [Header("References")]
    [SerializeField] TMP_Text _quantityText;
    [SerializeField] Toggle _massToggle;
    [SerializeField] Toggle _volumeToggle;
    [SerializeField] Toggle _textSizeToggle;
    [SerializeField] Image _button;

    RecipeViewer _viewer;

    private void Start()
    {
        _viewer = FindObjectOfType<RecipeViewer>();

        _massToggle.isOn = Manifest.instance.viewingOptions.massImperial;
        _volumeToggle.isOn = Manifest.instance.viewingOptions.volumeImperial;
        _textSizeToggle.isOn = Manifest.instance.useLargerText;

        gameObject.SetActive(false);

        SetQuantity();
    }

    public void SetQuantity()
    {
        _quantityText.text = Manifest.instance.viewingOptions.quantity.ToString("0.0#") + "x";
    }

    public void OnMassPress(bool val)
    {
        Manifest.instance.viewingOptions.massImperial = _massToggle.isOn;
        _viewer.Refresh();
    }

    public void OnVolumePress(bool val)
    {
        Manifest.instance.viewingOptions.volumeImperial = _volumeToggle.isOn;
        _viewer.Refresh();
    }

    public void OnQuantityUp()
    {
        Manifest.instance.viewingOptions.quantity += _increment;
        SetQuantity();
        _viewer.Refresh();
    }

    public void OnQuantityDown()
    {
        Manifest.instance.viewingOptions.quantity -= _increment;
        if (Manifest.instance.viewingOptions.quantity <= 0f)
        {
            Manifest.instance.viewingOptions.quantity = 0;
        }
        SetQuantity();
        _viewer.Refresh();
    }

    public void OnTextSizeToggle()
    {
        Manifest.instance.useLargerText = _textSizeToggle.isOn;
    }

    public void ToggleMenu()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        if (gameObject.activeSelf)
        {
            _button.color = _activeColor;
            //_button.GetComponentInChildren<TMP_Text>().fontStyle |= FontStyles.Bold;
        }
        else
        {
            _button.color = _inactiveColor;
            //_button.GetComponentInChildren<TMP_Text>().fontStyle &= ~FontStyles.Bold;
        }
    }
}
