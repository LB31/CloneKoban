using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ManagerUI : Singleton<ManagerUI>
{
    public List<GameObject> Levels = new();
    public GameObject StartScreen;
    public GameObject Menu;
    public GameObject RestartButton;
    public AudioSource Audio;

    public bool IsWon { get; private set; }
    private int startIndex = 0;

    private void Start()
    {
        StartScreen.SetActive(true);
        Menu.SetActive(false);
        RestartButton.SetActive(false);

        startIndex = Map.Map.Instance.currentTilemap;

        ActivateLevel(-1);
    }

    public void StartGame()
    {
        StartScreen.SetActive(false);
        OnLevelStart();
        Audio.Play();

        ActivateLevel(startIndex);
    }

    public void NextLevel()
    {
        StartGame();
    }

    public void Restart()
    {
        Map.Map.Instance.Reset();
        startIndex--;
        ActivateLevel(startIndex);
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
            PlayerMover.Instance.Clear();
            Map.Map.Instance.PrepareMap(index);
            startIndex++;
        }
            
    }
    
    public void OnWin()
    {
        Menu.SetActive(true);
        RestartButton.SetActive(false);
        IsWon = true;
    }

    public void OnLevelStart()
    {
        Menu.SetActive(false);
        RestartButton.SetActive(true);
        IsWon = false;
    }
}
