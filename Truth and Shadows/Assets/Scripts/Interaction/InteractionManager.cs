using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private float interactionRadius = 0.5f;
    [SerializeField] private Transform interactionSource;

    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;

    private IInteractable currentInteractable;
    private bool isInteracting;

    void Start()
    {
        InitializeSource();
    }

    void Update()
    {
        HandleInteractionInput();
        UpdateContinuousInteraction();
    }

    private void InitializeSource()
    {
        if (interactionSource == null)
            interactionSource = transform;
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TryStartInteraction();
        else if (Input.GetKeyUp(KeyCode.Space))
            EndCurrentInteraction();
    }

    private void UpdateContinuousInteraction()
    {
        if (isInteracting && currentInteractable?.RequiresContinuousInteraction == true)
            currentInteractable.ContinueInteraction();
    }

    private void TryStartInteraction()
    {
        if (!IsValidSource()) return;

        Vector3 origin = GetInteractionOrigin();
        Vector3 direction = interactionSource.forward;

        if (showDebugRay)
            Debug.DrawRay(origin, direction * interactionRange, Color.yellow, 0.1f);

        if (TryFindInteractable(origin, direction, out IInteractable interactable))
        {
            currentInteractable = interactable;
            isInteracting = true;
            currentInteractable.StartInteraction();
        }
    }

    private bool IsValidSource()
    {
        return interactionSource != null;
    }

    private Vector3 GetInteractionOrigin()
    {
        return interactionSource.position + Vector3.up;
    }

    private bool TryFindInteractable(Vector3 origin, Vector3 direction, out IInteractable interactable)
    {
        interactable = null;
        RaycastHit hit;
        
        if (Physics.SphereCast(origin, interactionRadius, direction, out hit, interactionRange))
        {
            interactable = hit.collider.GetComponent<IInteractable>();
        }

        return interactable != null;
    }

    public void EndCurrentInteraction()
    {
        if (!isInteracting || currentInteractable == null) return;

        currentInteractable.EndInteraction();
        
        if (!currentInteractable.RequiresContinuousInteraction)
        {
            currentInteractable = null;
            isInteracting = false;
        }
    }

    public void PreserveInteraction()
    {
        if (currentInteractable?.RequiresContinuousInteraction == true)
            isInteracting = Input.GetKey(KeyCode.Space);
    }
}
