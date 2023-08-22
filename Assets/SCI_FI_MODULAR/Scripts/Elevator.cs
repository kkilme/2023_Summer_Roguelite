using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Elevator : ActionBase
{
    enum State { STOPPED, MOVING, STOPPING};
    public enum AXIS {X, Y, Z };
    public AXIS axis = AXIS.Y;
    public float distance = 10f;
    public float speed = 1f;
    public GameObject actionTarget;
    public AudioClip startClip;
    public AudioClip endClip;
    public AudioClip loopClip;
    public AnimationCurve startCurve;
    public AnimationCurve endCurve;
    Vector3 targetPos;
    AudioSource audioSource;
    public bool action = false;

    private Vector3 basePos;
    private float epsilon = 0.05f;
    private Vector3 dir;
    // Start is called before the first frame update
    State nextDir = State.MOVING;
    State state = State.STOPPED;
    Rigidbody rbody;
    private float time;
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        rbody.useGravity = false;
        rbody.isKinematic = false;
        audioSource = GetComponent<AudioSource>();
      //  rbody.isKinematic = true;
        rbody.useGravity = false;
        basePos = transform.position;
        switch (axis)
        {
            case AXIS.X:
                targetPos = basePos + new Vector3(distance, 0f, 0f);
                break;
            case AXIS.Y:
                targetPos = basePos + Vector3.up * distance;
                break;
            case AXIS.Z:
                targetPos = basePos + Vector3.forward * distance;
                break;
        };
        dir = targetPos - basePos;
        dir.Normalize();
        epsilon = speed * 1.0f / 60f;
    }
    void SetState(State newState)
    {
        switch (newState)
        {
            case State.MOVING:
                audioSource.clip = startClip;
                audioSource.loop = false;
                audioSource.Play();
                rbody.isKinematic = false;
                break;
            case State.STOPPED:
                if (actionTarget)
                    actionTarget.SendMessage("Action", SendMessageOptions.DontRequireReceiver);
                rbody.isKinematic = true;
                transform.position = targetPos;
                targetPos = basePos;
                basePos = transform.position;
                dir = targetPos - basePos;
                dir.Normalize();
                rbody.velocity = Vector3.zero;

                break;
            case State.STOPPING:
                audioSource.clip = endClip;
                audioSource.loop = false;
                audioSource.Play();
                break;
        }
        time = 0.0f;
        state = newState;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (state)
        {
            case State.STOPPED:
                if (action == true)
                {
                    action = false;
                    Action();
                    return;
                }
                break;
            case State.MOVING:
                //reched end?
                if (Vector3.Dot((targetPos - transform.position).normalized, dir) < 0.5f)
                {
                    SetState(State.STOPPING);
                   
                    
                   
                   
                }
                else
                {
                    if (startCurve != null)
                    {
                        rbody.velocity = dir * startCurve.Evaluate(time) * speed;
                    }
                    else
                    {
                        rbody.velocity = dir * speed;
                    }
                    time += Time.fixedDeltaTime;
                    if (audioSource.clip == startClip && !audioSource.isPlaying)
                    {
                        audioSource.clip = loopClip;
                        audioSource.loop = true;
                        audioSource.Play();
                    }
                }
                break;
            case State.STOPPING:
                if (endCurve == null || time > endCurve.keys.LastOrDefault().time)
                {
                    SetState(State.STOPPED);
                    return;
                }
                
                rbody.velocity = dir * endCurve.Evaluate(time) * speed;
                time += Time.fixedDeltaTime;
                break;
        }

        
       

        

    }

    public override void SubAction()
    {
        Debug.Log("Elevator action");
        if (state == State.STOPPED)
        {
            SetState(State.MOVING);
        }
    }
}
