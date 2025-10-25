using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviourPun
{
    private List<BaseUnit> selectedUnits = new List<BaseUnit>();
    public LayerMask unitLayer;   // Layer for units
    public LayerMask groundLayer; // Layer for ground

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectUnit();
        }

        if (Input.GetMouseButtonDown(1))
        {
            MoveSelectedUnits();
        }
    }

    void SelectUnit()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, unitLayer);

        if (hit.collider != null)
        {
            BaseUnit unit = hit.collider.GetComponent<BaseUnit>();

            if (unit != null /*&& unit.photonView.Owner == PhotonNetwork.LocalPlayer*/)
            {
                selectedUnits.Clear();
                selectedUnits.Add(unit);
                unit.Selected.SetActive(true);

                Debug.Log($"Selected unit: {unit.name}");
            }
        }
        else
        {
            foreach( BaseUnit units in selectedUnits)
            {
                units.Selected.SetActive(false);
            }
            selectedUnits.Clear();
        }
    }

    void MoveSelectedUnits()
    {
        if (selectedUnits.Count == 0) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, groundLayer);

        if (hit.collider != null)
        {
            foreach (BaseUnit unit in selectedUnits)
            {
                unit.MoveToPosition(hit.point);
            }
        }
    }
}
