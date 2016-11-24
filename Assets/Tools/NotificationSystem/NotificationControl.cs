using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class NotificationControl : MonoBehaviour {

    public GameObject defaultNotification;

    private List<Notification> notitficationList = new List<Notification>();
    private List<GameObject> gameObjectNotificationList = new List<GameObject>();

	// Use this for initialization
	void Start () {
        defaultNotification.SetActive(false);
        this.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        bool listChanged = deleteNotificationsIfExpired();
        if (listChanged)
        {
            updateNotificationCenter();
        }
	}

    public void test()
    {
        System.Random rnd = new System.Random();
        Notification n = new Notification("Notification " + rnd.Next(1, 99), TimeSpan.Zero);// TimeSpan.FromSeconds(5));
        createNotification(n);
    }

    public void createNotification(Notification n)
    {
        notitficationList.Add(n);
        updateNotificationCenter();
    }

    private void createNotificationGameObject(Notification n)
    {
        // Create a new instance of the list button:
        GameObject newEntry = Instantiate(defaultNotification).gameObject;
        newEntry.SetActive(true);

        // Attach the new Entry to the list:
        newEntry.transform.SetParent(defaultNotification.transform.parent, false);

        //Set text
        newEntry.GetComponentInChildren<Text>().text = n.Text;

        //Set Icon
        if(n.NotificationSprite != null)
        {
            newEntry.transform.FindChild("Icon").GetComponent<Image>().sprite = n.NotificationSprite;
        }

        //Set reference to notification object
        newEntry.GetComponent<NotificationReference>().notification = n;

        gameObjectNotificationList.Add(newEntry);
    }

    private bool deleteNotificationsIfExpired()
    {
        bool result = false;
        for(int i = notitficationList.Count - 1; i >= 0; i--) //Foreach dont work
        {
            if(notitficationList[i].ExpireDate < DateTime.Now)
            {
                notitficationList.RemoveAt(i);
                result = true;
            }
        }
        return result;
    }

    //! Called after a notification has been removed or added
    private void updateNotificationCenter()
    {
        //Remove all notification game objects
        for (int i = gameObjectNotificationList.Count - 1; i >= 0; i--) 
        {
            Destroy(gameObjectNotificationList[i]);           
        }

        if (notitficationList.Count == 0)
        {
            this.gameObject.SetActive(false);
            return;
        }else
        {
            this.gameObject.SetActive(true);
        }

        //Create notification
        foreach(Notification n in notitficationList)
        {
            createNotificationGameObject(n);
        }
    }



    public void deleteNotoficationPressed(GameObject sender)
    {
        
        for (int i = notitficationList.Count - 1; i >= 0; i--)
        {
            if (sender.GetComponent<NotificationReference>().notification == notitficationList[i])
            {
                notitficationList.RemoveAt(i);
                break;
            }
        }
        updateNotificationCenter();
    }
}
