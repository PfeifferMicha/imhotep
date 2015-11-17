using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MouseUIInteraction : MonoBehaviour
{

    public Transform mouseElement;
    private List<Button> hoverList;

    private PointerEventData p = new PointerEventData(EventSystem.current);

    // Use this for initialization
    void Start()
    {
        hoverList = new List<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        List<RaycastResult> raycastResults = new List<RaycastResult>();

        // convert to a 2D position
        pointer.position = Camera.main.WorldToScreenPoint(Camera.main.transform.position + mouseElement.transform.position - Camera.main.transform.position);

        // shoot ray
        EventSystem.current.RaycastAll(pointer, raycastResults);

        // handle hits
        foreach (RaycastResult rr in raycastResults)
        {
            // hit contains a dragable ui element
            if (rr.gameObject.GetComponent<DragableUI>() != null)
            {
                rr.gameObject.GetComponent<DragableUI>().MoveElement();
            }

            // hit contains a button component
            if (rr.gameObject.GetComponent<Button>() != null)
            {
                Button b = rr.gameObject.GetComponent<Button>();

                //HOVER
                if (!hoverList.Contains(b))
                {
                    //PointerEventData p = new PointerEventData(EventSystem.current);
                    b.OnPointerEnter(p);
                    hoverList.Add(b);
                    //Debug.Log("Added button, count: " + hoverList.Count);
                }

                //BUTTON DOWN
                if (Input.GetMouseButtonDown(0))
                {
                    //PointerEventData p = new PointerEventData(EventSystem.current);
                    b.OnPointerDown(p);
                }

                //BUTTON UP
                if (Input.GetMouseButtonUp(0))
                {
                    //PointerEventData p = new PointerEventData(EventSystem.current);
                    b.OnPointerUp(p);
                    //b.OnSubmit(p); //TODO works also but what is the difference
                    b.OnPointerClick(p);
                }
            }

        }

        //TODO geht besser
        //un-hover all buttons not in raycastResults
        List<Button> deleteList = new List<Button>();
        foreach (Button b in hoverList)
        {
            bool isInRR = false;
            foreach (RaycastResult rr in raycastResults)
            {
                if(rr.gameObject.GetComponent<Button>() != null && rr.gameObject.GetComponent<Button>() == b)
                {
                    isInRR = true;
                }
            }

            if (!isInRR)
            {
                deleteList.Add(b);
            }
        }
        foreach(Button b in deleteList)
        {
            //PointerEventData p = new PointerEventData(EventSystem.current);
            b.OnPointerExit(p);
            hoverList.Remove(b);
            //Debug.Log("Removed button, count: " + hoverList.Count);
        }


    }


}
