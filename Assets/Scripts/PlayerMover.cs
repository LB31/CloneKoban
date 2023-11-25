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

    private bool moving;

    private void Start()
    {
        MapReference = Map.Map.Instance;
    }

    private void OnMove(InputValue inputValue)
    {
        //if (moving) return;

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
        if (movement.x < 0)
            nextMove = MoveDirection.LEFT;

        if (nextMove == MoveDirection.NONE) return;
        Debug.Log(movement + " " + nextMove);
        
        PathsSoFar.Add(nextMove);

        foreach (Player player in AllPlayers)
        {
            player.CalcNextMove();
        }
        MapReference.MoveAllPlayers();
        //StartCoroutine(MovePlayer(movement));
    }

    IEnumerator MovePlayer(Vector3 movement)
    {
        moving = true;
        float elapsed = 0;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + movement;

        while (elapsed < MoveDuration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / MoveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;

        moving = false;
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