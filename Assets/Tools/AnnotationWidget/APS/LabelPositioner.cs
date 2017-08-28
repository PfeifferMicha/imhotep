using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;


/**
 * \brief LabelPositioner.
 * 
 * 
 * Hier werden alle Berechnung für den Kraft-basierten Ansatz vorgenommen.
 * Alle Ebenen werden hier erstellt und verwaltet
 */



public class LabelPositioner : MonoBehaviour
{
    public struct PlaneVectors /**<  die Vektoren welche die Ebene aufspannen  */
    {
        public Vector3 x, y, z, origin;

        public PlaneVectors(Vector3 x, Vector3 y, Vector3 z, Vector3 o)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.origin = o;
        }
    }
    private PlaneVectors[] pointPlaneVectors; /**<  Vektoren für die Ebenen zu denen der Abstand von den Punkten gemessen wird  */
    private PlaneVectors[] labelPlaneVectors; /**<   Vektoren der Ebenen in denen die labels Liegen  */

    private Plane[] pointPlanes; /**<   Ebenen der Punkte   */
    private Plane[] labelPlanes; /**<   Ebenen in denen die Labels liegen   */

    private Plane[] camPlanes; /**<   Ebenen des ViewPlaneFrustum   */
    private float[] initialPlaneRadius = new float[4];  /**<   Radien der Ebenen   */

    private Bounds meshBox; /**<   BoundingBox um das Objekt   */
    private BoundingSphere bSphere; /**<   BoundingSphere um das Objekt   */

    private GameObject meshRotationNode;
    private GameObject meshNode;
    private GameObject meshPositionNode;
    private GameObject meshViewerBase;

    private List<GameObject> annotationList;

    public Camera silhouetteCamPrefab; /**<   Prefab mit der Später ein 2D-Bild gerendert wird   */
    private Texture2D silhouetteImage; /**<   Textur in der das Bild gespeichert wird   */

    private Camera silhouetteCam; /**<   Kamera mit der später das silhouetteImage gerendert wird   */

    public float scaleCloseRage = 1.75f; /**<   Schwellwert von MeshViewerScale (Scale) ab dem die hinteren Labels ausgeblendet werden   */

    public bool autoHideAnnotations = true;

    private List<Label>[] labelLists = new List<Label>[4]; /**<   ein Array von Listen für die Labels in den einzelnen Ebenen   */
    private List<Point>[] pointsOnPlane = new List<Point>[4]; /**<   ein Array von Listen für die Punkte in den einzelnen Ebenen   */

    private Quaternion meshRotation = Quaternion.identity; /**<   die Rotation des Objekts   */

    private float rotationChange; /**<   die Rotationsänderung als Winkel   */

    private float meshScale = 0; /**<   die Skalierung des Objekts   */
    private float scaleChange; /**<   die Skalierungsänderung des Objekts   */

    private int[] energyOfPlanes = new int[4]; /**<   die Unordnung jeder einzelnen Ebene   */

    private bool switchToZoomPlane = false; /**<   ob die Zoomebene verwendet wird oder nicht   */

    private int pointCount = 0; /**<   anzahl an Punkten   */
    private int counter = 0; /**<   anzahl an durchlaufenen Iteratiionschritten   */

    private float[] rads = new float[360]; /**<  die Radien der Kuchenstücke für die Silhouette   */

    private bool inCalculationOfZoomPlane = false; /**<   ob momentan eine berechnung zur neupositionierung in der Zoomebene stattfindet   */
    private bool inCalculationOfPlanes = false; /**<   ob momentan eine berechnung zur neupositionierung in den anderen Ebenen stattfindet   */

    public int iterations = 250; /**<   Anzahl der Iterationen   */
    public float weightAngleSizeRatio = 0.2f; /**<   Winkelgewicht verhältnis   */
    public bool middlePlane = false; /**<   ob Labels in der mittleren Ebene dargestellt werden sollen oder nicht   */

    public float zoomPlaneDistance = 2f; /**<   abstand der Zoomebene von der Kamera   */
    public float zoomPlaneDetectionRadius; /**<   Radius des Kreises auf der Zoomebene  */

    private int whichMode = 0;

    private bool startRepositioning; //dient als interrupt falls die anzahl der annotationen sich ändert

    void Start()
    {
        initialPlaneRadius = new float[4];

        pointPlanes = new Plane[4];
        labelPlanes = new Plane[4];

        pointPlaneVectors = new PlaneVectors[4];
        labelPlaneVectors = new PlaneVectors[4];

        meshViewerBase = GameObject.Find("MeshViewerBase");
        meshRotationNode = GameObject.Find("MeshViewerBase/MeshViewerScale/MeshRotationNode");
        meshNode = GameObject.Find("MeshViewerBase/MeshViewerScale");
        meshPositionNode = GameObject.Find("MeshViewerBase/MeshViewerScale/MeshRotationNode/MeshPositionNode");

        camPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        zoomPlaneDistance = 3f;
        weightAngleSizeRatio = 0.2f; // wurde experimentell bestimmt
        iterations = 350;
        whichMode = 0;

        silhouetteCam = (Camera)Instantiate(silhouetteCamPrefab, Camera.main.transform.position, Camera.main.transform.rotation);
        silhouetteCam.transform.parent = this.transform;

        annotationList = AnnotationControl.instance.getAnnotationList();
    }

    /**
    * \brief Updatemethode
    * 
    * Diese Methode wird jedes Frame aufgerufen
    * Hier wird überprüft ob die Anzahl der Annotationspunkte sich geändert hat oder nicht 
    */
    void Update()
    {
        rotationChange = Quaternion.Angle(meshRotation, meshRotationNode.transform.rotation);
        scaleChange = Math.Abs(meshNode.transform.localScale.x - meshScale);

        if (annotationList != null)
        {
            if (annotationList.Count != 0)
            {
                if (annotationList[annotationList.Count - 1].GetComponent<Annotation>().getLabel() != null)
                {
                    updateAnnotation();
                }
            }
        }

    }

    public void forceRecalculate()
    {
        whichMode = 0;
        rotationChange = 35;
        startRepositioning = true;
        inCalculationOfPlanes = false;
        inCalculationOfZoomPlane = false;
        if (meshNode != null)
        {
            updateAnnotation();
        }
    }


    /// <summary>
    /// Called by Annotation Control to update Annotation List when it changes
    /// </summary>
    /// <param name="annoList">tge new List with changes.</param>
    public void updateAnnotationList(List<GameObject> annoList)
    {
        annotationList = annoList;
        whichMode = 0;
        rotationChange = 35;
        startRepositioning = true;
        inCalculationOfPlanes = false;
        inCalculationOfZoomPlane = false;
        if (meshNode != null)
        {
            updateAnnotation();
        }

    }

    public void resetAnnotationList()
    {
        annotationList = new List<GameObject>();
    }


    /**
    * \brief updateAnnotation
    * prüft ob die Labels neu angeordnet werden sollen oder nicht.
    * Bestimmt zusätzlich welcher Modus angewandt wird (Zoom oder Normal)
    * Berechnet während der Berechnungszeit jedes Frame eine Iterationsstufe
    * \param annotationPoints. Liste der Annotationspunkte von AnnotationControl
    */
    private void updateAnnotation()
    {
        //things to do every frame

        createBoundingBox();
        zoomPlaneDetectionRadius = getInitialPlaneRadius(3);

        if (meshNode.transform.localScale.x < scaleCloseRage) //hier wird unterschieden ob das Objekt sehr nah ist oder nicht. Jenachdem werden bestimmte labels ausgeblendet oder eingeblendet
        {
            bool[] whichPlane = new bool[] { false, middlePlane, true, false };

            if (switchToZoomPlane)
            {
                rotationChange = 35;
                switchToZoomPlane = false;
            }

            if (rotationChange >= 35 || scaleChange >= 0.25f || startRepositioning) //hier wird geprüft ob das Model sich mehr als 35 grad gedreht hat --> labels werden neu angeordnet
            {
                if (whichMode == 2)
                {
                    meshRotation = meshRotationNode.transform.rotation;
                    meshScale = meshNode.transform.localScale.x;

                    inCalculationOfPlanes = true;
                    inCalculationOfZoomPlane = false;

                    calculateEnergy(whichPlane);

                    startRepositioning = false;
                    whichMode = 0;
                    counter = 0;   // iterationen zähler
                }
                else if (whichMode == 1)
                {
                    for (int i = 0; i < labelLists.Length; i++)
                    {
                        labelLists[i] = new List<Label>();
                        pointsOnPlane[i] = new List<Point>();
                    }
                    createLabelCircle(switchToZoomPlane);

                    sortListsByAngle(whichPlane);

                    defineNeighbours();

                    whichMode = 2;
                }
                else if (whichMode == 0)
                {
                    createPlanes(zoomPlaneDistance);

                    silhouetteCam.transform.position = Camera.main.transform.position;
                    silhouetteCam.transform.LookAt(meshBox.center);
                    takePicture();
                    createPie();
                    whichMode = 1;
                    //drawPicture();
                }




            }
            else if (inCalculationOfPlanes)
            {
                calculateForces(switchToZoomPlane, whichPlane);
                counter++;
                if (energyOfPlanes[2] == 0 || counter > iterations)
                {
                    inCalculationOfPlanes = false;
                    //drawPicture();
                    counter = 0;
                }
            }
        }
        else
        {

            bool[] whichPlane = new bool[] {false, false, false, true};

            if (!switchToZoomPlane)
            {
                rotationChange = 15;
                switchToZoomPlane = true;
            }
            if (rotationChange >= 15 || scaleChange >= 0.1f)
            {
                createPlanes(zoomPlaneDistance);

                inCalculationOfPlanes = false;
                inCalculationOfZoomPlane = true;

                meshRotation = meshRotationNode.transform.rotation;
                meshScale = meshNode.transform.localScale.x;

                for (int i = 0; i < labelLists.Length; i++)
                {
                    labelLists[i] = new List<Label>();
                    pointsOnPlane[i] = new List<Point>();
                }

                createLabelCircle(switchToZoomPlane);

                sortListsByAngle(whichPlane);

                defineNeighbours();

                calculateEnergy(whichPlane);

                counter = 0;
            }
            else if (inCalculationOfZoomPlane)
            {

                switchToZoomPlane = true;
                calculateForces(switchToZoomPlane, whichPlane);
                counter++;


                if (energyOfPlanes[3] == 0 || counter > iterations)
                {
                    inCalculationOfZoomPlane = false;
                    counter = 0;
                }
            }
        }
    }

    /**
    * \brief sortListsByAngle
    * hier werden alle Listen von Labels nach den Winkeln der Labels bezüglich der Y Achse sortiert.
    * 
    * \param whichPlane. Beschreibt welche Ebenen sortiert werden sollen
    */
    private void sortListsByAngle(bool[] whichPlane)
    {

        for (int i = 0; i < labelLists.Length; i++)
        {
            if (whichPlane[i])
            {
                labelLists[i].Sort((s1, s2) => s1.angle.CompareTo(s2.angle));
            }
        }


    }

    /**
    * \brief createLabelCircle
    * hier werden alle Label auf die Ebenen projiziert und ordnet diese Ringförmig um das Label an
    * 
    * \param annotationPoints. Liste der Annotationspunkte von AnnotationControl
    * \param zoomPlane. Beschreibt ob die Zoomebene aktiv ist oder nicht
    */
    private void createLabelCircle(bool zoomPlane)
    {
        int planeNumber;

        for (int i = 0; i < annotationList.Count; i++)
        {

            Point p = new Point();


            if (annotationList[i] != null)
            {

                planeNumber = updateWhichPlaneForPoint(annotationList[i], zoomPlane, middlePlane);


                p.planeNumber = planeNumber;
                p = orthoProjectPointOnPlane(annotationList[i].transform.position, planeNumber, labelPlaneVectors[planeNumber], labelPlanes[planeNumber]);
                p.annotationPoint = annotationList[i];

                //hier wird der Punkt der zuvor auf eine Ebene projiziert wurde auf einen Kreis in der ebene projiziert

                float radius = getInitialPlaneRadius(planeNumber);

                Vector3 pointProjectedOnCircle = labelPlaneVectors[planeNumber].origin + radius * (p.position - labelPlaneVectors[planeNumber].origin).normalized;

                float y = Vector3.Dot(pointProjectedOnCircle - labelPlaneVectors[planeNumber].origin, labelPlaneVectors[planeNumber].y);
                float x = Vector3.Dot(pointProjectedOnCircle - labelPlaneVectors[planeNumber].origin, labelPlaneVectors[planeNumber].x);

                Label l = new Label(y, x, pointProjectedOnCircle, planeNumber, p.angle, annotationList[i]);

                l.pointOnPlane = p;
                l.calculateAngleToLeftAndRight(labelPlaneVectors[planeNumber].x, labelPlaneVectors[planeNumber].y, labelPlaneVectors[planeNumber].origin);
                l.calculateWeight(weightAngleSizeRatio);
                
                p.label = l;
                //check if annotation is not current

                if (l.annotationLabel != null)
                {
                    if (AnnotationControl.instance.isCurrentAnnotation(p.annotationPoint))
                    {
                        l.annotationLabel.SetActive(true);
                    }

                    if (zoomPlane)
                    {
                        if (planeNumber != 3)
                        {
                            if (!AnnotationControl.instance.isCurrentAnnotation(p.annotationPoint))
                            {
                                if (autoHideAnnotations)
                                    l.annotationLabel.SetActive(false);
                            }
                            //l.annotationPoint.SetActive(false);
                        }
                        else
                        {
                            l.annotationLabel.SetActive(true);
                            l.annotationPoint.SetActive(true);
                            labelLists[planeNumber].Add(l);
                            pointsOnPlane[planeNumber].Add(p);
                        }
                    }
                    else
                    {
                        if (middlePlane)
                        {
                            if (planeNumber == 0 || planeNumber == 3)
                            {
                                if (!AnnotationControl.instance.isCurrentAnnotation(p.annotationPoint))
                                {
                                    if (autoHideAnnotations)
                                        l.annotationLabel.SetActive(false);
                                }
                                //l.annotationPoint.SetActive(false);
                            }
                            else
                            {
                                l.annotationLabel.SetActive(true);
                                l.annotationPoint.SetActive(true);
                                labelLists[planeNumber].Add(l);
                                pointsOnPlane[planeNumber].Add(p);
                            }
                        }
                        else
                        {
                            if (planeNumber != 2)
                            {
                                if (!AnnotationControl.instance.isCurrentAnnotation(p.annotationPoint))
                                {
                                    if (autoHideAnnotations)
                                        l.annotationLabel.SetActive(false);
                                }
                                //l.annotationPoint.SetActive(false);
                            }
                            else
                            {
                                l.annotationLabel.SetActive(true);
                                l.annotationPoint.SetActive(true);
                                labelLists[planeNumber].Add(l);
                                pointsOnPlane[planeNumber].Add(p);
                            }
                        }
                    }
                }
            }
        }
        if (zoomPlane)
        {
            calculateCenterOfZoomPlane(pointsOnPlane[3]);
        }
    }

    /**
    * \brief defineNeighbours
    * hier werden alle labels mit ihren ersten linksliegenden und ersten rechtsliegenden labels verknüpft
    * 
    */
    private void defineNeighbours()
    {
        for (int m = 0; m < labelLists.Length; m++)
        {
            for (int j = 0; j < labelLists[m].Count; j++)
            {
                if (j == 0)
                {
                    labelLists[m][j].leftLabel = labelLists[m][labelLists[m].Count - 1];

                    if (labelLists[m].Count >= 2)
                    {
                        labelLists[m][j].rightLabel = labelLists[m][j + 1];
                    }
                    else
                    {
                        labelLists[m][j].rightLabel = labelLists[m][j];
                    }
                }
                else if (j == labelLists[m].Count - 1)
                {
                    labelLists[m][j].rightLabel = labelLists[m][0];

                    if (labelLists[m].Count >= 2)
                    {
                        labelLists[m][j].leftLabel = labelLists[m][j - 1];
                    }
                    else
                    {
                        labelLists[m][j].leftLabel = labelLists[m][j];
                    }
                }
                else
                {
                    labelLists[m][j].rightLabel = labelLists[m][j + 1];
                    labelLists[m][j].leftLabel = labelLists[m][j - 1];

                }

            }
        }



    }


    /**
    * \brief calculateForces
    * hier werden die Kräfte zwischen den Labels berechnet
    * 
    * \param zoomPlane. Beschreibt ob die Zoomebene aktiv ist oder nicht
    * \param whichPlane. Beschreibt in welchen Ebenen Kräfte wirken können
    */
    private void calculateForces(bool zoomPlane, bool[] whichPlane)
    {
        for (int m = 0; m < labelLists.Length; m++)
        {
            if (whichPlane[m])
            {
                for (int j = 0; j < labelLists[m].Count; j++)
                {
                    labelLists[m][j].dispAngle = 0;

                    if (labelLists[m][j].annotationLabel != null)
                    {
                        if (labelLists[m][j].annotationLabel.activeSelf)
                        {
                            for (int u = 0; u < labelLists[m].Count; u++)
                            {
                                if (labelLists[m][u].annotationLabel != null)
                                {
                                    if (labelLists[m][u].annotationLabel.activeSelf)
                                    {
                                        if (j != u)
                                        {
                                            float angleOfForce = Vector3.Angle(labelLists[m][j].position, labelLists[m][u].position);

                                            if (overlapLabelOnLabel(labelLists[m][j], labelLists[m][u]))
                                            //if(true)
                                            {

                                                if (labelLists[m][j].angle > 180)
                                                {
                                                    if (labelLists[m][u].angle < labelLists[m][j].angle - 180)
                                                    {   //Kraft kommt von rechts
                                                        labelLists[m][j].dispAngle = labelLists[m][j].dispAngle - calculateRepForce(angleOfForce, labelLists[m][u].weight);

                                                    }
                                                    else
                                                    {
                                                        if (labelLists[m][u].angle > labelLists[m][j].angle)
                                                        {
                                                            //Kraft kommt auch von rechts
                                                            labelLists[m][j].dispAngle = labelLists[m][j].dispAngle - calculateRepForce(angleOfForce, labelLists[m][u].weight);

                                                        }
                                                        else
                                                        {
                                                            //Kraft kommt von links
                                                            labelLists[m][j].dispAngle = labelLists[m][j].dispAngle + calculateRepForce(angleOfForce, labelLists[m][u].weight);

                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (labelLists[m][u].angle > labelLists[m][j].angle + 180)
                                                    {
                                                        //Kraft kommt von links
                                                        labelLists[m][j].dispAngle = labelLists[m][j].dispAngle + calculateRepForce(angleOfForce, labelLists[m][u].weight);
                                                    }
                                                    else
                                                    {
                                                        if (labelLists[m][u].angle < labelLists[m][j].angle)
                                                        {
                                                            //kraft kommt von links
                                                            labelLists[m][j].dispAngle = labelLists[m][j].dispAngle + calculateRepForce(angleOfForce, labelLists[m][u].weight);
                                                        }
                                                        else
                                                        {  //kraft kommt von rechts
                                                            labelLists[m][j].dispAngle = labelLists[m][j].dispAngle - calculateRepForce(angleOfForce, labelLists[m][u].weight);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                applyForcesOnLabels(zoomPlane, whichPlane);
            }
        }
    }

    /**
    * \brief applyForcesOnLabels
    * hier werden die Kräfte zwischen den Labels auf die Labels angewandt
    * 
    * \param maxDispAngle. Gibt den maximalen Winkel an um welches ein Label rotieren darf
    * \param zoomPlane. Beschreibt ob die Zoomebene aktiv ist oder nicht
    * \param whichPlane. Beschreibt in welchen Ebenen Kräfte wirken können
    */
    private void applyForcesOnLabels(bool zoomPlane, bool[] whichPlane)
    {
        for (int m = 0; m < labelLists.Length; m++)
        {
            if (whichPlane[m])
            {
                calculateEnergy(whichPlane);
                float maxDispAngle = energyOfPlanes[m];

                for (int j = 0; j < labelLists[m].Count; j++)
                {
                    float angleToRight = 0;
                    float angleToLeft = 0;

                    if (labelLists[m][j].rightLabel.angle > labelLists[m][j].angle)
                    {
                        angleToRight = labelLists[m][j].rightLabel.angle - labelLists[m][j].angle;
                    }
                    else
                    {
                        angleToRight = (360 - labelLists[m][j].angle) + labelLists[m][j].rightLabel.angle;
                    }

                    if (labelLists[m][j].angle > labelLists[m][j].leftLabel.angle)
                    {
                        angleToLeft = labelLists[m][j].angle - labelLists[m][j].leftLabel.angle;
                    }
                    else
                    {
                        angleToLeft = (360 - labelLists[m][j].leftLabel.angle) + labelLists[m][j].angle;
                    }

                    labelLists[m][j].angleToLeftLabel = angleToLeft;
                    labelLists[m][j].angleToRightLabel = angleToRight;

                    if (labelLists[m][j].dispAngle > 0)
                    {

                        if (labelLists[m][j].angleToLeftLabel < labelLists[m][j].dispAngle)
                        {

                            labelLists[m][j].dispAngle = labelLists[m][j].angleToLeftLabel;

                            if (labelLists[m][j].dispAngle > maxDispAngle)
                            {
                                labelLists[m][j].dispAngle = maxDispAngle;
                            }
                        }
                        else
                        {
                            if (labelLists[m][j].dispAngle > maxDispAngle)
                            {
                                labelLists[m][j].dispAngle = maxDispAngle;
                            }
                        }
                    }
                    else
                    {
                        if (-labelLists[m][j].angleToRightLabel > labelLists[m][j].dispAngle)
                        {
                            labelLists[m][j].dispAngle = -(labelLists[m][j].angleToRightLabel);

                            if (labelLists[m][j].dispAngle < (-maxDispAngle))
                            {
                                labelLists[m][j].dispAngle = -maxDispAngle;
                            }
                        }
                        else
                        {
                            if (labelLists[m][j].dispAngle < (-maxDispAngle))
                            {
                                labelLists[m][j].dispAngle = -maxDispAngle;
                            }
                        }
                    }

                    labelLists[m][j].dispAngle = labelLists[m][j].dispAngle * Mathf.Deg2Rad;

                    float xNew = labelLists[m][j].x * Mathf.Cos(labelLists[m][j].dispAngle) - labelLists[m][j].y * Mathf.Sin(labelLists[m][j].dispAngle);
                    float yNew = labelLists[m][j].x * Mathf.Sin(labelLists[m][j].dispAngle) + labelLists[m][j].y * Mathf.Cos(labelLists[m][j].dispAngle);

                    labelLists[m][j].y = yNew;
                    labelLists[m][j].x = xNew;

                    labelLists[m][j].position = labelPlaneVectors[m].origin + yNew * labelPlaneVectors[m].y + xNew * labelPlaneVectors[m].x;

                    float angle = Vector3.Angle(labelLists[m][j].position - labelPlaneVectors[m].origin, labelPlaneVectors[m].origin + 2 * labelPlaneVectors[m].y);


                    if (labelLists[m][j].x >= 0)
                    {

                    }
                    else
                    {
                        angle = 360 - angle;
                    }

                    labelLists[m][j].angle = angle;
                    labelLists[m][j].dispAngle = 0;

                    float newRadius = getInitialPlaneRadius(m);

                    labelLists[m][j].calculateCornersOnPlane(meshNode, meshPositionNode, meshViewerBase);
                    labelLists[m][j].calculateAngleToLeftAndRight(labelPlaneVectors[m].x, labelPlaneVectors[m].y, labelPlaneVectors[m].origin);
                    
                    if (!zoomPlane)
                    {
                        if (m == 2)
                        {
                            newRadius = findSmallestPossibleRadius(labelLists[m][j]);
                        }

                        if (newRadius > getInitialPlaneRadius(m))
                        {
                            newRadius = getInitialPlaneRadius(m);
                        }
                    }

                    float offset = 0;

                    /*if (!zoomPlane)
                    {                        
                        offset = labelLists[m][j].calculateOffset2();
                    }*/
                    newRadius = newRadius + offset;

                    labelLists[m][j].position = labelPlaneVectors[m].origin + newRadius * (labelLists[m][j].position - labelPlaneVectors[m].origin).normalized;

                    labelLists[m][j].y = Vector3.Dot(labelLists[m][j].position - labelPlaneVectors[m].origin, labelPlaneVectors[m].y);
                    labelLists[m][j].x = Vector3.Dot(labelLists[m][j].position - labelPlaneVectors[m].origin, labelPlaneVectors[m].x);

                    labelLists[m][j].calculateCornersOnPlane(meshNode, meshPositionNode, meshViewerBase);
                    labelLists[m][j].calculateAngleToLeftAndRight(labelPlaneVectors[m].x, labelPlaneVectors[m].y, labelPlaneVectors[m].origin);

                    labelLists[m][j].calculateWeight(weightAngleSizeRatio);

                    labelLists[m][j].moveToNewPosition();
                }
            }
        }
    }



    /**
    * \brief calculateRepForce
    * die abstoßende Kraft welche zwischen zwei Label auftreten kann
    * 
    * \param distance. Gibt den Winkel zwischen zwei Label an
    * \param k. das Gewicht eines Label    
    */
    private float calculateRepForce(float distance, float k)
    {
        float magnitude;

        if (distance > 0)
        {
            magnitude = -(k * k) / distance;
        }
        else
        {
            magnitude = -(k * k) / 0.001f;
        }
        return magnitude;
    }


    /**
    * \brief createPlanes
    * hier werden die Ebenen erstellt
    * 
    * \param zoomPlaneDistance. Gibt an wie weit die Zoomebene vom Betrachter entfernt ist
    * \return berechnete Kraft  
    */
    private void createPlanes(float zoomPlaneDistance)
    {
        Vector3 center = meshNode.transform.position;
        center = meshBox.center;

        Vector3 planesNormal = (center - Camera.main.transform.position).normalized;

        float distanceBetweenPointPlanes = bSphere.radius / 2;

        Vector3 xAxis = planesNormal; //xAxis zeigt vom Mittelpunkt nach rechts
        Vector3 yAxis = planesNormal; //YAxis zeigt vom Mittelpunkt nach oben
        Vector3 zAxis = planesNormal; //zAxis zeigt vom Mittelpunkt nach hinten
        Vector3.OrthoNormalize(ref zAxis, ref yAxis, ref xAxis); // hier wird die Y-Achse leider umgedreht, daher wird sie bei verwendung wieder umgedreht

        //Debug.DrawLine(center, center - 5 * yAxis);
        //Debug.DrawLine(center, center + 5 * zAxis, Color.red, 10);
        //Debug.DrawLine(center, center + 5 * xAxis);

        //Fläche 1 hinterste ebene
        pointPlanes[0] = new Plane();
        pointPlanes[0].SetNormalAndPosition(planesNormal, center + (planesNormal * distanceBetweenPointPlanes));

        labelPlanes[0] = new Plane();
        labelPlanes[0].SetNormalAndPosition(planesNormal, center + (planesNormal * distanceBetweenPointPlanes * 2));

        pointPlaneVectors[0] = new PlaneVectors(xAxis, -yAxis, zAxis, center + (planesNormal * distanceBetweenPointPlanes));
        labelPlaneVectors[0] = new PlaneVectors(xAxis, -yAxis, zAxis, center + (planesNormal * distanceBetweenPointPlanes * 2));

        //Fläche 2 mittlere ebene
        pointPlanes[1] = new Plane();
        pointPlanes[1].SetNormalAndPosition(planesNormal, center);

        labelPlanes[1] = new Plane();
        labelPlanes[1].SetNormalAndPosition(planesNormal, center);

        pointPlaneVectors[1] = new PlaneVectors(xAxis, -yAxis, zAxis, center);
        labelPlaneVectors[1] = new PlaneVectors(xAxis, -yAxis, zAxis, center);

        //Fläche 3 vordere ebene
        pointPlanes[2] = new Plane();
        pointPlanes[2].SetNormalAndPosition(planesNormal, center - (planesNormal * distanceBetweenPointPlanes));

        labelPlanes[2] = new Plane();
        labelPlanes[2].SetNormalAndPosition(planesNormal, center - (planesNormal * distanceBetweenPointPlanes * 2));

        pointPlaneVectors[2] = new PlaneVectors(xAxis, -yAxis, zAxis, center - (planesNormal * distanceBetweenPointPlanes));
        labelPlaneVectors[2] = new PlaneVectors(xAxis, -yAxis, zAxis, center - (planesNormal * distanceBetweenPointPlanes * 2));


        //Fläche 4 Zoomebene
        RaycastHit hit = new RaycastHit();

        int layerMask = 1 << 9; //Layer 9 for MeshViewerLayer

        float maxDistance = (Camera.main.transform.position - meshBox.center).magnitude;



        Ray ray = new Ray(Camera.main.transform.position, meshBox.center - Camera.main.transform.position);
        Physics.Raycast(ray, out hit, maxDistance, layerMask);


        Vector3 zoomPlaneLabelCenter = Camera.main.transform.position + zoomPlaneDistance * planesNormal;
        Vector3 zoomPlanePointCenter = hit.point;

        Debug.DrawLine(Camera.main.transform.position, zoomPlanePointCenter);


        pointPlanes[3] = new Plane();
        pointPlanes[3].SetNormalAndPosition(planesNormal, zoomPlanePointCenter);

        labelPlanes[3] = new Plane();
        labelPlanes[3].SetNormalAndPosition(planesNormal, zoomPlaneLabelCenter);

        pointPlaneVectors[3] = new PlaneVectors(xAxis, -yAxis, zAxis, zoomPlanePointCenter);
        labelPlaneVectors[3] = new PlaneVectors(xAxis, -yAxis, zAxis, zoomPlaneLabelCenter);

    }

    /**
    * \brief calculateCenterOfZoomPlane
    * hier wird der Mittelpunkt der Zoomebene in abhängigkeit der auf der Zoomebene darzustellenden Punkten berechnet 
    * 
    * \param listOfPointsInCZoomPlane. Gibt an wie weit die Zoomebene vom Betrachter entfernt ist
    *     
    */
    private void calculateCenterOfZoomPlane(List<Point> listOfPointsInZoomPlane)
    {
        float yCenter = 0;
        float xCenter = 0;

        foreach (Point p in listOfPointsInZoomPlane)
        {
            yCenter += p.y;
            xCenter += p.x;
        }
        if (listOfPointsInZoomPlane.Count != 0)
        {
            yCenter = yCenter / listOfPointsInZoomPlane.Count;
            xCenter = xCenter / listOfPointsInZoomPlane.Count;
        }

        labelPlaneVectors[3].origin = labelPlaneVectors[3].origin + labelPlaneVectors[3].y * yCenter + labelPlaneVectors[3].x * xCenter;
        pointPlaneVectors[3].origin = pointPlaneVectors[3].origin + labelPlaneVectors[3].y * yCenter + labelPlaneVectors[3].x * xCenter;


        for (int i = 0; i < pointsOnPlane[3].Count; i++)
        {
            Point p = orthoProjectPointOnPlane(pointsOnPlane[3][i].annotationPoint.transform.position, 3, labelPlaneVectors[3], labelPlanes[3]);
            pointsOnPlane[3][i].position = p.position;
            pointsOnPlane[3][i].angle = p.angle;
            pointsOnPlane[3][i].x = p.x;
            pointsOnPlane[3][i].y = p.y;
            pointsOnPlane[3][i].z = p.z;

            Vector3 pointProjectedOnCircle = labelPlaneVectors[3].origin + initialPlaneRadius[3] * (p.position - labelPlaneVectors[3].origin).normalized;
            float y = Vector3.Dot(pointProjectedOnCircle - labelPlaneVectors[3].origin, labelPlaneVectors[3].y);
            float x = Vector3.Dot(pointProjectedOnCircle - labelPlaneVectors[3].origin, labelPlaneVectors[3].x);

            pointsOnPlane[3][i].label.position = pointProjectedOnCircle;
            pointsOnPlane[3][i].label.x = x;
            pointsOnPlane[3][i].label.y = y;
            pointsOnPlane[3][i].label.angle = p.angle;
        }


        sortListsByAngle(new bool[] { false, false, false, true });
    }

    /**
    * \brief updateWhichPlaneForPoint
    * hier wird bestimmt in welche Ebene ein Label gehört 
    * 
    * \param annoPoint. Der annotationpunkt des Labels
    * \param zoomPlane. ob die Zoomebene aktiv ist
    * \param middlePlane. ob auf der mittleren Ebene Labels dargestellt werden sollen oder nicht
    * \return Ebenennummer    
    */
    private int updateWhichPlaneForPoint(GameObject annoPoint, bool zoomPlane, bool middlePlane)
    {

        Vector3 pointPosition = annoPoint.transform.position;
        //hier wird bestimmt welche Fläche welchem Punkt am nächsten liegt
        float d1 = pointPlanes[0].GetDistanceToPoint(pointPosition);
        //d1 = Mathf.Abs(d1);
        float d2 = pointPlanes[1].GetDistanceToPoint(pointPosition);
        //d2 = Mathf.Abs(d2);
        float d3 = pointPlanes[2].GetDistanceToPoint(pointPosition);
        //d3 = Mathf.Abs(d3);
        float d4 = pointPlanes[3].GetDistanceToPoint(pointPosition);
        d4 = Mathf.Abs(d4);


        //wertet aus zu welcher Ebene der punkt den geringsten abstand hat

        float depth = 0.8f; // bei der zoomebene wird eine ebene auf der oberfläche des dargestellten objekts platziert und anschließend wird geprüft welche punkt innerhalb eines kreis auf dieser ebene liegt
                            //da dargestelltes objekt dreidimensional wird zusätzlich noch der abstand in z richtung bestimmt un gepüft ob der punkt nah genug an der ebene dran ist


        if (zoomPlane)
        {
            // punkt gehört in ebene 3 oder in die Blickfeldebene
            Point p = orthoProjectPointOnPlane(annoPoint.transform.position, 3, pointPlaneVectors[3], pointPlanes[3]);
            if ((p.x * p.x + p.y * p.y) < zoomPlaneDetectionRadius * zoomPlaneDetectionRadius && d4 <= depth)
            {
                //point is in fieldofVision, ebene 4                
                return 3;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            if (d2 > 0)
            {
                return 0;
            }
            else
            {
                if (Mathf.Abs(d3) <= Mathf.Abs(d2))
                {
                    return 2;
                }
                else
                {
                    if (middlePlane)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0; //nur labels in der vorderen ebene werden angezeigt
                    }
                }
            }
        }

    }

    /**
    * \brief updateWhichPlaneForPoint
    * gibt den standard Radius einer Ebene zurück
    * 
    * \param planeNumber. Ebenennummer
    * \return Radius   
    */
    private float getInitialPlaneRadius(int planeNumber)
    {
        initialPlaneRadius[0] = 0;
        initialPlaneRadius[1] = bSphere.radius * 1.05f;
        initialPlaneRadius[2] = bSphere.radius;
        initialPlaneRadius[3] = zoomPlaneDistance * Mathf.Tan(1.4f) / 8; //hier wird der radius des sichtfelds einer person berechnet. 1.4f in rad sind 80 grad. 80 grad ist die hälfte vom sichtfeld    

        return initialPlaneRadius[planeNumber];
    }

    /**
    * \brief createBoundingBox
    * erstellt eine BoundingBox um das zu annotierende Objekt
    */
    private void createBoundingBox()
    {
        //Transform child1 = meshPositionNode.transform.GetChild(0);
        //Transform childchild1 = child1.GetChild(0);
        //MeshRenderer mesh = childchild1.GetComponent<MeshRenderer>();
        MeshRenderer mesh = new MeshRenderer();

        meshBox = new Bounds(meshNode.transform.position, Vector3.zero);
        bSphere = new BoundingSphere();

        for (int i = 0; i < meshPositionNode.transform.childCount; i++)
        {
            Transform child = meshPositionNode.transform.GetChild(i);

            if (child.tag != "Annotation")
            {

                for (int j = 0; j < child.childCount; j++)
                {

                    Transform childChild = child.GetChild(j);

                    mesh = childChild.GetComponent<MeshRenderer>();

                    if (mesh != null)
                    {
                        meshBox.Encapsulate(mesh.bounds);  //merged alle boundingboxes zu einer großen
                    }

                }
            }
        }
        bSphere.position = meshBox.center;
        bSphere.radius = Mathf.Max(meshBox.extents.x, meshBox.extents.y, meshBox.center.z);

        //Debug.Log(meshBox.extents);

        //Debug.Log(meshBox.center);
        //drawBox();

    }

    private void drawPlane()
    {
        Vector3 y = pointPlaneVectors[0].y;
        Vector3 z = pointPlaneVectors[0].z;
        y = y.normalized;
        z = z.normalized;
        Debug.DrawLine(pointPlaneVectors[0].origin + (2 * z + 2 * y), pointPlaneVectors[0].origin + (2 * z - 2 * y), Color.red, 5f);
        Debug.DrawLine(pointPlaneVectors[0].origin + (2 * z - 2 * y), pointPlaneVectors[0].origin + (-2 * z - 2 * y), Color.red, 5f);
        Debug.DrawLine(pointPlaneVectors[0].origin + (-2 * z - 2 * y), pointPlaneVectors[0].origin + (-2 * z + 2 * y), Color.red, 5f);
        Debug.DrawLine(pointPlaneVectors[0].origin + (-2 * z + 2 * y), pointPlaneVectors[0].origin + (2 * z + 2 * y), Color.red, 5f);
    }

    /**
    * \brief orthoProjectPointOnPlane
    * projiziert einen Punkt Orthogonal auf eine Ebene
    * 
    * \param pointPosition. Position des zu projizierenden Punkt
    * \param planeNumber. Nummer der Ebene auf welche projiziert wird
    * \param planeVector. Koordinatenachsen der Ebene
    * \param planen. Ebene selbst
    * 
    * \return projizierten Punkt
    */
    private Point orthoProjectPointOnPlane(Vector3 pointPosition, int planeNumber, PlaneVectors planeVector, Plane plane)
    {
        Point p = new Point();
        float y;
        float x;
        Vector3 pointPos = pointPosition;
        Vector3 planeOrigin = planeVector.origin;
        Vector3 planeNormal = planeVector.z;

        //hier wird eine orthogonalprojektion von einem Punkt auf eine Fläche durchgeführt               

        float distance = plane.GetDistanceToPoint(pointPos); //hier wird der kürzeste Abstand vom punkt zur fläche bestimmt
        distance *= -2;
        Vector3 projectedPoint = pointPos + planeNormal * distance;

        //Skalarprodukt ist orthogonalprojektion von einem Vektor auf einen anderen
        // x und y koordinaten in der Fläche des projezierten Punktes
        y = Vector3.Dot(projectedPoint - planeOrigin, planeVector.y);
        x = Vector3.Dot(projectedPoint - planeOrigin, planeVector.x);

        //Winkelberechnung
        //Vector3.Angle gibt einen winkel zwischen 0 und 180 grad zurück. Benötigt wird ein Winkel zwischen 0 und 360 grad.
        //Es wird geprüft ob der punkt links oder rechts von der Y-Achse liegt. Abhängig davon kann man den Winkel auf die 360 hochrechnen

        float angle = Vector3.Angle(projectedPoint - planeOrigin, planeOrigin + 2 * planeVector.y);

        if (x >= 0)
        {

        }
        else
        {
            angle = 360 - angle;
        }

        p.y = y;
        p.x = x;

        p.planeNumber = planeNumber;
        p.position = projectedPoint;
        p.angle = angle;

        //Debug.DrawLine(pointPlaneVectors[planeNumber].origin, pointPlaneVectors[planeNumber].origin + z * pointPlaneVectors[planeNumber].z);
        //Debug.DrawLine(pointPlaneVectors[planeNumber].origin, pointPlaneVectors[planeNumber].origin + y * pointPlaneVectors[planeNumber].y);
        //Debug.DrawLine(pointPlaneVectors[planeNumber].origin, projectedPoint);

        return p;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(meshBox.center, meshBox.size);
    }

    /**
    * \brief overlapLabelOnLabel
    * prüft ob zwei Label sich überlappen
    * 
    * \param li. Label 1
    * \param lj. Label 2
    * \return true falls überlappung
    */
    private bool overlapLabelOnLabel(Label li, Label lj) /// 0 < overlap < 1,   0 no overlap, 1 full overlap 
    {
        float[] overlap = new float[2];
        float widthi = 0;
        float heighti = 0;
        float widthj = 0;
        float heightj = 0;

        GameObject labeli = li.annotationLabel;
        GameObject labelj = lj.annotationLabel;

        if (labeli != null && labelj != null)
        {
            widthi = labeli.GetComponent<RectTransform>().rect.width * labeli.GetComponent<RectTransform>().localScale.x * labeli.transform.parent.localScale.x * meshNode.transform.localScale.x * meshPositionNode.transform.localScale.x * meshViewerBase.transform.localScale.x;
            heighti = labeli.GetComponent<RectTransform>().rect.height * labeli.GetComponent<RectTransform>().localScale.x * labeli.transform.parent.localScale.x * meshNode.transform.localScale.x * meshPositionNode.transform.localScale.x * meshViewerBase.transform.localScale.x;
            widthj = labelj.GetComponent<RectTransform>().rect.width * labelj.GetComponent<RectTransform>().localScale.x * labeli.transform.parent.localScale.x * meshNode.transform.localScale.x * meshPositionNode.transform.localScale.x * meshViewerBase.transform.localScale.x;
            heightj = labelj.GetComponent<RectTransform>().rect.height * labelj.GetComponent<RectTransform>().localScale.x * labeli.transform.parent.localScale.x * meshNode.transform.localScale.x * meshPositionNode.transform.localScale.x * meshViewerBase.transform.localScale.x;
        }

        //daten für i Object
        float yi = li.y;
        float xi = li.x;

        //Debug.Log(widthi);

        //daten für j Object
        float yj = lj.y;
        float xj = lj.x;

        //overlap in y richtung

        if (yi == yj)
        {
            overlap[0] = 1;
        }
        else if (Mathf.Abs(yi - yj) <= ((heighti / 2) + (heightj / 2)))
        {
            overlap[0] = 1;
        }
        else
        {
            overlap[0] = 0;
        }

        //overlap in z richtung

        if (xi == xj)
        {
            overlap[1] = 1;
        }
        else if (Mathf.Abs(xi - xj) <= ((widthi / 2) + (widthj / 2)))
        {
            overlap[1] = 1;
        }
        else
        {
            overlap[1] = 0;
        }

        if (overlap[0] == 1 && overlap[1] == 1)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /**
    * \brief calculateEnergy
    * berechnet die Energie einer Ebene
    * 
    * \param whichPlanes. bestimmt auf der Welcher Ebene die Energie berechnet werden soll
    */
    private void calculateEnergy(bool[] whichPlanes)
    {
        for (int p = 0; p < labelLists.Length; p++)
        {
            if (whichPlanes[p])
            {
                energyOfPlanes[p] = 0;

                for (int j = 0; j < labelLists[p].Count; j++)
                {
                    for (int l = j; l < labelLists[p].Count; l++)
                    {
                        if (j != l)
                        {
                            if (overlapLabelOnLabel(labelLists[p][j], labelLists[p][l]))
                            {
                                energyOfPlanes[p]++;
                            }
                        }
                    }
                }
            }
        }
    }

    /**
    * \brief calculateSilhouette
    * bestimmt den Punkt auf der Silhouette und berechnet den Abstand zu Mittelpunkt in der Ebene
    * 
    * \param position. bestimmt auf der Welcher Ebene die Energie berechnet werden soll
    * \param cam. Mainkamera durch welche d
    * \param image. Bild auf dem gearbeitet wird
    * \param planeNumber.
    */
    private float calculateSilouhette(Vector3 position, Camera cam, Texture2D image, int planeNumber)
    {
        float radius = 0;
        float maxRadius = getInitialPlaneRadius(planeNumber);

        //position und das zentrum der Ebene werden auf die Silhouette kameraebene projiziert

        Vector3 centerIn3DOnCamPlane = cam.WorldToScreenPoint(labelPlaneVectors[1].origin);
        Vector2 centerIn2D = new Vector2(centerIn3DOnCamPlane.x, centerIn3DOnCamPlane.y);

        float distance = centerIn3DOnCamPlane.z;



        Vector3 pointPosIn3D = cam.WorldToScreenPoint(position);
        Vector2 pointPosIn2D = new Vector2(pointPosIn3D.x, pointPosIn3D.y);



        Vector2 pEdge = bresenhamLine((int)pointPosIn2D.x, (int)pointPosIn2D.y, (int)centerIn2D.x, (int)centerIn2D.y, image);

        Vector3 pEdgeInWorld = cam.ScreenToWorldPoint(new Vector3(pEdge.x, pEdge.y, distance));

        Vector3 centerIn3DInWorld = cam.ScreenToWorldPoint(centerIn3DOnCamPlane);
        centerIn3DInWorld = labelPlaneVectors[1].origin;

        radius = (pEdgeInWorld - centerIn3DInWorld).magnitude;

        if (radius <= 0)
        {
            radius = maxRadius;
        }
        else if (radius > maxRadius)
        {
            radius = maxRadius;
        }

        //Debug.DrawLine(centerIn3DInWorld, centerIn3DInWorld + ((pEdgeInWorld- centerIn3DInWorld).normalized * radius), Color.red, 0.020f);
        Debug.DrawRay(centerIn3DInWorld, (pEdgeInWorld - centerIn3DInWorld).normalized * radius, Color.red, 0.20f);

        return radius;
    }

    /**
    * \brief bresenhamLine
    * läuft mittels des Bresenham Algorithmus von Startpunkt zu Endpunkt jeden Pixel in image ab
    * 
    * \param image. Bild auf dem die Pixel betrachtet werden sollen
    * \param x. X-Koordinate der Startposition
    * \param y. Y-Koordinate der Startposition
    * \param x2. X-Koordinate der Endposition
    * \param y2. Y-Koordinate der Endposition
    */
    private Vector2 bresenhamLine(int x, int y, int x2, int y2, Texture2D image)
    {
        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);
        if (!(longest > shortest))
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;

        for (int i = 0; i <= longest; i++)
        {


            if (image.GetPixel(x, y) != Color.white)
            {
                image.SetPixel(x, y, Color.blue);

                //for (int n  = 0; n < 5; n++)
                //{
                //image.SetPixel(x, y + n, Color.blue);
                //image.SetPixel(x + n, y, Color.blue);
                //image.SetPixel(x, y - n, Color.blue);
                //image.SetPixel(x - n, y, Color.blue);

                //image.SetPixel(x + n, y + n, Color.blue);
                //image.SetPixel(x - n, y - n, Color.blue);
                //image.SetPixel(x + n, y - n, Color.blue);
                //image.SetPixel(x - n, y + n, Color.blue);

                //}              

                Vector2 pos = new Vector2(x, y);
                return pos;
            }
            else
            {
                //image.SetPixel(x, y, Color.red);
                //image.SetPixel(x, y + 1, Color.red);
                //image.SetPixel(x + 1, y, Color.red);
                //image.SetPixel(x, y - 1, Color.red);
                //image.SetPixel(x - 1, y, Color.red);

                //image.SetPixel(x2, y2, Color.red);
                //image.SetPixel(x2, y2 + 1, Color.red);
                //image.SetPixel(x2 + 1, y2, Color.red);
                //image.SetPixel(x2, y2 - 1, Color.red);
                //image.SetPixel(x2 - 1, y2, Color.red);

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        Vector2 center = new Vector2(0, 0);

        return center;
    }

    /**
    * \brief takePicture
    * hier wird eine Kamera erstellt welche ein Bild nur von den Objekten macht welche auf der "MeshViewer" layer liegen 
    * aus diesem Bild wird die Silhouette brechnet
    */
    private void takePicture()
    {
        //hier wird eine Kamera erstellt welche ein Bild nur von den Objekten macht welche auf der "MeshViewer" layer liegen 
        //aus diesem Bild wird die Silhouette brechnet

        //RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = silhouetteCam.targetTexture;
        silhouetteCam.Render();
        silhouetteImage = new Texture2D(silhouetteCam.targetTexture.width, silhouetteCam.targetTexture.height);
        silhouetteImage.ReadPixels(new Rect(0, 0, silhouetteCam.targetTexture.width, silhouetteCam.targetTexture.height), 0, 0);
        silhouetteImage.Apply();
        //RenderTexture.active = currentRT;
    }

    /**
    * \brief drawPicture
    * speichert das in takePicture erstellte Bild in eine Datei
    */
    private void drawPicture()
    {
        //draw Picture
        Byte[] b = silhouetteImage.EncodeToPNG();

        File.WriteAllBytes("C:/Users/Matthias/Desktop/Image.png", b);
    }

    /**
    * \brief createPie
    * erstellt 36 Punkte mittels welcher die Silhouette abgetastet werden
    */
    private void createPie() // hier werden Kuchenstücke erstellt um die Silhouette zu berechnen. Jedes Kuchenstück hat einen eigenen Radius
    {
        Point p = new Point();

        p.y = bSphere.radius;
        p.position = labelPlaneVectors[1].origin + (initialPlaneRadius[1]) * labelPlaneVectors[1].y;
        p.x = 0;
        p.z = 0;
        float xNew;
        float yNew;
        float rotangle;

        for (int i = 0; i < rads.Length; i++)
        {
            rads[rads.Length - 1 - i] = (((Camera.main.transform.position - labelPlaneVectors[2].origin).magnitude) * calculateSilouhette(p.position, silhouetteCam, silhouetteImage, 1)) / ((Camera.main.transform.position - meshBox.center).magnitude);

            rotangle = 1 * Mathf.Deg2Rad;

            xNew = p.x * Mathf.Cos(rotangle) - p.y * Mathf.Sin(rotangle);
            yNew = p.x * Mathf.Sin(rotangle) + p.y * Mathf.Cos(rotangle);

            p.x = xNew;
            p.y = yNew;

            p.position = labelPlaneVectors[1].origin + labelPlaneVectors[1].y * p.y + labelPlaneVectors[1].x * p.x;
        }
    }

    /**
    * \brief findSmallesPossibleRadius
    * hier wird ausgewertet in welchem Kuchenstück das Label liegt
    * 
    * \param l. Label
    * \return radius des Kuchenstücks
    */
    private float findSmallestPossibleRadius(Label l)
    {
        int angleSize = (int)(l.angleToLeftAndRightCorner[0] + l.angleToLeftAndRightCorner[1]);

        if (angleSize <= 0)
        {
            angleSize = 1;
        }

        int startAngle;

        if (l.angleToLeftAndRightCorner[0] < l.angle)
        {
            startAngle = (int)(l.angle - l.angleToLeftAndRightCorner[0]);

        }
        else
        {
            startAngle = 360 - Mathf.Abs((int)(l.angle - l.angleToLeftAndRightCorner[0]));

        }
        float[] rads1 = new float[angleSize];

        if ((startAngle + angleSize) < 360)
        {
            for (int i = 0; i < angleSize; i++)
            {
                rads1[i] = rads[startAngle + i];

            }
        }
        else
        {
            for (int i = 0; i < 360 - startAngle; i++)
            {
                rads1[i] = rads[startAngle + i];
            }

            for (int i = 0; i < angleSize - (360 - startAngle); i++)
            {
                float ko = rads[i];
                rads1[(360 - startAngle) + i] = ko;
            }

        }
        Array.Sort(rads1);
        return rads1[rads1.Length - 1];
    }
}
