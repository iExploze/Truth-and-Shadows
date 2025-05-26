using UnityEngine;

namespace TruthAndShadows.Interaction
{
    public class GateSwitch : InteractableBase
    {
        [SerializeField]
        private GameObject targetGate;

        private void Start()
        {
            if (targetGate == null)
            {
                targetGate = GameObject.Find("Gate");
                if (targetGate == null)
                {
                    Debug.LogWarning("No gate found for switch: " + gameObject.name);
                }
            }
        }

        public override void StartInteraction()
        {
            if (targetGate != null)
            {
                targetGate.SetActive(false);
            }
        }
    }
}
