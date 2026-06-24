using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragVisual : MonoBehaviour
{
    private Image _image;
    private RectTransform _rect;

    private void EnsureInitialized()
    {
        if (_image == null) _image = GetComponent<Image>();
        if (_rect == null) _rect = GetComponent<RectTransform>();
    }

    public void Show(Sprite sprite, Vector2 position)
    {
        EnsureInitialized();
        gameObject.SetActive(true);
        _image.sprite = sprite;
        _image.enabled = sprite != null;
        _rect.position = position;
    }

    public void MoveTo(Vector2 position)
    {
        EnsureInitialized();
        _rect.position = position;
    }

    public void Hide()
    {
        EnsureInitialized();
        gameObject.SetActive(false);
        _image.sprite = null;
    }
}
