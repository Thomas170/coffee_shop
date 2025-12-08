using Unity.Netcode;
using UnityEngine;

public class StepManager : NetworkBehaviour
{
    public static StepManager Instance;
    
    public TutorialStep CurrentStep => _currentStep.Value;
    private readonly NetworkVariable<TutorialStep> _currentStep = new();
    
    protected virtual void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _currentStep.OnValueChanged += OnCurrentStepChanged;
    }

    public override void OnNetworkDespawn()
    {
        _currentStep.OnValueChanged -= OnCurrentStepChanged;
        base.OnNetworkDespawn();
    }
    
    private void SetCurrentStep(TutorialStep step)
    {
        if (NetworkManager.Singleton.IsServer) _currentStep.Value = step;
    }
    
    private void OnCurrentStepChanged(TutorialStep oldValue, TutorialStep newValue)
    {
        if (oldValue == newValue) return;
        
        StepScenario stepScenario = TutorialScenario.Instance.Scenario
            .Find(s => s.Step == newValue);

        if (stepScenario == null) return;
        
        if (stepScenario.StepDialogs != null)
            DialogueManager.Instance.StartDialogue(stepScenario.StepDialogs);
        
        if (stepScenario.PointerStepTarget)
            TutorialManager.Instance.SetPointer(stepScenario.PointerStepTarget);
        
        if (stepScenario.StepPopup)
            PopupTips.Instance.OpenPopup(stepScenario.StepPopup);

        if (stepScenario.RobotStepTarget)
            RobotController.Instance.MoveTo(stepScenario.RobotStepTarget);
    }
    
    public void ValidStep(TutorialStep step)
    {
        Debug.Log("Step " + CurrentStep + " " + step);
        if (step == CurrentStep)
        {
            SetCurrentStep(CurrentStep + 1);
        }
    }
}
