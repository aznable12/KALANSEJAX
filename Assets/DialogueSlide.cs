using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSlide : MonoBehaviour
{
    public GameObject PanelDialogue;
    public void ShowHideDialogue()
    {
        if (PanelDialogue != null)
        {
            Animator animator = PanelDialogue.GetComponent<Animator>();
            if (animator != null)
            {
                bool isOpen = animator.GetBool("show");
                animator.SetBool("show", !isOpen);
            }
        }
    }
}

