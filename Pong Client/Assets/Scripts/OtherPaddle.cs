using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPaddle : MonoBehaviour
{
    public Rigidbody rig;
    public Vector3 Sync;
    public float speed;

    float SnapThreshold = 1;
    void Start()
    {
        rig = GetComponent<Rigidbody>();
    }
    //  IEnumerator ModelFollowServerPos()
    //  {
    //used to store difference between client model and server position
    //  Vector3 delta;
    //  float dist;
    //  while (!isServer)
    //  {
    //     dist = Vector3.Distance(Sync, transform.position);
    //   if (dist > SnapThreshold) ;// SnapModel();

    //  transform.position += syncVelocity * Time.deltaTime;
    // delta = syncPosition - transform.position;
    // transform.position += delta * Time.deltaTime * correctionInfluence;
    //    yield return 0;
    //}
    //    }
}
