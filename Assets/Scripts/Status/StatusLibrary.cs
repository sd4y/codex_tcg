using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TCG/Status Library")]
public class StatusLibrary : ScriptableObject
{
    [SerializeField] private List<StatusDefinition> statuses = new();

    private readonly Dictionary<string, StatusDefinition> lookup = new();

    private void OnEnable()
    {
        BuildLookup();
    }

    private void OnValidate()
    {
        BuildLookup();
    }

    public StatusDefinition GetDefinition(string statusId)
    {
        if (string.IsNullOrEmpty(statusId))
        {
            return null;
        }

        if (lookup.TryGetValue(statusId, out var definition))
        {
            return definition;
        }

        return StatusDefinition.CreateDefault(statusId);
    }

    private void BuildLookup()
    {
        lookup.Clear();
        foreach (var status in statuses)
        {
            if (status == null || string.IsNullOrEmpty(status.StatusId))
            {
                continue;
            }

            lookup[status.StatusId] = status;
        }
    }
}
