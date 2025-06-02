using Unity.Netcode;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int PickHash = Animator.StringToHash("Pick");
    private static readonly int DropHash = Animator.StringToHash("Drop");

    private bool _lastRunState;

    /*public void SetRunState(bool isRunning)
    {
        if (isRunning != _lastRunState)
        {
            _lastRunState = isRunning;
            SetRunStateServerRpc(isRunning);
        }
    }

    public void PlayPickAnimation()
    {
        PlayPickAnimationServerRpc();
    }

    public void PlayDropAnimation()
    {
        PlayDropAnimationServerRpc();
    }*/

    [ServerRpc(RequireOwnership = false)]
    public void SetRunStateServerRpc(bool isRunning)
    {
        SetRunStateClientRpc(isRunning);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayPickAnimationServerRpc()
    {
        PlayPickAnimationClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayDropAnimationServerRpc()
    {
        PlayDropAnimationClientRpc();
    }

    [ClientRpc]
    private void SetRunStateClientRpc(bool isRunning)
    {
        animator.SetBool(RunHash, isRunning);
    }

    [ClientRpc]
    private void PlayPickAnimationClientRpc()
    {
        animator.SetTrigger(PickHash);
    }

    [ClientRpc]
    private void PlayDropAnimationClientRpc()
    {
        animator.SetTrigger(DropHash);
    }
}