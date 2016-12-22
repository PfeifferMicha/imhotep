using UnityEngine;
/**
 * \brief Label.
 * 
 * 
 * Hier werden alle Berechnung durchgeführt die für jedes Label individuell sind
 * Erbt von der Klasse Punkt
 */


public class Label : Point{
    
   
    public float dispAngle; /**< Drehwinkel welcher durch Moment erzeugt wird */
    
    public GameObject annotationLabel; /**< Das Label auf welches dieses Label verweist */

    public Point pointOnPlane;  /**< der Annotationspunkt des Label */

    public float[] angleToLeftAndRightCorner = new float[2];  /**< abstand in Grad zur linken und rechten Ecke */

    public float angleToLeftLabel; /**< abstand in Grad zum linken Label */
    public float angleToRightLabel; /**< abstand in Grad zum rechten Label */

    public Vector2[] corners = new Vector2[4]; /**< Position der Ecken des Labels in seiner Ebene */

    public float weight;  /**< Gewicht des Labels */

    public Label leftLabel;
    public Label rightLabel;

    public Label()
    {

    }

    /**
    * \brief Hat einen int Parameter.
    */

    public Label(float y, float x, Vector3 v, int p, float a, GameObject annoPoint)
    {
        
        this.y = y;
        this.x = x;

        this.planeNumber = p;

        this.position = v;
              
        this.angle = a;        

		this.annotationPoint = annoPoint;

		annotationLabel = annotationPoint.GetComponent<Annotation>().getLabel();
    }

    
     
    /**
    * \brief Hier wird das GameObject Annotationlabel an eine neue position verschoben
    */
    public void moveToNewPosition()
    {       
        
        if (annotationLabel != null && annotationPoint != null)
        {
            if (!float.IsNaN(this.position.x) && !float.IsNaN(this.position.y) && !float.IsNaN(this.position.z))
            {

                CanvasMovement lm = annotationLabel.GetComponent<CanvasMovement>();
				lm.annotationPoint = annotationPoint;
                lm.newPos = this.position;
            }
            
        }
    }

    /**
    * \brief hier werden die Positionen der Ecken eines Annotationlabel berechnet
    */
    public void calculateCornersOnPlane(GameObject meshNode, GameObject meshPositionNode, GameObject meshViewerBase)
    {        


        float width = annotationLabel.GetComponent<RectTransform>().rect.width * annotationLabel.transform.parent.localScale.x * annotationLabel.GetComponent<RectTransform>().localScale.x * meshNode.transform.localScale.x * meshPositionNode.transform.localScale.x * meshViewerBase.transform.localScale.x;
        float height = annotationLabel.GetComponent<RectTransform>().rect.height * annotationLabel.transform.parent.localScale.x * annotationLabel.GetComponent<RectTransform>().localScale.x * meshNode.transform.localScale.x * meshPositionNode.transform.localScale.x * meshViewerBase.transform.localScale.x;

        corners[0].x = x - width / 2;
        corners[0].y = y + height / 2;

        corners[1].x = x + width / 2;
        corners[1].y = y + height / 2;

        corners[2].x = x - width / 2;
        corners[2].y = y - height / 2;

        corners[3].x = x + width / 2;
        corners[3].y = y - height / 2;

    }

    /**
    * \brief hier wird berechnet welchen Abstand in Grad die linke und rechte Ecke zum Mittelpunkt der Labels hat
    */
    public void calculateAngleToLeftAndRight(Vector3 xdirection, Vector3 ydirection, Vector3 planeOrigin)
    {

        if(angle > 0 && angle <= 90)
        {
            angleToLeftAndRightCorner[0] = Vector3.Angle(corners[0].x * xdirection + corners[0].y * ydirection, this.position - planeOrigin);
            angleToLeftAndRightCorner[1] = Vector3.Angle(corners[3].x * xdirection + corners[3].y * ydirection, this.position - planeOrigin);

        }
        else if (angle > 90 && angle <= 180)
        {
            angleToLeftAndRightCorner[0] = Vector3.Angle(corners[1].x * xdirection + corners[1].y * ydirection, this.position - planeOrigin);
            angleToLeftAndRightCorner[1] = Vector3.Angle(corners[2].x * xdirection + corners[2].y * ydirection, this.position - planeOrigin);
        }
        else if(angle > 180 && angle <= 270)
        {
            angleToLeftAndRightCorner[0] = Vector3.Angle(corners[3].x * xdirection + corners[3].y * ydirection, this.position - planeOrigin);
            angleToLeftAndRightCorner[1] = Vector3.Angle(corners[0].x * xdirection + corners[0].y * ydirection, this.position - planeOrigin);
        }
        else
        {
            angleToLeftAndRightCorner[0] = Vector3.Angle(corners[2].x * xdirection + corners[2].y * ydirection, this.position - planeOrigin);
            angleToLeftAndRightCorner[1] = Vector3.Angle(corners[1].x * xdirection + corners[1].y * ydirection, this.position - planeOrigin);
        }


    }

    /**
    * \brief hier wird das Annotationlabel an das Zoomen des Nutzers angepasst
    */
    public void adaptLabelToZooming(float meshScale, float labelscale) // die größe der Labels sollten unabhängig vom zoomen sein..
    {                                                              // ist jetzt im canvasmovement..
        if(annotationLabel != null)
        {
            annotationLabel.GetComponent<RectTransform>().localScale = new Vector3(labelscale / meshScale, labelscale / meshScale, labelscale / meshScale);
        }
        
    }      

    /**
    * \brief Offset berechnung
    * 
    * es wird der Abstand zwischen Mittelpunkt von Label und dem Punkt auf dem Label
    * ausgerechnet welcher den geringsten abstand zum Mittelpunkt des annotierten objekt aufweist 
    */
    public float calculateOffset()
    {

        float offset = 0;

        float height = (corners[0] - corners[2]).magnitude;
        float width = (corners[0] - corners[1]).magnitude;

        float a = (height / 2) + Mathf.Sqrt((height / 4) + (width / 4));
        float b = Mathf.Sqrt(a * (height / 2));
        float epsilon = (width / (a * 2));



        if (angle >= 0 && angle <= 270)
        {

            float phi = 270 - angle;
            phi = Mathf.Deg2Rad * phi;

            float r = b / (Mathf.Sqrt(1 - epsilon * epsilon * Mathf.Cos(phi) * Mathf.Cos(phi)));

            offset = r;


        }
        else{

            float phi = 360 + (270 - angle);
            phi = Mathf.Deg2Rad * phi;

            float r = b / (Mathf.Sqrt(1 - epsilon * epsilon * Mathf.Cos(phi) * Mathf.Cos(phi)));

            offset = r;

        }


            return offset;

    }

    /**
    * \brief Gewichtsberechnung 
    */
    public void calculateWeight(float weightAngleSizeRatio)
    {       

        float angleSize = angleToLeftAndRightCorner[0] + angleToLeftAndRightCorner[1];

        weight = weightAngleSizeRatio * angleSize;

        
    }
}
