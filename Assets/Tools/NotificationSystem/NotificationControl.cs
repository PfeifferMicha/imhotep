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
    public GameObject customAdditionalInfoIn; //For debugging
    public GameObject customAdditionalInfo = null;

    [Header("Variables for moving notification center to the viewport")]
    public bool moveNotificationCenter = true;
    public int minXPositionNotificationCenter = -650;
    public int maxXPositionNotificationCenter = 1000;

    private List<Notification> notitficationList = new List<Notification>();


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

        hideAdditionalInformation();
    }

    // Update is called once per frame
    void Update () {
        bool listChanged = deleteNotificationsIfExpired();
        if (listChanged)
        {
            hideAdditionalInformation();
            updateNotificationCenter();
        }

        //moving notification center to center of view port
        if (moveNotificationCenter)
        {
            RaycastHit hit;
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            LayerMask onlyUIMeshLayer = 100000000; // hits only the ui mesh  layer

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyUIMeshLayer)) {
                float statusbarWidth = statusBar.GetComponent<RectTransform>().rect.width;
                int newXPos = (int)(statusbarWidth * hit.textureCoord.x);
                newXPos = newXPos - (int)(statusbarWidth / 2);
                newXPos = Math.Min(newXPos, maxXPositionNotificationCenter);
                newXPos = Math.Max(newXPos, minXPositionNotificationCenter);
                this.GetComponent<RectTransform>().localPosition = new Vector2(newXPos, this.GetComponent<RectTransform>().localPosition.y);
            }
        }
    }

    public void debug()
    {
        System.Random rnd = new System.Random();
        Notification i = new Notification("Notification " + rnd.Next(1, 99), TimeSpan.Zero);
        Notification m = new Notification("Notification " + rnd.Next(1, 99), TimeSpan.Zero, null, new AdditionalInfo("test test \ntest " + rnd.Next(1, 99)));
        Notification n = new Notification("Notification " + rnd.Next(1, 99), TimeSpan.Zero, null, new AdditionalInfo(customAdditionalInfoIn));
        createNotification(m);
        createNotification(n);
        createNotification(i);
    }

    public void createNotification(Notification n)
    {
        notitficationList.Add(n);
		orderNotitficationList();
        updateNotificationCenter();
	}

	//! Convenience overload
	public void createNotification( string text, TimeSpan timeToLive, Sprite notificationSprite = null, AdditionalInfo additionalInfo = null )
	{
		Notification n = new Notification (text, timeToLive, notificationSprite, additionalInfo);
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
            setTextAndIconInNotifiaction(firstNotification, notitficationList[0], true);
        }
        if (notitficationList.Count > 1)
        {
            secondNotification.SetActive(true);
            setTextAndIconInNotifiaction(secondNotification, notitficationList[1], false);
        }
        if (notitficationList.Count > 2)
        {
            thirdNotification.SetActive(true);
            setTextAndIconInNotifiaction(thirdNotification, notitficationList[2], false);
        }
    }

    private void setTextAndIconInNotifiaction(GameObject notificationGameObject, Notification n, bool firstNotification)
    {
        //Set text
        notificationGameObject.GetComponentInChildren<Text>().text = n.Text;

        //Set Icon
        if (n.NotificationSprite != null)
        {
            notificationGameObject.transform.Find("Icon").GetComponent<Image>().sprite = n.NotificationSprite;
        }

        //Set reference to notification object
        notificationGameObject.GetComponent<NotificationReference>().notification = n;

        //Set additional info text if notification has an additional info text and is the first notification on screen
        if (firstNotification)
        {
            if(n.AdditionalInfo != null)
            {
                if (n.AdditionalInfo.Type == AdditionalInfo.AdditionalInfoType.TEXT)
                {
                    additionalInformation.GetComponentInChildren<Text>().text = n.AdditionalInfo.AdditionalInfoText;
                    customAdditionalInfo = null;
                }
                if (n.AdditionalInfo.Type == AdditionalInfo.AdditionalInfoType.CUSTOM)
                {
                    customAdditionalInfo = n.AdditionalInfo.CustomUIElement;
                }
            }
            else
            {
                hideAdditionalInformation();
            }
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
        hideAdditionalInformation();
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
        if(notitficationList.Count > 0 && notitficationList[0].AdditionalInfo != null)
        {
            if (notitficationList[0].AdditionalInfo.Type == AdditionalInfo.AdditionalInfoType.TEXT)
            {
                additionalInformation.SetActive(true);
            } else if (notitficationList[0].AdditionalInfo.Type == AdditionalInfo.AdditionalInfoType.CUSTOM)
            {
                customAdditionalInfo.SetActive(true);
            }

        }
    }

    public void hideAdditionalInformation()
    {
        additionalInformation.SetActive(false);
        if(customAdditionalInfo != null)
        {
            customAdditionalInfo.SetActive(false);
        }
    }
}
