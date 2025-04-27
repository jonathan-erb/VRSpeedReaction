using Oculus.Interaction;
using UnityEngine;

public class ButtonInteractionHandler : MonoBehaviour
{
    private ReactionGameManager _reactionGameManager;
    private InteractableColorVisual _interactableColorVisual;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find the ReactionGameManager in the scene
        _reactionGameManager = Object.FindFirstObjectByType<ReactionGameManager>();

        // Try to find the InteractableColorVisual attached to the ButtonVisual component (child)
        _interactableColorVisual = GetComponentInChildren<InteractableColorVisual>();

        if (_reactionGameManager == null || _interactableColorVisual == null)
        {
            Debug.LogError("ReactionGameManager or InteractableColorVisual not found!");
        }
    }

    // This method will be called when the button is tapped
    public void OnButtonTapped()
    {
        if (_reactionGameManager != null && _interactableColorVisual != null)
        {
            // Notify the game manager that this button was tapped
            //_reactionGameManager.OnButtonTapped(_interactableColorVisual);
        }
    }
}
