using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UI
{
    public class WidgetControl : MonoBehaviour
    {

        public GameObject[] AvailableWidgets;

        GameObject mScrollView;

        // Use this for initialization
        void Start()
        {
            mScrollView = transform.Find("Canvas/Scroll View").gameObject;

            // Find the default button and disable it:
            GameObject defaultWidgetButton = mScrollView.transform.Find("Viewport/Content/WidgetSelectorButton").gameObject;
            defaultWidgetButton.SetActive(false);
            
            string msg = "Available Widgets:\n";
            foreach (GameObject widget in AvailableWidgets)
            {
                // Create a new instance of the list button:
                GameObject newButton = Instantiate(defaultWidgetButton);
                newButton.SetActive(true);

                // Attach the new button to the list:
                newButton.transform.SetParent(defaultWidgetButton.transform.parent);

                // Change button text to name of tool:
                GameObject textObject = newButton.transform.Find("OverlayImage/Text").gameObject;
                Text t = textObject.GetComponent<Text>();
                t.text = widget.name;

                GameObject captured = widget;       // needed, because otherwise this is changed every iteration

                // Set the button actions:
                Button b = newButton.GetComponent<Button>();
                b.onClick.AddListener(() => HideWidgetList());
                b.onClick.AddListener(() => StartWidget(captured));

                msg += widget.name + "\n";
            }
            Debug.Log(msg);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void StartWidget(GameObject widget)
        {
            GameObject newWidget = Instantiate(widget);
            newWidget.transform.Translate(new Vector3(0, Random.value*10, 0));
            newWidget.SetActive(true);
            Debug.Log("Created: " + widget.name);
        }

        public void ShowWidgetList()
        {
            mScrollView.SetActive(true);
        }
        public void HideWidgetList()
        {
            mScrollView.SetActive(false);
        }
        
    }
}