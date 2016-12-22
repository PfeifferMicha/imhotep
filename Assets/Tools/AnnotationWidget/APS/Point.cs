using UnityEngine;
/**
 * \brief Point.
 * 
 * Dient der Übersicht
 * Alle parameter die ein Punkt im Raum hat
 */
public class Point {
    

    public Vector3 position; /**< Position des Punktes in Weltkoordinaten */
    public float angle;  /**< Winkel zum y-Vektor in einer Ebene */
    public float y, z, x; /**< Koordinaten in der Ebene */
    public int planeNumber; /**< Ebenennummer */
    public GameObject annotationPoint; /**<  Annotationspunkt  */
    public Label label; /**<  Annotationlabel  */

}
