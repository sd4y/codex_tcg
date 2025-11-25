using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIntentView : MonoBehaviour
{
    [SerializeField] private TMP_Text intentText;
    [SerializeField] private Image intentIcon;

    private EnemyAI boundAi;

    public void Bind(EnemyAI ai)
    {
        boundAi = ai;
        Refresh();
    }

    public void Refresh()
    {
        if (boundAi == null)
        {
            return;
        }

        var move = boundAi.NextMove;
        if (move == null)
        {
            SetIntentDisplay("...");
            return;
        }

        var description = string.IsNullOrEmpty(move.Description)
            ? move.Intent.ToString()
            : move.Description;

        if (intentText != null)
        {
            intentText.SetText(description);
        }

        if (intentIcon != null)
        {
            intentIcon.sprite = move.Icon;
            intentIcon.enabled = move.Icon != null;
        }
    }

    private void SetIntentDisplay(string text)
    {
        if (intentText != null)
        {
            intentText.SetText(text);
        }

        if (intentIcon != null)
        {
            intentIcon.enabled = false;
        }
    }
}
