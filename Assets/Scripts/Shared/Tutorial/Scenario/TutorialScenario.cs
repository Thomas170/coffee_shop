using System.Collections.Generic;
using UnityEngine;

public class TutorialScenario : MonoBehaviour
{
    public static TutorialScenario Instance;
    
    public Transform entranceTarget;
    public Transform coffeeCrateTarget;
    public Transform grinderTarget;
    public Transform coffeeMachineTarget;
    public Transform dishCabinetTarget;
    public Transform robotTarget;
    public Transform robotSpawnTuto;
    
    public Sprite moveTuto;
    public Sprite coffeeTuto;
    public Sprite orderTuto;

    public readonly List<StepScenario> Scenario = new();
        
    protected virtual void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        InitScenario();
    }

    private void InitScenario()
    {
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.EnterCafe,
            StepDialogs = new[]
            {
                "Salut ! Je suis Cappu, ton assistant caféiné.",
                "Je vais t'apprendre à préparer un café parfait.",
                "Pour commencer, essaie de te déplacer et entre dans le café."
            },
            PointerStepTarget = entranceTarget,
            StepPopup = moveTuto,
            RobotStepTarget = entranceTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.TakeGrains,
            StepDialogs = new[]{ "Maintenant, prends des grains de café." },
            PointerStepTarget = coffeeCrateTarget,
            StepPopup = coffeeTuto,
            RobotStepTarget = robotTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.GrindGrains,
            StepDialogs = new[]{ "Super ! Broie les grains dans le moulin." },
            PointerStepTarget = grinderTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.UseCoffeeMachine1,
            StepDialogs = new[]{ "Il est temps de préparer un café avec la machine." },
            PointerStepTarget = coffeeMachineTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.TakeCup,
            StepDialogs = new[]{ "Prends une tasse propre dans le placard." },
            PointerStepTarget = dishCabinetTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.UseCoffeeMachine2,
            StepDialogs = new[]{ "Verse ton café dans la tasse." },
            PointerStepTarget = coffeeMachineTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.GiveCupClient,
            StepDialogs = new[]{ "Apporte la tasse au client." },
            PointerStepTarget = TutorialManager.Instance.tutorialClient.transform,
            StepPopup = orderTuto,
            StepAction = () =>
            {
                TutorialManager.Instance.SpawnTutorialClient();
            }
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.Done,
            StepDialogs = new[]
            {
                "Bravo tu as fini le tutoriel !",
                "Maintenant tu peux t'occuper du café et servir les clients."
            },
            StepAction = () =>
            {
                TutorialManager.Instance.FinishTutorial();
            }
        });
    }
}
