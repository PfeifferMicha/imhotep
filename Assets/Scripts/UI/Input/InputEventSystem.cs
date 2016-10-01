using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ObjectInputEvent : UnityEvent<object>
{
    public int mLayer = -1;
    public InputEventSystem.Event mEvent;
}


public class InputEventSystem
{
    private List<ObjectInputEvent> mEventList;
    private static InputEventSystem mInstance = null;

    /*! All possible events: */
    public enum Event
    {
        INPUTDEVICE_LeftButtonPressed, 
        INPUTDEVICE_LeftButtonReleased 
    }

    /*! Returns the singleton instance: */
    public static InputEventSystem instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new InputEventSystem();
            }
            return mInstance;
        }
    }

    //if you want to listen to all layers, set layer = 32
    public static void startListening(Event eventType, int layer, UnityAction<object> listener)
    {
        bool foundEvent = false;
        foreach(ObjectInputEvent oie in instance.mEventList)
        {
            if(oie.mLayer == layer && oie.mEvent == eventType)
            {
                oie.AddListener(listener);
                foundEvent = true;
                break;
            }
        }

        if (!foundEvent)
        {
            ObjectInputEvent newEvent = new ObjectInputEvent();
            newEvent.mLayer = layer;
            newEvent.mEvent = eventType;
            newEvent.AddListener(listener);
            instance.mEventList.Add(newEvent);
        }

        Debug.Log("Added input event listener for event: " + eventType);
    }
    public static void stopListening(Event eventType, int layer, UnityAction<object> listener)
    {
        if (mInstance == null)
        {
            return;
        }

        foreach (ObjectInputEvent oie in instance.mEventList)
        {
            if (oie.mLayer == layer && oie.mEvent == eventType)
            {
                oie.RemoveListener(listener);
                break;
            }
        }

        Debug.Log("Removed input event listener for input event: " + eventType);
    }
    public static void triggerEventOnLayer(Event eventType, int layer, object obj = null)
    {
        foreach (ObjectInputEvent oie in instance.mEventList)
        {
            if ((oie.mLayer == layer || oie.mLayer == 32) && oie.mEvent == eventType)
            {
                oie.Invoke(obj);
                //Debug.Log("Triggering Event: " + eventType);
                break;
            }
        }
    }

    private InputEventSystem()
    {
        if (mEventList == null)
        {
            mEventList = new List<ObjectInputEvent>();
        }
    }

}




