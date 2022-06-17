using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages an in-game round.
/// </summary>
public class GameRoundManager : MonoBehaviour
{
    [SerializeField] private List<Character> aliveCharacters;

    public bool RoundEnded { get; private set; } = false;

    public void OnCharacterDied(Character character)
    {
        aliveCharacters.Remove(character);
        if (aliveCharacters.Count == 1)
        {
            aliveCharacters[0].OnWin();
            OnGameEnd();
        }
    }

    private void OnGameEnd()
    {
        // TODO:
        // wait for rematch requests
        // or quit if at least on of them quitted
        Debug.Log("Game round ended.");
        RoundEnded = true;
    }
}
