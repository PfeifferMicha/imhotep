using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

/*! Handles Notifications.
 * How to use the Notification system:
 * 		// Show notification for 10 seconds:
 *      NotificationControl.instance.createNotification("test", new TimeSpan(0,0,10));
 * A notification can also have its own icon:
 *      public Sprite SpriteUnknown; //Set in editor
 *      NotificationControl.instance.createNotification("test", new TimeSpan(0,0,10), s);
*/
public class NotificationControl : MonoBehaviour {

    public GameObject firstNotification;
    public GameObject secondNotification;
    public GameObject thirdNotification;
    public GameObject statusBar;
    public GameObject additionalInformation;

    private List<Notification> notitficationList = new List<Notification>();

    private int leftPosX = 0;
    private int centerPosX = 0;
    private int rightPosX = 0;

	public static NotificationControl instance { get; private set; }

	public NotificationControl()
	{
		if (instance != null)
			throw( new System.Exception("Cannot create NotificationControl: Only one NotificationControl may exist!" ));

		instance = this;
	}

    // Use this for initialization
    void Start () {
        firstNotification.SetActive(false);
        secondNotification.SetActive(false);
        thirdNotification.SetActive(false);
        this.gameObject.SetActive(false);

		//Place notification center in the center
        this.GetComponent<RectTransform>().localPosition = new Vector2(0, this.GetComponent<RectTransform>().localPosition.y);

        hiddeAdditionalInformation();
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
        Notification n = new Notification(s, TimeSpan.FromSeconds(10));
        createNotification(n);
    }

    public void createNotification(Notification n)
    {
        notitficationList.Add(n);
		orderNotitficationList();
        updateNotificationCenter();
	}

	//! Convenience overload
	public void createNotification( string text, TimeSpan timeToLive, Sprite notificationSprite = null )
	{
		Notification n;
		if (notificationSprite == null) {
			n = new Notification (text, timeToLive);
		} else {
			n = new Notification (text, timeToLive, notificationSprite);
		}
		createNotification (n);
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

	//! First Order by ExpireDate then order by CreationDate
	private void orderNotitficationList(){
		notitficationList.Sort((x, y) => {
			int result = x.ExpireDate.CompareTo(y.ExpireDate);
			if (result == 0) {
				result = x.CreationDate.CompareTo(y.CreationDate);
			}
			return result;
		});
	}

    public void showAdditionalInformation()
    {
        additionalInformation.SetActive(true);
    }

    public void hiddeAdditionalInformation()
    {
        additionalInformation.SetActive(false);
    }
}
