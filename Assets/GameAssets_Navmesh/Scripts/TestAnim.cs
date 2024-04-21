using System;
using UnityEngine;

public class TestAnim : MonoBehaviour
{
    public Animator animator;
    public string animName = "Run";
    public bool RunAnim;
    public bool RunCheckHash;
    public int Hash;
    public bool ContinuePlayAnim;
    public bool ContinueCheckHash;

    private void Update()
    {
        Hash = Animator.StringToHash(animName);

        if (ContinuePlayAnim)
        {
            PlayAnim();
        }

        if (RunAnim)
        {
            RunAnim = false;
            PlayAnim();
        }

        if (RunCheckHash)
        {
            RunCheckHash = false;
            CheckHash();
        }

        if (ContinueCheckHash)
        {
            CheckHash();
        }
    }

    private void OnValidate()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAnim()
    {
        if (animator.IsInTransition(0)) return;
        animator.CrossFade(Hash, .2f);
    }

    public void CheckHash()
    {
        if (animator.IsInTransition(0))
        {
            AnimatorTransitionInfo transitionInfo = animator.GetAnimatorTransitionInfo(0);
            string name1 = animName.Equals("Fast Run") ? "Sad Idle" : "Fast Run";
            if (transitionInfo.IsName($"{name1} -> {animName}"))
            {
                Debug.Log($"Trasition {name1} -> {animName}");
            }
        }
        else
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == Hash)
            {
                Debug.Log($"shortNameHash _ {animName}");
            }
        }
    }
}