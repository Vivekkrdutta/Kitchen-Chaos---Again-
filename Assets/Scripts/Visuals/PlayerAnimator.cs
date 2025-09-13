using System;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private const string ISWALKING = "isWalking";
    private Animator animator;
    public bool isToggled { get; private set; }
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetWalkingAnimation(bool val)
    {
        isToggled = val;
        animator.SetBool(ISWALKING, val);
    }

}
