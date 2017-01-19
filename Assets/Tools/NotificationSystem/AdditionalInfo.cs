using UnityEngine;
using System.Collections;

public class AdditionalInfo {

    public enum AdditionalInfoType { TEXT, CUSTOM }; //, TEXTWITH2BUTTONS, };
    private AdditionalInfoType type;
    private GameObject customUIElement;
    private string additionalInfoText;
    //private delegate yes;
    //private delegate no;

    //! Creates a default ui element above the notification with the text given in additionalInfoText
    public AdditionalInfo(string additionalInfoText)
    {
        this.type = AdditionalInfoType.TEXT;
        this.additionalInfoText = additionalInfoText;
        this.customUIElement = null;
    }

    /*
    //! Creates a default ui element with two buttons ...
    public AdditionalInfo(string additionalInfoText, methode1, methode2
    {
        this.type = AdditionalInfoType.CUSTOM;
        this.additionalInfoText = additionalInfoText;
        this.customUIElement = null;
    }*/

    //! Uses the custom ui element given in customUIElement.
    public AdditionalInfo(GameObject customUIElement)
    {
        this.type = AdditionalInfoType.CUSTOM;
        this.additionalInfoText = null;
        this.customUIElement = customUIElement;
    }

    public AdditionalInfoType Type
    {
        get { return type; }
    }

    public string AdditionalInfoText
    {
        get { return additionalInfoText; }
    }

    public GameObject CustomUIElement
    {
        get { return customUIElement; }
    }

}
