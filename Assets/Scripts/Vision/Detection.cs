using UnityEngine;

public struct Detection
{
    public int classID;
    public float score;
    public Rect bbox;
    public Vector3 centerPosition;

    public Detection(int classID, float score, Rect bbox, Vector3 centerPosition)
    {
        this.classID = classID;
        this.score = score;
        this.bbox = bbox;
        this.centerPosition = centerPosition;
    }
}
