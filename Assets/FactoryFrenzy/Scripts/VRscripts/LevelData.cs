using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    public string levelName;
    public List<PieceData> pieces = new List<PieceData>();
}

[Serializable]
public class PieceData
{
    public string typeId;          // "tree", "moving_platform"... must match the PC prefab IDs
    public Vector3 position;
    public Vector3 rotation;       // euler angles
    public bool locked;
    public bool isMoving;          // true only for moving platforms
    public MovingData moving = new MovingData();
}

[Serializable]
public class MovingData
{
    public Vector3 start;
    public Vector3 end;
    public float travelTime;
    public Vector3 rotationAxis;   // optional rotation of MovingPlatform
    public float rotationSpeed;
}