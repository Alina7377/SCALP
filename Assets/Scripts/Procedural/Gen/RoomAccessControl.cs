using System;
using UnityEngine;

public class RoomAccessControl : MonoBehaviour
{
    [SerializeField] private string _tagNameRoom;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private AccessCardColor requiredAccessLevel;
    [SerializeField] private bool hasPower = true; // По умолчанию комнаты имеют питание

    public event Action NoPower;

    private void Start()
    {
        Events.Instance.OnInteractGenerator += ActivatePower;
    }

    private void OnDisable()
    {
        Events.Instance.OnInteractGenerator -= ActivatePower;
    }

    private void ActivatePower()
    {
        if (!HasPower)
        {
            hasPower = true;

            // Вызов метода обновления материалов дверей
            if (gridManager != null)
            {
                gridManager.UpdateDoorMaterials();
            }
            else
            {
                Debug.LogWarning("GridManager не назначен для RoomAccessControl.");
            }
        }
    }

    public void NoNav() 
    {
        NoPower?.Invoke();
    }
    public string GetTagNameRoom()
    {
        return _tagNameRoom;
    }

    public AccessCardColor RequiredAccessLevel
    {
        get => requiredAccessLevel;
        set => requiredAccessLevel = value;
    }

    public bool HasPower
    {
        get => hasPower;
        set => hasPower = value;
    }

    public AccessCardColor GetCardColor() 
    {
        return requiredAccessLevel;
    }

   
    public void Initialize(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    public GridManager GetGrid
    {
        get { return gridManager; }
    }
}
