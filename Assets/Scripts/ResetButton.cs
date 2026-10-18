using GreenAbyss.Events;
using GreenAbyss.Services;
using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    private Button button;
    private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(OnResetGame);
    }

    void OnResetGame()
    {
        EventBus.Raise<OnRestartEvent>();
    }
}