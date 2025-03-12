using Mirror;
using UnityEngine;

public class CustomNetworkRenderer : NetworkBehaviour
{
    private Renderer objectRenderer;

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError("Renderer component not found.");
        }
    }

    [ClientRpc]
    public void RpcToggleVisibility(bool isVisible)
    {
        if (objectRenderer != null)
        {
            objectRenderer.enabled = isVisible;
        }
    }

    [ClientRpc]
    public void RpcUpdateAppearance(Color color)
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = color;
        }
    }

    public void ToggleVisibility(bool isVisible)
    {
        if (isServer)
        {
            RpcToggleVisibility(isVisible);
        }
    }

    public void UpdateAppearance(Color color)
    {
        if (isServer)
        {
            RpcUpdateAppearance(color);
        }
    }
}
