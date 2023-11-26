using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ManagerUI : MonoBehaviour
{
    public List<GameObject> Levels = new();
    public GameObject StartScreen;
    public GameObject Menu;
    public GameObject RestartButton;
    public AudioSource Audio;


    private int startIndex = 0;

    private void Start()
    {
        StartScreen.SetActive(true);
        Menu.SetActive(false);
        RestartButton.SetActive(false);

        startIndex = Map.Map.Instance.CurrentTilemap;

        ActivateLevel(-1);
    }

    public void StartGame()
    {
        StartScreen.SetActive(false);
        Menu.SetActive(false);
        RestartButton.SetActive(true);
        Audio.Play();

        ActivateLevel(startIndex);
    }

    public void NextLevel()
    {
        ActivateLevel(startIndex);
        
    }

    public void Restart()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ActivateLevel(int index)
    {
        Menu.SetActive(false);
        RestartButton.SetActive(true);
        if (Levels.Count == 0) return;

        foreach (GameObject item in Levels)
        {
            item.SetActive(false);
        }

        if (index != -1)
        {
            Levels[index].SetActive(true);

            Map.Map.Instance.PrepareMap(index);
            startIndex++;
        }
            
    }

    public void OnCancel(InputValue value)
    {
        Menu.SetActive(!Menu.activeSelf);
        RestartButton.SetActive(!Menu.activeSelf);
    }
}
