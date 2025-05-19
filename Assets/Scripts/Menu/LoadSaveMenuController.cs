using TMPro;

public class LoadSaveMenuController : BaseMenuController
{
    public GameSetupMenuController gameSetupMenuController;
    
    private SaveSlotData[] _slots = new SaveSlotData[3]
    {
        new SaveSlotData(true, 5),
        new SaveSlotData(true, 12),
        new SaveSlotData(false),
    };

    protected override void Start()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            var entry = menuButtons[i];
            if (!_slots[i].HasData)
            {
                entry.button.transform.Find("Empty").gameObject.SetActive(true);
                entry.button.transform.Find("Info").gameObject.SetActive(false);
                entry.button.interactable = false;
            }
            else
            {
                entry.button.transform.Find("Empty").gameObject.SetActive(false);
                var info = entry.button.transform.Find("Info");
                info.gameObject.SetActive(true);
                
                var lvlTxt = info.Find("Level").GetComponentInChildren<TextMeshProUGUI>();
                lvlTxt.text = $"{_slots[i].Level}";
                
                entry.button.interactable = true;
            }
        }
        
        base.Start();
    }

    public override void ExecuteMenuAction(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                HandleBack();
                break;
            default:
                int index = SelectedIndex;
                MenuManager.Instance.CurrentGameIndex = index;
                CloseMenu();
                gameSetupMenuController.OpenMenu();
                break;
        }
    }
}
