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
    
    public GameObject invisibleWallEntrance;
    
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
            StepDialogues = new[]
            {
                "Salut ! Je suis Cappu, ton assistant caféiné.",
                "Je vais t'apprendre à préparer un café parfait.",
                "Pour commencer, essaie de te déplacer et entre dans le café."
            },
            PointerStepTarget = entranceTarget,
            StepPopup = moveTuto,
            RobotStepTarget = entranceTarget,
            EndStepAction = () =>
            {
                invisibleWallEntrance.SetActive(false);
            }
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.TakeGrains,
            StepDialogues = new[]{ "Maintenant, prends des grains de café." },
            PointerStepTarget = coffeeCrateTarget,
            StepPopup = coffeeTuto,
            RobotStepTarget = robotTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.GrindGrains,
            StepDialogues = new[]{ "Super ! Broie les grains dans le moulin." },
            PointerStepTarget = grinderTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.UseCoffeeMachine1,
            StepDialogues = new[]{ "Il est temps de préparer un café avec la machine." },
            PointerStepTarget = coffeeMachineTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.TakeCup,
            StepDialogues = new[]{ "Prends une tasse propre dans le placard." },
            PointerStepTarget = dishCabinetTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.UseCoffeeMachine2,
            StepDialogues = new[]{ "Verse ton café dans la tasse." },
            PointerStepTarget = coffeeMachineTarget,
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.GiveCupClient,
            StepDialogues = new[]{ "Apporte la tasse au client." },
            PointerStepTarget = TutorialManager.Instance.tutorialClient.transform,
            StepPopup = orderTuto,
            StartStepAction = () =>
            {
                TutorialManager.Instance.SpawnTutorialClient();
            }
        });
        
        Scenario.Add(new StepScenario
        {
            Step = TutorialStep.Done,
            StepDialogues = new[]
            {
                "Bravo tu as fini le tutoriel !",
                "Maintenant tu peux préparer cafés et servir les clients."
            },
            StartStepAction = () =>
            {
                TutorialManager.Instance.FinishTutorial();
            }
        });
    }
}
