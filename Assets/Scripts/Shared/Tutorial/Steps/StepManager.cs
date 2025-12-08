using Unity.Netcode;

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
        
        TutorialManager.Instance.SetPointer(null);
        
        StepScenario stepScenario = TutorialScenario.Instance.Scenario
            .Find(s => s.Step == newValue);

        if (stepScenario == null) return;
        
        stepScenario.StartStepAction?.Invoke();

        if (stepScenario.StepDialogues != null)
        {
            RobotController.Instance.bubbleDialogue.StartDialogue(stepScenario.StepDialogues, 4f, () =>
            {
                TriggerStep(stepScenario);
                stepScenario.EndStepAction?.Invoke();
            });
        }
        else
        {
            TriggerStep(stepScenario);
            stepScenario.EndStepAction?.Invoke();
        }
    }

    private void TriggerStep(StepScenario stepScenario)
    {
        if (stepScenario.PointerStepTarget)
            TutorialManager.Instance.SetPointer(stepScenario.PointerStepTarget);

        if (stepScenario.StepPopup)
        {
            PopupTips.Instance.OpenPopup(stepScenario.StepPopup);
            TutorialManager.Instance.currentPopup = stepScenario.StepPopup;
        }

        if (stepScenario.RobotStepTarget)
            RobotController.Instance.MoveTo(stepScenario.RobotStepTarget);
    }
    
    public void ValidStep(TutorialStep step)
    {
        if (step == CurrentStep)
        {
            SetCurrentStep(CurrentStep + 1);
        }
    }
}
