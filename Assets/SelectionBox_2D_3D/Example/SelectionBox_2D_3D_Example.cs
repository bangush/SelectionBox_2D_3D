using System.Collections.Generic;

using UnityEngine;

public class SelectionBox_2D_3D_Example : MonoBehaviour
{
    [SerializeField] SelectionBox_2D_3D selectionBox;
    [SerializeField] float rayCastDist = 100;

    [SerializeField] GameObject CubeA;
    [SerializeField] GameObject CubeB;

    List<MeshRenderer> highlightedObjects = new List<MeshRenderer>();
    void Awake()
    {
        selectionBox.OnSelectionBoxRelease = () => 
        {
            ChangeColor(highlightedObjects,Color.white);
            highlightedObjects.Clear();
        };
        selectionBox.OnSelectionBoxSelectObjects = (List<MeshRenderer> SelectedObjects) => 
        {
            ChangeColor(highlightedObjects, Color.white);
            highlightedObjects.Clear();

            highlightedObjects.AddRange(SelectedObjects);

            ChangeColor(highlightedObjects, Color.blue);

            print($"You have selected {SelectedObjects.Count} Objects");
        };
        selectionBox.OnSelectionPointObjectSelect = (MeshRenderer SelectedObject) => 
        {
            highlightedObjects.Add(SelectedObject);

            ChangeColor(highlightedObjects, Color.blue);

            print(SelectedObject.name);
        };

        List<MeshRenderer> selectables = new List<MeshRenderer>();
        if (CubeA) selectables.Add(CubeA.GetComponent<MeshRenderer>());
        if (CubeB) selectables.Add(CubeB.GetComponent<MeshRenderer>());
        selectionBox.SetSelectableObjects(selectables.ToArray());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionBox.StartSelectionBox(Input.mousePosition, rayCastDist);
        }
        else if (Input.GetMouseButton(0))
        {
            selectionBox.UpdateSelectionBox(Input.mousePosition);
        }
        else if(Input.GetMouseButtonUp(0))
        {
            selectionBox.ReleaseSelectionBox();
        }
    }
    void ChangeColor(List<MeshRenderer> meshes, Color color)
    {
        foreach(MeshRenderer meshRenderer in meshes)
        {
            meshRenderer.material.color = color;
        }
    }
}
