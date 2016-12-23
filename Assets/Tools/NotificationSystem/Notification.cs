using UnityEngine;
using System.Collections;
using System;

public class Notification {

    private string text;
    private DateTime expireDate;
    private Sprite notificationSprite;
	private DateTime creationDate;


    //! A constructor.
    /*!
      Creates a new Notification with the default icon.
      \param text The notification text 
      \param timeToLive The time to live indicates how long the notification is displayed. 
                        If the time to live is zero (TimeSpan.Zero) the notification is displayed permanently
    */
    public Notification(string text, TimeSpan timeToLive)
    {
        this.text = text;
        this.notificationSprite = null;
		this.creationDate = DateTime.Now;
        if(timeToLive == TimeSpan.Zero)
        {
            this.expireDate = DateTime.MaxValue;
        }
        else
        {
            this.expireDate = DateTime.Now + timeToLive;
        }
    }

    //! A constructor.
    /*!
      Creates a new Notification.
      \param text The notification text 
      \param timeToLive The time to live indicates how long the notification is displayed. 
                        If the time to live is zero (TimeSpan.Zero) the notification is displayed permanently
      \param notificationSprite The notifiaction icon 
    */
    public Notification(string text, TimeSpan timeToLive, Sprite notificationSprite) : this(text, timeToLive)
    {
        this.notificationSprite = notificationSprite;
    }

    public string Text
    {
        get { return text; }
        //set { text = value; }
    }

    public DateTime ExpireDate
    {
        get { return expireDate; }
    }

	public DateTime CreationDate
	{
		get { return creationDate; }
	}

    public Sprite NotificationSprite
    {
        get { return notificationSprite; }
    }




}
