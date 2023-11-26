using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ManagerUI : Singleton<ManagerUI>
{
    public GameObject StartScreen;
    [FormerlySerializedAs("Menu")] public GameObject victoryScreen;
    public GameObject finalVictoryScreen;
    public GameObject ingameUI;
    [SerializeField] private GameObject PauseMenu;
     public bool isPaused;
    public bool IsWon { get; private set; }
    [SerializeField] private int startIndex;
    private List<GameObject> levels = new ();
    public TextMeshProUGUI movesText;
    public string[] moveTextColorCodes;
    
    private void Start()
    {

        for (var i = 0; i < Map.Maps.Instance.transform.childCount; i++)
        {
            levels.Add(Map.Maps.Instance.transform.GetChild(i).gameObject);
        }
        
        StartScreen.SetActive(true);
        victoryScreen.SetActive(false);
        ingameUI.SetActive(false);
        PauseMenu.SetActive(false);
        ActivateLevel(-1);
    }

    public void StartGame()
    {
        StartScreen.SetActive(false);
        OnLevelStart();
        ActivateLevel(startIndex);
    }

    public void NextLevel()
    {
        StartGame();
    }

    public void Restart()
    {
        Map.Maps.Instance.Reset();
        startIndex--;
        ActivateLevel(startIndex);
    }

    public void ActivateLevel(int index)
    {
        if (index >= levels.Count)
        {
            index = 0;
            finalVictoryScreen.SetActive(false);
        }
        else
        {
            victoryScreen.SetActive(false);
        }

        if (Map.Maps.Instance.tilemaps.Count == 0) return;

        foreach (GameObject level in levels)
        {
            level.SetActive(false);
        }

        if (index < 0) return;
        levels[index].gameObject.SetActive(true);
        PlayerMover.Instance.Clear();
        Map.Maps.Instance.PrepareMap(index);
        startIndex++;
        if (startIndex > levels.Count)
            startIndex = 0;
    }

    public void UpdateMoveText()
    {
        var moveHistory = PlayerMover.Instance.moveHistory;
        var newText = "Previous Moves: ";

        for (int i = 0; i < moveTextColorCodes.Length; i++)
        {
            var offset = moveTextColorCodes.Length - i;
            newText += moveTextColorCodes[i];
            newText += moveHistory.Count >= offset ? moveHistory[^offset] : "None";
            if (i < moveTextColorCodes.Length - 1)
                newText += ", ";
        }
        movesText.text = newText;
    }
    
    public void OnWin()
    {
        (startIndex < levels.Count ? victoryScreen : finalVictoryScreen).SetActive(true);
        ingameUI.SetActive(false);
        IsWon = true;
    }

    public void OnLevelStart()
    {
        victoryScreen.SetActive(false);
        ingameUI.SetActive(true);
        IsWon = false;
    }

    public void OnCancel()
    {
        if (IsWon)
            return;
        isPaused = !isPaused;
        ingameUI.SetActive(!isPaused);
        PauseMenu.SetActive(isPaused);
    }

    public void OnUndo()
    {
        Debug.Log("Undo triggered!");
        PlayerMover.Instance.Undo();
    }
}
