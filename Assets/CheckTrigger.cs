using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckTrigger : MonoBehaviour
{
    private bool canReact = false;
    public bool CanReact
    {
        get { return canReact; }
        set { canReact = value; }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("TalkTarget"))
        {
            canReact = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("TalkTarget"))
        {
            canReact = false;
        }
    }
}
