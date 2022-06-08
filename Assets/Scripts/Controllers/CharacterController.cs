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

    private bool continueToTriggerLeftMouseButtonDown;

    [Tooltip("The key which should be pressed to trigger the first skill of the character.")]
    [SerializeField]
    private KeyCode firstSkillKeyCode = KeyCode.Q;

    [Tooltip("The key which should be pressed to trigger the first skill of the character.")]
    [SerializeField]
    private KeyCode secondSkillKeyCode = KeyCode.W;

    [Tooltip("The key which should be pressed to trigger the first skill of the character.")]
    [SerializeField]
    private KeyCode thirdSkillKeyCode = KeyCode.E;

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

    /// <summary>
    /// Handles mouse events. The character should attack on left click or continue attacking if the click started an attack. 
    /// Otherwise it should move to the left click's position and guard while the right mouse button is held down.
    /// </summary>
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
        if (Input.GetMouseButtonUp(0))
        {
            continueToTriggerLeftMouseButtonDown = false;
        }
        bool isLeftMouseButtonDown = Input.GetMouseButtonDown(0) || continueToTriggerLeftMouseButtonDown;
        bool isLeftMouseButton = Input.GetMouseButton(0);
        bool isRightMouseButton = Input.GetMouseButton(1);
        if (isLeftMouseButtonDown || isLeftMouseButton || isRightMouseButton)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var layerMask = isLeftMouseButtonDown ? (1 << Globals.GroundLayer) | (1 << Globals.CharacterLayer) | (1 << Globals.InteractableLayer): (1 << Globals.GroundLayer);
            if (Physics.Raycast(ray, out hit, 20, layerMask, QueryTriggerInteraction.Collide))
            {
                bool interactableAtHit = hit.transform.gameObject.layer == Globals.InteractableLayer;
                bool enemyAtHit = hit.transform.gameObject.layer == Globals.CharacterLayer && character.gameObject != hit.transform.gameObject;
                if ((isLeftMouseButtonDown || isLeftMouseButton) && Input.GetKey(KeyCode.LeftShift))
                {
                    character.TryAttack(hit.point);
                    continueToTriggerLeftMouseButtonDown = true;
                }
                else if (isLeftMouseButtonDown && enemyAtHit)
                {
                    character.SetChaseTarget(hit.transform);
                    continueToTriggerLeftMouseButtonDown = true;
                }
                else if (isLeftMouseButtonDown && interactableAtHit)
                {
                    character.SetInteractionTarget(hit.transform.parent.GetComponent<Interactable>());
                    continueToTriggerLeftMouseButtonDown = true;
                }
                else if ((isLeftMouseButtonDown || isLeftMouseButton) && character.gameObject == hit.transform.gameObject) // self hit
                {
                    if (Physics.Raycast(ray, out hit, 20, (1 << Globals.GroundLayer)))
                    {
                        character.MoveTo(hit.point);
                        continueToTriggerLeftMouseButtonDown = false;
                    }
                }
                else if ((isLeftMouseButtonDown || isLeftMouseButton) && character.gameObject != hit.transform.gameObject)
                {
                    character.MoveTo(hit.point);
                    continueToTriggerLeftMouseButtonDown = false;
                }
                else if (isRightMouseButton && !isLeftMouseButton && !isLeftMouseButtonDown)
                {
                    character.SetRotationTarget(hit.point);
                }
            }
        }
    }

    /// <summary>
    /// Handles keyboard events. The skills of the character should be triggered using the binded keys and the position of the mouse.
    /// </summary>
    private void HandleKeyboardInputs()
    {
        if (Input.GetKeyDown(firstSkillKeyCode) || Input.GetKeyDown(secondSkillKeyCode) || Input.GetKeyDown(thirdSkillKeyCode))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 20, 1 << Globals.GroundLayer))
            {
                if (Input.GetKeyDown(firstSkillKeyCode))
                {
                    character.FireSkill(1, hit.point);
                }
                else if (Input.GetKeyDown(secondSkillKeyCode))
                {
                    character.FireSkill(2, hit.point);
                }
                else if (Input.GetKeyDown(thirdSkillKeyCode))
                {
                    character.FireSkill(3, hit.point);
                }
            }
        }
    }

    #endregion

    #endregion
}
