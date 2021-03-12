using System;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Manages a 2D Selection Box to selects 3D Objects, Works on any screen resolution
/// Requires a selectionBox UI and a selectionBoxBoundRect
/// Able To Select Multiple 3d Objects from it's Mesh/Collider bounds
/// Able To Select Single 3d Objects that has meshcolliders
/// </summary>
public class SelectionBox_2D_3D : MonoBehaviour
{
    [Header("Managed UI")]
    [SerializeField] RectTransform selectionBoxBoundRect; // Restrict SelectionBox Within the Rect Area
    [SerializeField] GameObject selectionBox2DGO; // the 2d selectionBox GO

    [Header("Managed Scene")]
    [SerializeField] Camera cam;

    [Header("Settings")]
    public bool SelectUsingColliderBounds = false;

    //Cache
    RectTransform selectionBoxRT;

    //Private
    GameObject[] selectableObjects;
    float raycastDistance;
    Vector2 startMousePos_UI;
    List<GameObject> currentlySelectedObjects = new List<GameObject>();
    Vector2 previousSelectionBoxRTSize;
    int previousSelectedObjectCount = 0;

    //Flags
    bool isSelecting = false;

    //Additional Info - remove if not needed
    enum SelectionBoxDragDirection { Left_To_Right, Right_To_Left };
    SelectionBoxDragDirection mouseDragDirection;

    //Events
    public Action<List<GameObject>> OnSelectionBoxSelectObjects;
    public Action<GameObject> OnSelectionPointObjectSelect;
    public Action OnSelectionBoxRelease;

    //Debug
    bool debug = true;

    void Awake()
    {
        selectionBoxRT = selectionBox2DGO.GetComponent<RectTransform>();
        selectionBox2DGO.transform.SetParent(selectionBoxRT.transform);

        ResetSelectionBox();
    }
    void Update()
    {
        if (isSelecting)
        {
            if (selectableObjects != null)
            {
                bool selectionBoxSizeNotZero = (selectionBoxRT.rect.width != 0 && selectionBoxRT.rect.height != 0);
                bool newSelectionBoxSize = previousSelectionBoxRTSize.x != selectionBoxRT.rect.width || previousSelectionBoxRTSize.y != selectionBoxRT.rect.height;

                // Use a Fustrum (5 Planes) projected from camera to select Objects
                if (selectionBoxSizeNotZero && newSelectionBoxSize)
                {
                    //Obtain the world pos corners of the Selection Box
                    Vector3[] corners = new Vector3[4];
                    selectionBoxRT.GetWorldCorners(corners);//bl,tl,tr,br
                    Vector3 bl = corners[0];
                    Vector3 tl = corners[1];
                    Vector3 tr = corners[2];
                    Vector3 br = corners[3];

                    //convert the position to screen point
                    //when using canvas screenspace - overlay - world pos is alraedy equals to screen point, so this conversion does nothing
                    Camera c = GetScreenPointCamera(selectionBoxBoundRect);
                    bl = RectTransformUtility.WorldToScreenPoint(c, bl);
                    tl = RectTransformUtility.WorldToScreenPoint(c, tl);
                    tr = RectTransformUtility.WorldToScreenPoint(c, tr);
                    br = RectTransformUtility.WorldToScreenPoint(c, br);

                    //C onstruct ray from the camera using selectionBox Corner Pos
                    Ray bottomLeftRay = cam.ScreenPointToRay(bl);
                    Ray topLeftRay = cam.ScreenPointToRay(tl);
                    Ray topRightRay = cam.ScreenPointToRay(tr);
                    Ray bottomRightRay = cam.ScreenPointToRay(br);

                    // Create Planes from Rays
                    Plane[] planes = new Plane[5];
                    planes[0].Set3Points(topLeftRay.GetPoint(3), bottomLeftRay.GetPoint(3), bottomRightRay.GetPoint(3)); // front
                    planes[1].Set3Points(topLeftRay.origin, topRightRay.origin, topRightRay.GetPoint(raycastDistance)); // top
                    planes[2].Set3Points(topLeftRay.origin, topLeftRay.GetPoint(raycastDistance), bottomLeftRay.GetPoint(raycastDistance)); //left
                    planes[3].Set3Points(bottomLeftRay.origin, bottomLeftRay.GetPoint(raycastDistance), bottomRightRay.GetPoint(raycastDistance));//bottom
                    planes[4].Set3Points(topRightRay.origin, bottomRightRay.GetPoint(raycastDistance), topRightRay.GetPoint(raycastDistance)); //right

                    // Check each selectable Objects to see if their mesh bounds is within the Fustrum(Planes)
                    currentlySelectedObjects.Clear();
                    foreach (GameObject selectable in selectableObjects)
                    {
                        Vector3 boundsMin;
                        Vector3 boundsMax;

                        if (SelectUsingColliderBounds)
                        {
                            Collider collider = selectable.GetComponent<Collider>();
                            boundsMin = collider.bounds.min;
                            boundsMax = collider.bounds.max;
                        }
                        else
                        {
                            MeshRenderer mesh = selectable.GetComponent<MeshRenderer>();
                            boundsMin = mesh.bounds.min;
                            boundsMax = mesh.bounds.max;
                        }

                        GeometryUtilityExtension.TestPlanesResults result = GeometryUtilityExtension.TestPlanesAABBInternalFast(planes, ref boundsMin, ref boundsMax, true);
                        if (mouseDragDirection == SelectionBoxDragDirection.Left_To_Right && result == GeometryUtilityExtension.TestPlanesResults.Inside
                            || mouseDragDirection == SelectionBoxDragDirection.Right_To_Left && result != GeometryUtilityExtension.TestPlanesResults.Outside)
                        {
                            currentlySelectedObjects.Add(selectable);
                        }
                    }

                    // Save for next update loop
                    previousSelectionBoxRTSize = new Vector2(selectionBoxRT.rect.width, selectionBoxRT.rect.height);
                    if (previousSelectedObjectCount != currentlySelectedObjects.Count)
                    {
                        // Also Invoke event whenever there's an update to the currentlySelectedObjects list
                        OnSelectionBoxSelectObjects?.Invoke(currentlySelectedObjects);
                        previousSelectedObjectCount = currentlySelectedObjects.Count;
                    }
                }
                // Select the first Object Using Raycast on mouse - not going to use input.mouse directly, instead calculate from selectionbox
                else if (!selectionBoxSizeNotZero)
                {
                    if (currentlySelectedObjects.Count == 0)
                    {
                        Vector3[] corners = new Vector3[4];
                        selectionBoxRT.GetWorldCorners(corners);//bl,tl,tr,br
                        Vector2 centerRT = (corners[0] + corners[2]) / 2;
                        Vector2 startMousePos = RectTransformUtility.WorldToScreenPoint(GetScreenPointCamera(selectionBoxBoundRect), centerRT);

                        RaycastHit hit = cam.GetFirstHitAtMouse(startMousePos, raycastDistance);
                        if (hit.collider != null)
                        {
                            GameObject SelectedObject = hit.transform.gameObject;

                            currentlySelectedObjects.Add(SelectedObject);
                            OnSelectionPointObjectSelect?.Invoke(SelectedObject);
                        }
                    }
                }
            }
        }
    }

