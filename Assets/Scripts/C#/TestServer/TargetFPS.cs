using UnityEngine;

public class TargetFPS : MonoBehaviour
{
    public int target = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = target;
    }
}
