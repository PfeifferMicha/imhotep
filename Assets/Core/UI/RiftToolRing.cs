using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*! Tool bar in Heads-Up-Display inside the Rift.
 * Used to select tools. This can also be used for developing/debugging.
 * Automatically enabled when the Rift camera is active. */
public class RiftToolRing : MonoBehaviour {
	
	public static RiftToolRing instance { private set; get; }

	public void OnEnable()
	{
		if (instance != null) {
			throw(new System.Exception ("Error: Cannot create more than one instance of RiftToolRing!"));
		}
		instance = this;

		setAvailableTools (null);
	}

	public void OnDisable()
	{
		if( this == instance )
		{
			instance = null;
		}
	}

	public void setAvailableTools (List<ToolWidget> tools)
	{
		// Disable the toolbar if there's no tools to display:
		Transform toolBar = transform.Find ("ToolBar");
		if (tools == null || tools.Count == 0) {
			toolBar.gameObject.SetActive (false);
			return;
		} else {
			toolBar.gameObject.SetActive (true);
		}

		// Get the default tool button:
		GameObject toolButton = toolBar.Find ("ToolButton").gameObject;
		float toolButtonWidth = toolButton.GetComponent<RectTransform> ().rect.width;
		// Resize the tool bar:
		RectTransform r = toolBar.GetComponent<RectTransform> ();
		r.sizeDelta = new Vector2 ((tools.Count + 1)* toolButtonWidth + 2f, r.sizeDelta.y);
		// Add an entry for each tool:
		int i = 0;
		foreach (ToolWidget tool in tools) {
			GameObject b = Instantiate (toolButton);
			b.SetActive (true);
			b.transform.SetParent (toolButton.transform.parent, false);
			RectTransform rb = b.GetComponent<RectTransform> ();
			rb.anchoredPosition = new Vector2 (1f + i * toolButtonWidth, 0f);

			Image im = b.transform.Find ("Image").GetComponent<Image> ();
			im.sprite = tool.ToolIcon;

			Button button = b.GetComponent<Button> ();
			ToolWidget captured = tool;
			button.onClick.AddListener (() => ToolControl.instance.chooseTool (captured));
			i++;
		}

		GameObject closeButton = toolBar.Find ("CloseButton").gameObject;
		RectTransform rc = closeButton.GetComponent<RectTransform> ();
		rc.anchoredPosition = new Vector2 (1f + i * toolButtonWidth, 0f);
		Button cButton = closeButton.GetComponent<Button> ();
		cButton.onClick.RemoveAllListeners ();
		cButton.onClick.AddListener (() => ToolControl.instance.closeActiveTool ());
		closeButton.SetActive (true);
	}
}
