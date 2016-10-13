using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


namespace UI
{
	public class Widget : MonoBehaviour
    {
		public LayoutPosition layoutPosition{ private set; get; }
		public Screen layoutScreen = Screen.center;
		public AlignmentH layoutAlignHorizontal = AlignmentH.stretch;
		public AlignmentV layoutAlignVertical = AlignmentV.stretch;
		[Range(0, 99)] public int layoutLayer = 0;

		private Vector3 activeScale = new Vector3( 1f, 1f, 1f );
		private Vector3 inactiveScale = new Vector3( 0.95f, 0.95f, 0.95f );
		private Material textMaterial = null;
		private Material imageMaterial = null;

		public void Awake()
		{
			// Make sure my canvas is centered:
			Canvas cv = GetComponentInChildren (typeof(Canvas), true) as Canvas;
			if (cv != null) {
				cv.transform.localPosition = Vector3.zero;
			}

			UpdateMaterials ();
		}

		public void UpdateMaterials()
		{
			if( textMaterial == null )
				textMaterial = new Material (Shader.Find ("Custom/TextShader"));
			if( imageMaterial == null )
				imageMaterial = new Material (Shader.Find ("Custom/UIObject"));

			// Set material for all texts:
			Component[] texts;
			texts = GetComponentsInChildren (typeof(Text), true);
			if (texts != null) {
				foreach (Text t in texts)
					t.material = textMaterial;
			}

			// Set material for all images:
			Component[] images;
			images = GetComponentsInChildren (typeof(Image), true);
			if (images != null) {
				foreach (Image i in images)
					i.material = imageMaterial;
			}
		}

		public void OnEnable()
		{
			layoutPosition = new LayoutPosition();
			layoutPosition.screen = layoutScreen;
			layoutPosition.alignHorizontal = layoutAlignHorizontal;
			layoutPosition.alignVertical = layoutAlignVertical;
			UI.Core.instance.layoutSystem.addWidget (this);
		}

		public void OnDisable()
		{
			UI.Core.instance.layoutSystem.removeWidget (this);
		}

        public void Close()
        {
            Destroy(gameObject);
        }

		public void OnDestroy()
		{
			UI.Core.instance.layoutSystem.removeWidget (this);
		}

		public void setPosition( LayoutPosition newPosition )
		{
			layoutPosition = newPosition;
			UI.Core.instance.layoutSystem.setWidgetPosition( this, newPosition );
		}

		public void highlight()
		{
			imageMaterial.SetInt ("_highlight", 1);

			//GetComponent<RectTransform> ().localScale = activeScale;
			Vector3 pos = transform.localPosition;
			pos.z = 100 - layoutLayer;
			transform.localPosition = pos;
		}

		public void unHighlight()
		{
			imageMaterial.SetInt ("_highlight", 0);

			//GetComponent<RectTransform> ().localScale = inactiveScale;
			Vector3 pos = transform.localPosition;
			pos.z = 200 - layoutLayer;
			transform.localPosition = pos;
		}
    }
}
