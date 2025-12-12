public class InteractDialogue : InteractElement
{
    public string[] dialogues;
    public string dialogueName;

    public override void Interact()
    {
        DialogueManager.Instance.StartDialogue(dialogues, dialogueName);
    }
}
