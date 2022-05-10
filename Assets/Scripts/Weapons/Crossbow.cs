using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the crossbow weapon.
/// </summary>
public class Crossbow : MonoBehaviour
{
    private Animator animator;
    [SerializeField]
    private GameObject handOfCharacter;
    [SerializeField]
    private GameObject character;
    private const string AnimatorReload = "Reload";
    private const string AnimatorAttack = "Attack";
    private const string AnimatorDieFront = "DieFront";
    private const string AnimatorDieBack = "DieBack";
    void Start()
    {
        animator = GetComponent<Animator>();
        gameObject.transform.parent = handOfCharacter.transform;
    }

    public void AnimateDeath(HitDirection direction)
    {
        gameObject.transform.parent = character.transform;
        animator.SetTrigger(direction == HitDirection.Back ? AnimatorDieBack : AnimatorDieFront);
    }
    public void AnimateReload()
    {
        animator.SetTrigger(AnimatorReload);
    }
    public void AnimateAttack()
    {
        animator.SetTrigger(AnimatorAttack);
    }

}
