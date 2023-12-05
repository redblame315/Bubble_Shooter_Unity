/*
 ##########################################################
 ##########################################################
 -----------------   created by David ---------------------
 -----------------   created date : 12/04/2023 ------------
 ##########################################################
 ##########################################################
*/

using UnityEngine;
using System.Collections;

public class DisableAnimator : MonoBehaviour
{

    Animator animator;
    Animation animationA;

    void Start()
    {
        animator = GetComponent<Animator>();
        animationA = gameObject.GetComponent<Animation>();
    }

    public void disableAnimator()
    {
        if (animator != null)
            animator.enabled = false;
        if (GetComponent<Animation>() != null)
            animationA.enabled = false;
    }

    public void Suicide()
    {
        Destroy(gameObject, 0.1f);
    }
}
