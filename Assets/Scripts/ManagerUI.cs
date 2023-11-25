using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ManagerUI : MonoBehaviour
{
    public List<GameObject> Levels = new();
    public GameObject Menu;
    public GameObject RestartButton;
    

    private int startIndex = 0;

    private void Start()
    {
        ActivateLevel(startIndex++);
        Menu.SetActive(false);
    }

    public void NextLevel()
    {
        ActivateLevel(startIndex++);
    }

    public void Restart()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ActivateLevel(int index)
    {
        if (Levels.Count == 0) return;

        foreach (GameObject item in Levels)
        {
            item.SetActive(false);
        }

        Levels[index].SetActive(true);
    }

    private void OnCancel(InputValue value)
    {
        Debug.Log(value.isPressed);
        Menu.SetActive(!Menu.activeSelf);
        RestartButton.SetActive(!Menu.activeSelf);
    }
}
