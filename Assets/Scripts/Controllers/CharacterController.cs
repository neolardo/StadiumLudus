using Photon.Pun;
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
    private CharacterUI characterUI;
    private PhotonView photonView;
    private bool AreInputsEnabled => characterUI.IsUIVisible;
    private bool ignoreUntilRelease = false;

    [Tooltip("The key which should be pressed to trigger the first skill of the character.")]
    [SerializeField]
    private KeyCode firstSkillKeyCode = KeyCode.Q;

    [Tooltip("The key which should be pressed to trigger the first skill of the character.")]
    [SerializeField]
    private KeyCode secondSkillKeyCode = KeyCode.W;

    [Tooltip("The key which should be pressed to trigger the first skill of the character.")]
    [SerializeField]
    private KeyCode thirdSkillKeyCode = KeyCode.E;

    [Tooltip("The key which should be held to make the character sprint.")]
    [SerializeField]
    private KeyCode sprintKeyCode = KeyCode.Space;

    [Tooltip("The key which should be held to make the character guard.")]
    [SerializeField]
    private KeyCode guardKeyCode = KeyCode.LeftShift;

    private bool hasInitialized = false;

    #endregion

    #region Methods

    private void Awake()
    {
        mainCamera = Camera.main;
        character = GetComponent<Character>();
        photonView = GetComponent<PhotonView>();
    }

    public void Initialize(CharacterUI characterUI)
    {
        if (!hasInitialized)
        {
            hasInitialized = true;
            this.characterUI = characterUI;
            if (photonView.IsMine)
            {
                StartCoroutine(HandleInputsUntilAlive());
            }
        }
    }

    private IEnumerator HandleInputsUntilAlive()
    {
        while (character.IsAlive)
        {
            HandleInputs();
            yield return null;
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
        if (AreInputsEnabled)
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
                ignoreUntilRelease = false;
            }
            bool isLeftMouseButtonDown = Input.GetMouseButtonDown(0);
            bool isLeftMouseButton = Input.GetMouseButton(0);
            bool isRightMouseButton = Input.GetMouseButton(1);
            if (isLeftMouseButtonDown || isLeftMouseButton || isRightMouseButton)
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                var layerMask = isLeftMouseButtonDown ? (1 << Globals.GroundLayer) | (1 << Globals.CharacterLayer) | (1 << Globals.InteractableLayer) : (1 << Globals.GroundLayer);
                if (Physics.Raycast(ray, out hit, 20, layerMask, QueryTriggerInteraction.Collide))
                {
                    bool interactableAtHit = hit.transform.gameObject.layer == Globals.InteractableLayer;
                    bool enemyAtHit = hit.transform.gameObject.layer == Globals.CharacterLayer && character.gameObject != hit.transform.gameObject;
                    if (!ignoreUntilRelease)
                    {
                        if ((isLeftMouseButtonDown || isLeftMouseButton) && Input.GetKey(KeyCode.LeftShift))
                        {
                            character.TryAttack(hit.point);
                        }
                        else if (isLeftMouseButtonDown && enemyAtHit)
                        {
                            character.SetChaseTarget(hit.transform);
                            ignoreUntilRelease = true;
                        }
                        else if (isLeftMouseButtonDown && interactableAtHit)
                        {
                            character.SetInteractionTarget(hit.transform.parent.GetComponent<Interactable>());
                            ignoreUntilRelease = true;
                        }
                        else if ((isLeftMouseButtonDown || isLeftMouseButton) && character.gameObject == hit.transform.gameObject) // self hit
                        {
                            if (Physics.Raycast(ray, out hit, 20, (1 << Globals.GroundLayer)))
                            {
                                character.MoveTo(hit.point);
                            }
                        }
                        else if ((isLeftMouseButtonDown || isLeftMouseButton) && character.gameObject != hit.transform.gameObject)
                        {
                            character.MoveTo(hit.point);
                        }
                        else if (isRightMouseButton && !isLeftMouseButton && !isLeftMouseButtonDown)
                        {
                            character.SetRotationTarget(hit.point);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles keyboard events. The skills of the character should be triggered using the binded keys and the position of the mouse.
    /// </summary>
    private void HandleKeyboardInputs()
    {
        //skills
        if (AreInputsEnabled && (Input.GetKeyDown(firstSkillKeyCode) || Input.GetKeyDown(secondSkillKeyCode) || Input.GetKeyDown(thirdSkillKeyCode)))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 20, 1 << Globals.GroundLayer))
            {
                if (Input.GetKeyDown(firstSkillKeyCode))
                {
                    character.StartSkill(1, hit.point);
                    characterUI.ChangeSkillButtonPress(1, true);
                }
                else if (Input.GetKeyDown(secondSkillKeyCode))
                {
                    character.StartSkill(2, hit.point);
                    characterUI.ChangeSkillButtonPress(2, true);
                }
                else if (Input.GetKeyDown(thirdSkillKeyCode))
                {
                    character.StartSkill(3, hit.point);
                    characterUI.ChangeSkillButtonPress(3, true);
                }
            }
        }
        if (Input.GetKeyUp(firstSkillKeyCode))
        {
            character.EndSkill(1);
            characterUI.ChangeSkillButtonPress(1, false);

        }
        if (Input.GetKeyUp(secondSkillKeyCode))
        {
            character.EndSkill(2);
            characterUI.ChangeSkillButtonPress(2, false);
        }
        if (Input.GetKeyUp(thirdSkillKeyCode))
        {
            character.EndSkill(3);
            characterUI.ChangeSkillButtonPress(3, false);
        }
        //sprint
        if (AreInputsEnabled && Input.GetKeyDown(sprintKeyCode))
        {
            character.SetIsSprintingRequested(true);
        }
        else if(Input.GetKeyUp(sprintKeyCode))
        {
            character.SetIsSprintingRequested(false);
        }
        // pause menu
        if (character.IsAlive && !GameRoundManager.Instance.RoundEnded && Input.GetKeyDown(KeyCode.Escape))
        {
            characterUI.ShowHidePauseMenu();
        }
    }

    #endregion

    #endregion
}
