using UnityEngine;
using System.Collections;

public class ActionTrigger : MonoBehaviour 
{
    public bool triggerOnce = true;
    private bool triggered = false;
    public GameObject[] objects;
    private BoxCollider mCollider;

    void Awake()
    {
        mCollider = GetComponent<BoxCollider> ();
        gameObject.layer = LayerMask.NameToLayer ("Trigger");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggered || !triggerOnce) 
        {
            if (other.gameObject.tag == "Player") 
            {
                triggered = true;
                foreach (GameObject o in objects)
                {
                    if (o)
                    {
                        ToggleObjectAction toggleAction = o.GetComponent<ToggleObjectAction>();
                        if(toggleAction && !o.activeSelf)
                        {
                            o.SetActive(true);
                            toggleAction.ignoreNext = true;
                        }
                        o.SendMessage("Action", SendMessageOptions.DontRequireReceiver);
                    }
                }
                Debug.Log("Action triggered");
            }
        }
    }

    void OnDrawGizmos()
    {
        if (mCollider == null)
            return;
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube (mCollider.center, mCollider.size);
        Gizmos.DrawIcon (transform.position, "trigger.png", false);
    }
}
