using Unity.Netcode;
using UnityEngine;

public abstract class ManualInteractableBase : InteractableBase
{
    [SerializeField] protected float requiredHoldDuration = 10f;
    [SerializeField] protected ProgressGaugeUI gaugeUI;

    private float _holdProgress;
    private float _lastSyncTime;
    private const float SyncInterval = 0.1f;

    public void Action(bool isHolding)
    {
        if (isHolding && isInUse)
        {
            _holdProgress += Time.deltaTime;
            if (Time.time - _lastSyncTime >= SyncInterval)
            {
                SyncProgressServerRpc(_holdProgress, NetworkManager.LocalClientId);
                _lastSyncTime = Time.time;
            }
            
            gaugeUI.UpdateGaugeLocal(_holdProgress / requiredHoldDuration);

            if (_holdProgress >= requiredHoldDuration)
            {
                PlayerController player = PlayerListManager.Instance.GetPlayer(NetworkManager.LocalClientId);
                player.canMove = true;
                player.playerAnimation.SetSinkAnimationServerRpc(false);
                
                SyncActionCompleteServerRpc();
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SyncProgressServerRpc(float progress, ulong clientId) => SyncProgressClientRpc(progress, clientId);

    [ClientRpc]
    private void SyncProgressClientRpc(float progress, ulong clientId)
    {
        _holdProgress = progress;
        gaugeUI.UpdateGaugeLocal(progress / requiredHoldDuration);
    }

    protected override void StartAction()
    {
        base.StartAction();
        _holdProgress = 0f;
        gaugeUI.ShowManualGaugeServerRpc(requiredHoldDuration);
    }

    protected override void StopAction()
    {
        base.StopAction();
        _holdProgress = 0f;
        gaugeUI.HideServerRpc();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SyncActionCompleteServerRpc() => SyncActionCompleteClientRpc();

    [ClientRpc]
    private void SyncActionCompleteClientRpc()
    {
        OnActionComplete();
    }
}