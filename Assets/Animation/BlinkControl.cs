using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(BlinkAndWait(Random.Range(4, 11)));
    }

    public IEnumerator BlinkAndWait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        animator.SetBool("blink", true);
        yield return new WaitForSeconds(1);
        animator.SetBool("blink", false);
        StartCoroutine(BlinkAndWait(Random.Range(4, 11)));

    }


}
