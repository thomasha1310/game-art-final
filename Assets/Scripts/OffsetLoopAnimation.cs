using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetLoopAnimation : MonoBehaviour
{

    [SerializeField] public bool randomOffset = true;
    [SerializeField] public float offset = 0f;

    private Animator animator = null;

    private void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
    }
    void Start()
    {
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);
        animator.Play(animState.fullPathHash, 0, randomOffset ? Random.value : offset);
    }

}
