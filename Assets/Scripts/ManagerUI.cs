using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ManagerUI : Singleton<ManagerUI>
{
    public GameObject StartScreen;
    public GameObject Menu;
    public GameObject IngameButtons;
    [SerializeField] private GameObject PauseMenu;
    public AudioSource Audio;
    [HideInInspector] public bool isPaused;
    public bool IsWon { get; private set; }
    [SerializeField] private int startIndex;
    private List<GameObject> levels = new ();
    
    private void Start()
    {

        for (var i = 0; i < Map.Map.Instance.transform.childCount; i++)
        {
            levels.Add(Map.Map.Instance.transform.GetChild(i).gameObject);
        }
        
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
        
        if (Map.Map.Instance.tilemaps.Count == 0) return;

        foreach (GameObject level in levels)
        {
            level.SetActive(false);
        }

        if (index < 0) return;
        levels[index].gameObject.SetActive(true);
        PlayerMover.Instance.Clear();
        Map.Map.Instance.PrepareMap(index);
        startIndex++;

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
        Debug.Log("Undo triggered!");
        PlayerMover.Instance.Undo();
    }
}
