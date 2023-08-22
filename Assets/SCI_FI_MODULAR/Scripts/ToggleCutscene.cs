using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleCutscene : MonoBehaviour
{
    public GameObject thePlayer;
    public GameObject cutSceneCam;

    public void Action()
    {
        cutSceneCam.SetActive(true);
        thePlayer.SetActive(false);

    }
}
