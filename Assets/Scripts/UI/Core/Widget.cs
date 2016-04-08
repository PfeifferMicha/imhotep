using UnityEngine;
using System.Collections;


namespace UI
{
    public class Widget : MonoBehaviour
    {
        public void OnEnable()
        {
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
    }
}
