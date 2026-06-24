using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject _pausePanel;

    [Header("Config")]
    [SerializeField] private string _characterSelectScene = "CharacterSelect";

    private bool _isPaused;

    private void Awake()
    {
        _pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void OnContinuePressed()
    {
        Resume();
    }

    public void OnOptionsPressed()
    {
        // TODO: abrir menu de opciones
    }

    public void OnQuitPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_characterSelectScene);
    }

    private void Pause()
    {
        _isPaused = true;
        _pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void Resume()
    {
        _isPaused = false;
        _pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}
