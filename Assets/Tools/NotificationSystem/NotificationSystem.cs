using UnityEngine;
using System.Collections;


//! How to use the Notification system:
/*!
  First create a Notification. Then send it to the notification system.
  Example:
        Notification n = new Notification("test", TimeSpan.Zero);
        NotificationSystem.createNotification(n);

    A notification can also has its own icon:
        public Sprite SpriteUnknown; //Set in editor
        Notification n = new Notification("test", TimeSpan.Zero, s);

*/
public class NotificationSystem {

    private static NotificationControl nc;

    static NotificationSystem()
    {
        nc = GameObject.Find("UIScene/UI/NotificationCenter").GetComponent<NotificationControl>();
    }

	public static void createNotification(Notification n)
    {
        nc.createNotification(n);
    }
}
