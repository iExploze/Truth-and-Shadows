using TruthAndShadows.Interaction;
using UnityEngine;

namespace TruthAndShadows.Interaction
{
    [RequireComponent(typeof(MeshFilter))]
    public class LeverInteractable : InteractableBase
    {
        [Header("Lever Meshes")]
        [SerializeField]
        private Mesh offMesh;

        [SerializeField]
        private Mesh onMesh;

        [SerializeField]
        private bool startOn = false;

        private MeshFilter meshFilter;
        private bool isOn;

        protected override void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            base.Start(); // Important: call base.Start() after getting meshFilter but before setting state
            SetLeverState(startOn);
        }

        public override void StartInteraction()
        {
            ToggleLever();
        }

        public void ToggleLever()
        {
            SetLeverState(!isOn);
        }

        public void SetLeverState(bool turnOn)
        {
            isOn = turnOn;
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = isOn ? onMesh : offMesh;
                // Force outline update after mesh swap
                if (outlineComponents != null)
                {
                    foreach (var outline in outlineComponents)
                    {
                        if (outline != null)
                        {
                            outline.enabled = false;
                            outline.enabled = outlineShouldBeVisible;
                        }
                    }
                }
            }
        }

        public bool IsLeverOn() => isOn;
    }
}
