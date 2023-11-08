// Designed by Kinemation, 2023

using UnityEngine;

namespace Demo.Scripts.Runtime
{
    public class AnimEventReceiver : MonoBehaviour
    {
        [SerializeField] private FPSController controller;

        private void Start()
        {
            if (controller == null)
            {
                controller = GetComponentInParent<FPSController>();
            }
        }
        
        public void SetActionActive(int isActive)
        {
            controller.SetActionActive(isActive);
        }

        public void ChangeWeapon()
        {
            //controller.EquipWeapon();
        }

        public void RefreshStagedState()
        {
            controller.RefreshStagedState();
        }

        public void ResetStagedState()
        {
            controller.ResetStagedState();
        }
    }
}