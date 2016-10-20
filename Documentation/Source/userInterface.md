User Interface
=================================

The user interface (UI) in IMHOTEP is designed in such a way that it should work with and without VR controllers.

In most 3D applications, UI Elements are glued to the camera. This is achieved by rendering them after rendering the 3D scene. However, in VR applications, this can have negative effects: The Elements can overlap objects in the scene in weird ways and the perceived distance of the object from the eyes is hard to control. Also, interaction with such elements using the controllers would be very difficult.
Instead, IMHOTEP places all UI Elements - even those which are 2D - into the 3D scene. This is achieved by using Unity's "World Space Canvas" components for all 2D UI elements.

In addition to this, the UI is split into two parts: The Main UI Mesh which is fixed in the scene and displayed around the user's position and the Tool UI:

Main UI:
---------------------------------

The main UI is made up of widgets which are placed into the "UIScene" GameObject in the Hierarchy.
This UIScene is on the "UI" layer and is ignored by the main rendering process. Instead, the UIScene contains the UICamera, which renders the UI elements once each frame. The result is placed into a RenderTexture, which is then displayed on the UIMesh (which is generated at startup) in the main scene.
This UIMesh is fixed to the platform which the user is standing on. When using the Rift, the UIMesh has the form of a cylinder which is centered around the user's chair. In case the HTC Vive (and with it, Room-Scale-VR) is used, the UIMesh is a cuboid mesh which spans the sides of the room/platform.
At runtime, the elements in the UIScene will be displayed on one of the virtual screens (currently "Left", "Center" and "Right"). These screens are automatically generated at runtime to fit the current UIMesh.

Each UI element in the UIScene should have the Canvas, Canvas Scaler, Graphic Raycaster and Widget components attached to it.
The Widget script has public attributes which control on which virtual Screen it should be placed and how it should be aligned there (vertical and horizontal alignment). As soon as a UI element is enabled, the layout system will look up these values and place/scale the UI element accordingly. Because of this, each UI Element should be scalable (i.e. when you resize the canvas in the Unity Editor using the rect transform tool, all elements inside the canvas should scale and move to fit).

Troubleshooting when UI element doesn't work:

- Make sure the layer of the element is set to "UI".
- Compare position and scale of the element to the 
- Make sure the element is centered 

(TODO: Explain how UI Elements automatically get enabled or disabled depending on the selected tool).


Tool UI:
---------------------------------

The Tool UI Scene holds all those UI elements which should be connected to the left controller when the tool is picked up.

To create a new tool:

1. Create a GameObject which is a direct child of the ToolScene. Attach a ToolWidget component to this GameObject. We'll call this GameObject the "Tool" from now on.
2. Add your tool's UI elements to the Tool: Add a GameObject with a Canvas (set it to WorldSpace) and then add Lists, Buttons etc. as Children of this canvas. You can do this again for multiple Canvases. See the other tools for a good setup. These elements will be attached to the controller whenever the tool is picked up.
3. Any Object which has a Canvas component should have a scale of 0.0025 in x, y, and z direction.
4. Any Object which has a Canvas should be rotated to 90,0,0.
5. Any Object which has a Canvas also needs a CreateBoxColliderForCanvas component attached to it.
5. Any Object which has a Canvas also needs a CanvasRaycaster component attached to it.
6. Set the Layer of the Tool and its children to be "UITool" in the inspector.

This should be enough to have a first UI ready for your tool. To add functionality, add your own script to the Tool GameObject. You can use normal Unity callbacks (Start, Update, OnEnable, OnDisable...) to program the tool's functions. Note that OnEnable is called when the tool is picked up and OnDisable is called when the tool is placed back on the ToolStand (i.e. another Tool is picked up). Initialize things you need to initialize only once in Start. Things that you need to set up when the tool is picked up should be placed in OnEnable. Also make sure to "clean up" in OnDisable.

Note: The Tool GameObject should be inactive when you start to run, otherwise it is enabled at startup (which can be nice for debugging, but remember that no patient is loaded at startup).
Note: You can skip steps 1 and 2 by simply pulling the ToolExample from Assets/Scripts/UI/Prefabs into the ToolScene (and then change the "Tool Example" to the name you want your tool to have).
Note: If there are no controllers found then the tool's UI is connected to the camera instead, acting like a helmet heads-up-display. This UI can then be controlled using the mouse.


Prefabs:
---------------------------------

The easiest way to get started with your own tool is to copy already existing tools in the project's hierarchy (for example, you can copy the "Opacity Control" in the ToolScene and the "Patient Briefing" in the UIScene.

Additionally, the project contains UI prefabs in Assets/Scripts/UI/Prefabs. The "Tool Example" is an empty tool which can be pulled into the ToolScene. The "Widget" is an empty widget which can be pulled into the UIScene/UI.
