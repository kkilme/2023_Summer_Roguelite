using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SF.ModularBase
{
    public class RiffleIdleRandomizer : StateMachineBehaviour
    {
        public bool randomizeOnEnter;
        public bool randomizeOnUpdate;
        public bool randomizeOnExit;

        private static readonly int IDLE_RANDOM = Animator.StringToHash("Idle_Random");
        private int randomRange = 3;
        private float time;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            time = 0f;
            if (randomizeOnEnter)
            {
                Randomize(animator);
            }
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (randomizeOnUpdate && time < stateInfo.normalizedTime)
            {
                time++;
                Randomize(animator);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (randomizeOnExit)
            {
                Randomize(animator);
            }
        }

        private void Randomize(Animator animator)
        {
            Debug.Log("Randomized");
            var random = Random.Range(0, randomRange);
            animator.SetInteger(IDLE_RANDOM, random);
        }
    }
}
