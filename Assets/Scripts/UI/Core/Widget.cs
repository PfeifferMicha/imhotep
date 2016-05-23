using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace UI
{
    public class Widget : MonoBehaviour
    {
		public Sprite ToolIcon;
		public string uniqueWidgetName {
			get;
			private set;
		}
		public bool unique = false;

		public Widget()
		{
			uniqueWidgetName = "";
		}

        public void OnEnable()
        {
        }

		public void initialize( string name )
		{
			uniqueWidgetName = name;
		}

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void Close()
        {
            Destroy(gameObject);
            Debug.Log("Close " + gameObject.name);
        }

		public void OnDestroy()
		{
			Debug.Log("Destroyed " + gameObject.name);
			Debug.Log ("Destroyed " + uniqueWidgetName);
		}
    }
}
