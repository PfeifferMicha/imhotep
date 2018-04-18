Building Custom Tools and Widgets
=================================

Custom Tools should go into individual subfolders in the "Tools" folder.
To start of, we strongly recommend to duplicate one of the standard tools (for example the Opacity Control) and then modifying this to fit your needs.

If you want to build your own tool (which is displayed in the list of tools after a patient is loaded), the tool needs to be a child of the "ToolScene" GameObject and needs to have a "ToolWidget" Script attached to it. If this ToolWidget is given a Tool Icon sprite, this will be displayed in the list of tools. Note that tools are automatically disabled when you run the project and are enabled when the user selects them. Note also that the ToolScene is 50 meters below the real scene. To navigate there, simply select it in the Hierarchy, then move your cursor over the Scene View and press "F" on your keyboard.

If you want to build a widget for the curved 2D screen, this needs to be a child of the UIScene/UI GameObject. Again, we recommend to copy one of our widgets to get you started. The UI Widget needs to have a "Widget" component where you can control where it should be placed on the 2D screen. Note that the UIScene is 40 meters below the real scene. To navigate there, simply select it in the Hierarchy, then move your cursor over the Scene View and press "F" on your keyboard.


Merging:
----------------------

Since Unity3D and Git sometimes do not work well together and merging a scene which has been modified by two developers fails most of the time, we recommend the following procedure when merging (until we find a better solution):

Build the tool inside your scene. Once you are satisfied and you want to merge with another branch, save the tool as a prefab inside your tool folder (drag the Tool's main game object from the Hierarchy to the tool folder in Unity). Then commit everything and attempt the merge, for example by doing:

	git pull origin master
	
If there is a merge conflict in the Workstation scene, you can now simply checkout the remote's scene:

	git checkout --theirs Assets/Workstation.unity

Then git commit to finalize the merge. Afterwards, you can re-add your tool to the scene simply by dragging it from the tool folder back to the Hierarchy in Unity.



