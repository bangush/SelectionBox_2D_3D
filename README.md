# SelectionBox_2D_3D
Manages A SelectionBox UI To Select 3D Objects On Screen
 
## Features
- Restrict SelectionBox within a choosen Rect area
- Works in any Screen Resoulution
- Able to select single objects on cursor position

## Additional-Features
- 3 Event Callback - OnSelectionBoxSelect, OnSelectionBoxRelease, OnSelectionPointSelect
- Choose to select objects using [Mesh Bounds][MB] or [Collider Bounds][CB]
- Different criteria for object selection
  - Selected objects can be fully inside, outside, or intersecting the selectionbox

## Usage
1. Click and Drag over the screen to create an area to select all objects within the area
2. Click to select single objects in 3D Scene

## Implementation
1. Set a list of selectable objects using SetSelectableObjects(list)
2. Assign SelectionBoxUI and SelectionBoxArea
3. Use StartSelectionBox(),UpdateSelectionBox() and ReleaseSelectionBox()
 
## License
[MIT][L]



[L]: https://github.com/frozonnorth/SelectionBox_2D_3D/blob/main/LICENSE
[MB]: https://docs.unity3d.com/ScriptReference/Mesh-bounds.html
[CB]: https://docs.unity3d.com/ScriptReference/Collider-bounds.html
