namespace Neuma.Core.DialogueSystem
{
    public interface IDialogueRepository
    {
        Dialogue Get(string caseId, string dialogueId);
        bool TryGet(string caseId, string dialogueId, out Dialogue? dialogue);
    }
}

