using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public float interactionRange = 2f;
    public float interactionRadius = 0.5f; // How wide the interaction check is
    public Transform interactionSource; // Transform to raycast from
    private IInteractable currentInteractable;
    private bool isInteracting;

    void Start()
    {
        // If no source specified, use this object's transform
        if (interactionSource == null)
            interactionSource = transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryStartInteraction();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            EndCurrentInteraction();
        }
        else if (isInteracting && currentInteractable?.RequiresContinuousInteraction == true)
        {
            currentInteractable.ContinueInteraction();
        }
    }

    void TryStartInteraction()
    {
        // Cast sphere from character position forward
        Vector3 sourcePosition = interactionSource.position + Vector3.up; // Slightly up from feet
        Vector3 sourceForward = interactionSource.forward;
        
        // Debug visualization
        Debug.DrawRay(sourcePosition, sourceForward * interactionRange, Color.yellow, 0.1f);

        // Use SphereCast for more forgiving interaction
        RaycastHit hit;
        if (Physics.SphereCast(sourcePosition, interactionRadius, sourceForward, out hit, interactionRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                isInteracting = true;
                currentInteractable.StartInteraction();
            }
        }
    }

    public void EndCurrentInteraction()
    {
        if (isInteracting && currentInteractable != null)
        {
            currentInteractable.EndInteraction();
            if (!currentInteractable.RequiresContinuousInteraction)
            {
                currentInteractable = null;
                isInteracting = false;
            }
        }
    }

    // Called by StateManager when switching forms
    public void PreserveInteraction()
    {
        // Keep interaction state but don't end it
        if (currentInteractable?.RequiresContinuousInteraction == true)
        {
            isInteracting = Input.GetKey(KeyCode.Space);
        }
    }
}
