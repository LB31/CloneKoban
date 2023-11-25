using Obstacle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : Singleton<PlayerMover>
{
    public Map.Map MapReference;
    public List<Player> AllPlayers;

    public float MoveDuration = 1;
    public List<MoveDirection> PathsSoFar = new();

    private void Start()
    {
        MapReference = Map.Map.Instance;
    }

    private void OnMove(InputValue inputValue)
    {
        Debug.Log("On move called!");
        Vector2 move = inputValue.Get<Vector2>();
        Vector3 movement = Vector3.zero;

        if (move.x != 0)
            movement.x = move.x;
        else if (move.y != 0)
            movement.y = move.y;

        MoveDirection nextMove = MoveDirection.NONE;
        // Top
        if (movement.y > 0)
            nextMove = MoveDirection.TOP;
        // Down
        if (movement.y < 0)
            nextMove = MoveDirection.DOWN;
        // Right
        if (movement.x > 0)
            nextMove = MoveDirection.RIGHT;
        // Left
        if (movement.x > 0)
            nextMove = MoveDirection.LEFT;


        PathsSoFar.Add(nextMove);

        foreach (var player in AllPlayers)
        {
            player.CalcNextMove();
        }
        MapReference.MoveAllPlayers();
        //StartCoroutine(MovePlayer(movement));
    }
}

public enum MoveDirection
{
    NONE = -1,
    TOP = 0,
    RIGHT = 1,
    DOWN = 2,
    LEFT = 3
}