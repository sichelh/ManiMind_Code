public interface IDialogueUI
{
    bool IsTyping { get; }
    void Show(DialogueLine line, bool withTyping = true);
    void CompleteTyping();
}

