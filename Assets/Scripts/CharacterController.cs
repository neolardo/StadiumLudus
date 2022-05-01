using System.Collections;
using UnityEngine;

/// <summary>
/// Controls a character of the game.
/// </summary>
public class CharacterController : MonoBehaviour
{
    [Tooltip("The character which is controlled.")]
    public Character character;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    #region Inputs

    private void Update()
    {
        if (character.IsAlive)
        {
            HandleInputs();
        }
    }
    private void HandleInputs()
    {
        HandlePositionSetting();
        HandleAttack();
        HandleGuarding();
    }

    private void HandlePositionSetting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                // if ground is valid...
                character.NextPosition = new Vector2(hit.point.x, hit.point.z);
            }
        }
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(1))
        {
            character.TryAttack();
        }
    }

    private void HandleGuarding()
    {
        // test
        if (Input.GetKeyDown(KeyCode.Space))
        {
            character.TryStartGuarding();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            character.EndGuarding();
        }
    }

    #endregion

}
