# SelectionBox_2D_3D
Manages A SelectionBox UI To Select Objects Using Mesh.Bounds or Collider
- Controls a SelectionBox UI size and position
- Restricts SelectionBox UI within bounds of another UI
 
## Features
- Works in any Screen Resoulution
- Works in any Canvas Rendering Mode - Overlay,Camera or WorldSpace
- Able to select single objects on cursor position

## Additional-Features
- 3 Event Callback - OnSelectionBoxSelect, OnSelectionBoxRelease, OnSelectionPointSelect
- Choose to select objects using [Mesh Bounds][MB] or [Collider Bounds][CB]
- Different criteria for object selection
  - Selected objects can be fully inside, outside, or intersecting the selectionbox

## Usage
1. Click and Drag over the screen to create an area to select all objects within the area
# ![SelectionBoxSelectArea](https://user-images.githubusercontent.com/5699978/110730119-de1c5b80-825a-11eb-9b24-589c032d4567.gif)
2. Click to select single objects in 3D Scene
# ![SelectionBoxSelectPoint](https://user-images.githubusercontent.com/5699978/110730125-dfe61f00-825a-11eb-9595-4bce90882b2b.gif)

## Implementation
1. Set a list of selectable objects using SetSelectableObjects(list)
2. Assign SelectionBoxUI and SelectionBoxArea
3. Use StartSelectionBox(),UpdateSelectionBox() and ReleaseSelectionBox()
 
## License
[MIT][L]

[L]: https://github.com/frozonnorth/SelectionBox_2D_3D/blob/main/LICENSE
[MB]: https://docs.unity3d.com/ScriptReference/Mesh-bounds.html
[CB]: https://docs.unity3d.com/ScriptReference/Collider-bounds.html
