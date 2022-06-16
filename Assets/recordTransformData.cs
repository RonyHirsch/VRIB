using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class recordTransformData : MonoBehaviour
{
    [SerializeField] Transform transformRef; // object that points forward and that is reference to head and gaze
    [SerializeField] Vector3 relativePosition;
    [SerializeField] float gazeAngle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // calculates head position relative to transformReference which should be placed in player start position.
        relativePosition = transformRef.position - transform.position;

        // calculate gaze angle
        Vector3.Dot(transformRef.forward, transform.forward);

    }
}
