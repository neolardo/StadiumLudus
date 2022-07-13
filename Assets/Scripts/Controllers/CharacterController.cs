using Photon.Pun;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
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
    private bool ignoreEverythingUntilRelease = false;
    private bool ignoreActionsExceptAttackUntilRelease = false;
    private bool lastActionWasAttack = false;

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
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var layerMask = (1 << Globals.GroundLayer) | (1 << Globals.CharacterLayer) | (1 << Globals.InteractableLayer);
        bool isRaycastSuccessful = Physics.Raycast(ray, out hit, Globals.RaycastDistance,layerMask, QueryTriggerInteraction.Collide);
        HandleMouseInputs(hit, isRaycastSuccessful);
        HandleKeyboardInputs(hit, isRaycastSuccessful);
    }

    /// <summary>
    /// Handles mouse events. On left click the character should start an attack, interact or move.
    /// On right click, the character should start an attack. On release the character should end their attacks.
    /// </summary>
    private void HandleMouseInputs(RaycastHit hit, bool isRaycastSuccessful)
    {
        if (AreInputsEnabled)
        {
            bool isLeftMouseButton = Input.GetMouseButton(0) || Input.GetMouseButtonDown(0);
            bool isRightMouseButton = Input.GetMouseButton(1) || Input.GetMouseButtonDown(1);
            bool isLeftMouseButtonUp = !isLeftMouseButton;
            bool isRightMouseButtonUp = !isRightMouseButton;
            if (isLeftMouseButtonUp || isRightMouseButtonUp)
            {
                ignoreEverythingUntilRelease = false;
                ignoreActionsExceptAttackUntilRelease = false;
            }
            if (isRaycastSuccessful)
            {
                bool interactableAtHit = hit.transform.gameObject.layer == Globals.InteractableLayer;
                bool enemyAtHit = hit.transform.gameObject.layer == Globals.CharacterLayer || hit.transform.gameObject.layer == Globals.RigidbodyLayer;
                if (interactableAtHit || enemyAtHit)
                {
                    hit.transform.parent.GetComponent<IHighlightable>().Highlight();
                }
                if (lastActionWasAttack)
                {
                    character.SetRotationTarget(hit.point);
                    if (isLeftMouseButtonUp || isRightMouseButtonUp)
                    {
                        character.EndAttack(hit.point, enemyAtHit ? hit.transform.parent.GetComponent<Character>() : null);
                        lastActionWasAttack = false;
                    }
                }
                if ((isLeftMouseButton || isRightMouseButton) && !ignoreEverythingUntilRelease)
                {
                    if (!ignoreActionsExceptAttackUntilRelease)
                    {
                        if (enemyAtHit && (isRightMouseButton || isLeftMouseButton))
                        {
                            character.StartAttack(hit.point, hit.transform.parent.GetComponent<Character>());
                            ignoreEverythingUntilRelease = true;
                            lastActionWasAttack = true;
                        }
                        else if (isRightMouseButton)
                        {
                            character.StartAttack(hit.point);
                            ignoreEverythingUntilRelease = true;
                            lastActionWasAttack = true;
                        }
                        else if (interactableAtHit && isLeftMouseButton)
                        {
                            character.SetInteractionTarget(hit.transform.parent.GetComponent<Interactable>());
                            ignoreEverythingUntilRelease = true;
                        }
                        else if (isLeftMouseButton)
                        {
                            character.MoveTo(hit.point);
                            ignoreActionsExceptAttackUntilRelease = true;
                        }
                    }
                    else if (isRightMouseButton)
                    {
                        character.StartAttack(hit.point, enemyAtHit? hit.transform.parent.GetComponent<Character>() : null);
                        ignoreEverythingUntilRelease = true;
                        lastActionWasAttack = true;
                    }
                    else if (isLeftMouseButton)
                    {
                        character.MoveTo(hit.point);
                        ignoreActionsExceptAttackUntilRelease = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles keyboard events. The skills and the guarding should be triggered using the binded keys and the position of the mouse.
    /// </summary>
    private void HandleKeyboardInputs(RaycastHit hit, bool isRaycastSuccessful)
    {
        //skills
        if (AreInputsEnabled && isRaycastSuccessful && (Input.GetKeyDown(firstSkillKeyCode) || Input.GetKeyDown(secondSkillKeyCode) || Input.GetKeyDown(thirdSkillKeyCode)))
        {
            var target = (hit.transform.gameObject.layer == Globals.CharacterLayer || hit.transform.gameObject.layer == Globals.RigidbodyLayer) ? hit.transform.parent.GetComponent<Character>() : null;
            if (Input.GetKeyDown(firstSkillKeyCode))
            {
                character.StartSkill(1, hit.point, target);
                characterUI.ChangeSkillButtonPress(1, true);
            }
            else if (Input.GetKeyDown(secondSkillKeyCode))
            {
                character.StartSkill(2, hit.point, target);
                characterUI.ChangeSkillButtonPress(2, true);
            }
            else if (Input.GetKeyDown(thirdSkillKeyCode))
            {
                character.StartSkill(3, hit.point, target);
                characterUI.ChangeSkillButtonPress(3, true);
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
        //guard
        if (AreInputsEnabled && Input.GetKey(guardKeyCode))
        {
            character.StartGuarding();
            character.SetGuardTarget(hit.point);
        }
        else if (AreInputsEnabled)
        {
            character.EndGuarding();
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
