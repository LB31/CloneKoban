using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ManagerUI : Singleton<ManagerUI>
{
    public List<GameObject> Levels = new();
    public GameObject StartScreen;
    public GameObject Menu;
    public GameObject IngameButtons;
    [SerializeField] private GameObject PauseMenu;
    public AudioSource Audio;
    [HideInInspector] public bool isPaused;
    
    public bool IsWon { get; private set; }
    [SerializeField] private int startIndex;

    private void Start()
    {
        StartScreen.SetActive(true);
        Menu.SetActive(false);
        IngameButtons.SetActive(false);
        PauseMenu.SetActive(false);
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
        IngameButtons.SetActive(false);
        IsWon = true;
    }

    public void OnLevelStart()
    {
        Menu.SetActive(false);
        IngameButtons.SetActive(true);
        IsWon = false;
    }

    public void OnCancel()
    {
        if (IsWon)
            return;
        isPaused = !isPaused;
        PauseMenu.SetActive(isPaused);
    }

    public void OnUndo()
    {
        PlayerMover.Instance.Undo();
    }
}
