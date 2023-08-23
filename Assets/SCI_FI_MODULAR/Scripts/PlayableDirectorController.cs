using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;
using UnityStandardAssets.Characters.FirstPerson;

namespace SF.ModularBase
{
    [RequireComponent(typeof(PlayableDirector))]
    public class PlayableDirectorController : MonoBehaviour
    {
        public bool teleportPlayer;
        public float lerpSpeed = 1f;
        public AnimationCurve lerpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public LayerMask ground = ~0;

        private float lerpProgress;
        private readonly string playerTag = "Player";
        private PlayableDirector playableDirector;
        private Camera cameraPlayer;
        private Camera cameraAnim;
        private Quaternion cameraRotationStart;
        private Quaternion cameraRotationEnd;
        private Vector3 cameraPositionStart;
        private Vector3 cameraPositionEnd;
        private bool stopped;
        private bool isRunning;
        private GameObject playerObject;
        private Vector3 defaultCameraLocalPosition;
        private SkinnedMeshRenderer skinnedMesh;

        private void Awake()
        {
            playableDirector = GetComponent<PlayableDirector>();
            cameraAnim = GetComponentInChildren<Camera>();
            skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();

            playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                cameraPlayer = playerObject.GetComponentInChildren<Camera>();
                cameraAnim.enabled = false;
                var audioListener = cameraAnim.GetComponent<AudioListener>();
                if (audioListener)
                {
                    Destroy(audioListener);
                }
                var firstPersonController = playerObject.GetComponent<FirstPersonController>();
                if (firstPersonController != null)
                {
                    defaultCameraLocalPosition = playerObject.GetComponent<FirstPersonController>().OriginalCameraPosition;
                }
            }
        }

        private void OnEnable()
        {
            playableDirector.played += OnPlayed;
            playableDirector.stopped += OnStopped;
            playableDirector.Play();
        }

        private void OnDisable()
        {
            playableDirector.played -= OnPlayed;
            playableDirector.stopped -= OnStopped;

            EnablePlayer();
        }

        private void SetActiveSkinnedMesh(bool isActive)
        {
            if (skinnedMesh != null)
            {
                skinnedMesh.enabled = isActive;
            }
        }

        private void OnPlayed(PlayableDirector playableDirector)
        {
            if (isRunning)
            {
                return;
            }
            SetActiveSkinnedMesh(false);
            DisablePlayer();
            StartCoroutine(LerpCamera());
        }

        private void OnStopped(PlayableDirector playableDirector)
        {
            cameraPositionEnd = cameraAnim.transform.position;
            cameraRotationEnd = cameraAnim.transform.rotation;
            lerpProgress = 1f;
            if (teleportPlayer)
            {
                float groundLevel = playerObject.transform.position.y;
                //Ray ray = new Ray(cameraAnim.transform.position, Vector3.down);
                //if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, ground, QueryTriggerInteraction.Ignore))
                //{
                //    groundLevel = hit.point.y;
                //}
                playerObject.transform.position = new Vector3(cameraAnim.transform.position.x, groundLevel, cameraAnim.transform.position.z);
                // rotate player
                // obczaic rotation skrypt tool
                cameraRotationStart = cameraRotationEnd;
            }
            stopped = true;
        }

        private void DisablePlayer()
        {
            if (playerObject != null)
            {
                //playerObject.gameObject.SetActive(false);
                playerObject.GetComponent<FirstPersonController>().blockInput = true;
            }
        }

        private void EnablePlayer()
        {
            if (playerObject != null)
            {
                //playerObject.gameObject.SetActive(true);
                playerObject.GetComponent<FirstPersonController>().blockInput = false;
            }
        }

        private IEnumerator LerpCamera()
        {
            isRunning = true;
            yield return new WaitForEndOfFrame();

            playableDirector.Pause();

            cameraPositionStart = cameraPlayer.transform.position;
            cameraRotationStart = cameraPlayer.transform.rotation;
            cameraPositionEnd = cameraAnim.transform.position;
            cameraRotationEnd = cameraAnim.transform.rotation;

            while (lerpProgress <= 1)
            {
                lerpProgress += Time.deltaTime * lerpSpeed;
                if (cameraAnim != null && cameraPlayer != null)
                {
                    cameraPlayer.transform.position = Vector3.Lerp(cameraPositionStart, cameraPositionEnd, lerpCurve.Evaluate(lerpProgress));
                    cameraPlayer.transform.rotation = Quaternion.Slerp(cameraRotationStart, cameraRotationEnd, lerpCurve.Evaluate(lerpProgress));
                }
                yield return null;
            }

            SetActiveSkinnedMesh(true);
            stopped = false;
            playableDirector.Play();
            var parent = cameraPlayer.transform.parent;
            cameraPlayer.transform.SetParent(cameraAnim.transform);

            yield return new WaitUntil(() => stopped);

            SetActiveSkinnedMesh(false);
            cameraPlayer.transform.SetParent(parent);

            cameraAnim.enabled = false;
            cameraPlayer.enabled = true;

            cameraPositionEnd = cameraPlayer.transform.localPosition;
            cameraPositionStart = Vector3.zero;  //playerObject.transform.TransformPoint(defaultCameraLocalPosition);

            while (lerpProgress >= 0)
            {
                lerpProgress -= Time.deltaTime * lerpSpeed;
                if (cameraAnim != null && cameraPlayer != null)
                {
                    cameraPlayer.transform.localPosition = Vector3.Lerp(cameraPositionStart, cameraPositionEnd, lerpCurve.Evaluate(lerpProgress));
                    cameraPlayer.transform.rotation = Quaternion.Slerp(cameraRotationStart, cameraRotationEnd, lerpCurve.Evaluate(lerpProgress));

                }
                yield return null;
            }

            isRunning = false;
            gameObject.SetActive(false);
        }

        private void OnValidate()
        {
            GetComponent<PlayableDirector>().playOnAwake = false;
        }
    }
}
