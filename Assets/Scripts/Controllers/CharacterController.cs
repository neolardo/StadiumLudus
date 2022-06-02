using System.Collections;
using UnityEngine;

/// <summary>
/// Controls a character of the game.
/// </summary>
public class CharacterController : MonoBehaviour
{
    #region Properties and Fields

    private Character character;
    private Camera mainCamera;

    #endregion

    #region Methods

    private void Start()
    {
        mainCamera = Camera.main;
        character = GetComponent<Character>();
    }

    private void Update()
    {
        if (character.IsAlive)
        {
            HandleInputs();
        }
    }

    #region Inputs

    private void HandleInputs()
    {
        HandleMouseClick();
        HandleKeyboardInputs();
    }

    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            character.StartGuarding();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            character.EndGuarding();
        }
        bool isLeftMouseButton = Input.GetMouseButton(0);
        bool isRightMouseButton = Input.GetMouseButton(1);
        if (isLeftMouseButton || isRightMouseButton)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 20, (1 << Globals.GroundLayer) | (1 << Globals.CharacterLayer)))
            {
                if (isLeftMouseButton)
                {
                    bool enemyAtHit = hit.transform.gameObject.layer == Globals.CharacterLayer && character.gameObject != hit.transform.gameObject;
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        character.TryAttack(hit.point);
                    }
                    else if (enemyAtHit)
                    {
                        character.ChaseAndAttack(hit.transform);
                    }
                    else if (character.gameObject == hit.transform.gameObject) // self hit
                    {
                        if (Physics.Raycast(ray, out hit, 20, (1 << Globals.GroundLayer)))
                        {
                            character.MoveTo(hit.point);
                        }
                    }
                    else if (character.gameObject != hit.transform.gameObject)
                    {
                        character.MoveTo(hit.point);
                    }
                }
                if (isRightMouseButton && !isLeftMouseButton)
                {
                    character.SetRotationTarget(hit.point);
                }
            }
        }
    }

    private void HandleKeyboardInputs()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 20, 1 << Globals.GroundLayer))
            {
                character.FireSkill(1, hit.point);
            }
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 20, 1 << Globals.GroundLayer))
            {
                character.FireSkill(2, hit.point);
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 20, 1 << Globals.GroundLayer))
            {
                character.FireSkill(3, hit.point);
            }
        }

    }

    #endregion

    #endregion
}
