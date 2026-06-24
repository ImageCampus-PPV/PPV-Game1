using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [SerializeField] private string _characterSelectSceneName = "CharacterSelect";

    private void Update()
    {
        //if (Keyboard.current.escapeKey.wasPressedThisFrame)
        //    QuitOrReturnToSelect();
    }

    private void QuitOrReturnToSelect()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ReturnToCharacterSelect()
    {
        SceneManager.LoadScene(_characterSelectSceneName);
    }
}