    Camera GetScreenPointCamera(RectTransform rectTransform)
    {
        Canvas rootCanvas = null;
        Transform rectCheck = rectTransform;

        //Check Each Parent Canvas up the heirachy to try and find the root canvas
        do
        {
            rootCanvas = rectCheck.GetComponent<Canvas>();

            if (rootCanvas && !rootCanvas.isRootCanvas)
            {
                rootCanvas = null;
            }

            //Then we promote the rect we're checking to it's parent.
            rectCheck = rectTransform.parent;

        } while (rectCheck != null && rootCanvas == null);

        //Once we have found the root Canvas, we return a camera depending on it's render mode.
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }
        else if (rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            return rootCanvas.worldCamera ? rootCanvas.worldCamera : Camera.main;
        }
        else// if (rootCanvas.renderMode == RenderMode.WorldSpace)
        {
            return Camera.main;
        }
    }

    /// <summary>
    /// Set the List of Objects that user can select which ideally would only be objects visible by camera
    /// </summary>
    /// <param name="objects">List of Objects that user can select</param>
    public void SetSelectableObjects(GameObject[] objects) { selectableObjects = objects; }

    /// <summary>
    /// Call this first to init and start selection box
    /// </summary>
    /// <param name="startMousePos">The current mouse position on screen to start the selecction box from</param>
    /// <param name="raycastDist">The raycast distance used scan for objects</param>
    public void StartSelectionBox(Vector2 startMousePos, float raycastDist)
    {
        if (!isSelecting)
        {
            //Obtain mouseposition relative to the selectionBoxBoundRect
            Vector2 uiMousePostion;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(selectionBoxBoundRect, startMousePos, GetScreenPointCamera(selectionBoxBoundRect), out uiMousePostion);

            startMousePos_UI = uiMousePostion;

            raycastDistance = raycastDist;

            isSelecting = true;
        }
    }

    /// <summary>
    /// Call this to update the selection Box UI with our current Mouse Position
    /// </summary>
    /// <param name="updatedMousePos">The updated mouse position on screen to update the selecction box ui</param>
    public void UpdateSelectionBox(Vector2 updatedMousePos)
    {
        if (!isSelecting) return;
        if (!selectionBox2DGO.activeInHierarchy) selectionBox2DGO.SetActive(true);

        //Obtain mouseposition relative to the selectionBoxBoundRect
        Vector2 uiMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(selectionBoxBoundRect, updatedMousePos, GetScreenPointCamera(selectionBoxBoundRect), out uiMousePosition);

        //Calculate new SelectionBox Position And Size - using Start and End mouse position 
        Vector2 endMousePos_UI = uiMousePosition;
        Vector2 center = (startMousePos_UI + endMousePos_UI) / 2f;
        Vector2 size = new Vector2(Mathf.Abs(startMousePos_UI.x - endMousePos_UI.x),
            Mathf.Abs(startMousePos_UI.y - endMousePos_UI.y));

        //Limit new SelectionBox Position And Size To Be Within Bounds
        Rect boundRect = selectionBoxBoundRect.rect;
        float minX = (center.x - size.x / 2) - 0;
        float maxX = (center.x + size.x / 2) - boundRect.width;
        float minY = (center.y - size.y / 2) - 0;
        float maxY = (center.y + size.y / 2) - boundRect.height;

        //Limit X MinMax Pos
        if (minX < 0)
        {
            center.x -= minX / 2;
            size.x = (center.x - 0) * 2;
        }
        else if (maxX > 0)
        {
            center.x -= maxX / 2;
            size.x = (boundRect.width - center.x) * 2;
        }

        //Limit Y MinMax Pos
        if (minY < 0)
        {
            center.y -= minY / 2;
            size.y = (center.y - 0) * 2;
        }
        else if (maxY > 0)
        {
            center.y -= maxY / 2;
            size.y = (boundRect.height - center.y) * 2;
        }

        //Additional Info - Check Drag Direction 
        if (endMousePos_UI.x - startMousePos_UI.x < 0)
        {
            mouseDragDirection = SelectionBoxDragDirection.Right_To_Left;
        }
        else
        {
            mouseDragDirection = SelectionBoxDragDirection.Left_To_Right;
        }

        //Update UI
        selectionBoxRT.anchoredPosition = center;
        selectionBoxRT.sizeDelta = size;
    }

    /// <summary>
    /// Call this to end the selection Box UI
    /// </summary>
    public void ReleaseSelectionBox()
    {
        if (isSelecting)
        {
            ResetSelectionBox();

            OnSelectionBoxRelease?.Invoke();

            isSelecting = false;
        }
    }

    void ResetSelectionBox()
    {
        //Reset SelectionBox Bounds
        selectionBoxBoundRect.pivot = Vector2.zero;
        selectionBoxBoundRect.anchorMin = Vector2.zero;
        selectionBoxBoundRect.anchorMax = Vector2.one;

        //Reset SelectionBox UI
        selectionBox2DGO.SetActive(false);
        selectionBoxRT.pivot = new Vector2(0.5f, 0.5f);
        selectionBoxRT.anchorMin = Vector2.zero;
        selectionBoxRT.anchorMax = Vector2.zero;
        selectionBoxRT.sizeDelta = Vector2.zero;

        //Reset Flag
        isSelecting = false;

        //Reset Data
        currentlySelectedObjects.Clear();
        previousSelectionBoxRTSize = Vector2.zero;
        previousSelectedObjectCount = 0;

    }

    void OnDrawGizmos()
    {
        if (debug)
        {
            if (selectionBoxRT)
            {
                Gizmos.color = Color.red;

                //Obtain the world pos corners of the Selection Box
                Vector3[] corners = new Vector3[4];
                selectionBoxRT.GetWorldCorners(corners);//bl,tl,tr,br
                Vector3 bl = corners[0];
                Vector3 tl = corners[1];
                Vector3 tr = corners[2];
                Vector3 br = corners[3];

                //convert the position to screen point
                //when using canvas screenspace - overlay - world pos is alraedy equals to screen point, so this conversion does nothing
                Camera c = GetScreenPointCamera(selectionBoxBoundRect);
                bl = RectTransformUtility.WorldToScreenPoint(c, bl);
                tl = RectTransformUtility.WorldToScreenPoint(c, tl);
                tr = RectTransformUtility.WorldToScreenPoint(c, tr);
                br = RectTransformUtility.WorldToScreenPoint(c, br);

                Ray bottomLeft = cam.ScreenPointToRay(bl);
                Ray topLeft = cam.ScreenPointToRay(tl);
                Ray topRight = cam.ScreenPointToRay(tr);
                Ray bottomRight = cam.ScreenPointToRay(br);

                Gizmos.DrawRay(bottomLeft);
                Gizmos.DrawRay(topLeft);
                Gizmos.DrawRay(topRight);
                Gizmos.DrawRay(bottomRight);
            }
        }
    }
}