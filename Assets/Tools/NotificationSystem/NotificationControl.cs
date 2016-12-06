using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class NotificationControl : MonoBehaviour {

    public GameObject firstNotification;
    public GameObject secondNotification;
    public GameObject thirdNotification;

    private List<Notification> notitficationList = new List<Notification>();
    private List<GameObject> gameObjectNotificationList = new List<GameObject>();

	// Use this for initialization
	void Start () {
        firstNotification.SetActive(false);
        secondNotification.SetActive(false);
        thirdNotification.SetActive(false);
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

    public void debug()
    {
        System.Random rnd = new System.Random();
        string s = "Notification " + rnd.Next(1, 99);
        Notification n = new Notification(s, TimeSpan.FromSeconds(5));
        createNotification(n);
    }

    public void createNotification(Notification n)
    {
        notitficationList.Add(n);
        updateNotificationCenter();
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
        if (notitficationList.Count == 0)
        {
            this.gameObject.SetActive(false);
            return;
        }

        firstNotification.SetActive(false);
        secondNotification.SetActive(false);
        thirdNotification.SetActive(false);

        this.gameObject.SetActive(true);

        if(notitficationList.Count > 0)
        {
            firstNotification.SetActive(true);
            setTextAndIconInNotifiaction(firstNotification, notitficationList[0]);
        }
        if (notitficationList.Count > 1)
        {
            secondNotification.SetActive(true);
            setTextAndIconInNotifiaction(secondNotification, notitficationList[1]);
        }
        if (notitficationList.Count > 2)
        {
            thirdNotification.SetActive(true);
            setTextAndIconInNotifiaction(thirdNotification, notitficationList[2]);
        }
    }

    private void setTextAndIconInNotifiaction(GameObject notificationGameObject, Notification n)
    {
        //Set text
        notificationGameObject.GetComponentInChildren<Text>().text = n.Text;

        //Set Icon
        if (n.NotificationSprite != null)
        {
            notificationGameObject.transform.FindChild("Icon").GetComponent<Image>().sprite = n.NotificationSprite;
        }

        //Set reference to notification object
        notificationGameObject.GetComponent<NotificationReference>().notification = n;

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
