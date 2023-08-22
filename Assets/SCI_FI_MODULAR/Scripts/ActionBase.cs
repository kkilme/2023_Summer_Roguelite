using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionBase : MonoBehaviour
{
    public float delay = 0f;

    //public IEnumerator coroutine;

    //public ActionBase()
    //{
    //    coroutine = Coroutine();
    //}

    public virtual void Action()
    {
        StartCoroutine(Coroutine());
    }

    public IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(delay);
        SubAction();
    }

    public abstract void SubAction();
   
}
