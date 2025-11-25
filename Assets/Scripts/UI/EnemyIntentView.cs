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

        var description = BuildDescription(move);

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

    private string BuildDescription(EnemyMove move)
    {
        if (!string.IsNullOrEmpty(move.Description))
        {
            return move.Description;
        }

        switch (move.Intent)
        {
            case EnemyIntentType.Attack:
                return $"Attack {move.Power}";
            case EnemyIntentType.Defend:
                return $"Block {move.Power}";
            case EnemyIntentType.Buff:
                return string.IsNullOrEmpty(move.StatusId)
                    ? "Buff"
                    : $"Buff {move.StatusId} {move.StatusStacks}";
            case EnemyIntentType.Debuff:
                return string.IsNullOrEmpty(move.StatusId)
                    ? "Debuff"
                    : $"Debuff {move.StatusId} {move.StatusStacks}";
            default:
                return move.Intent.ToString();
        }
    }
}
