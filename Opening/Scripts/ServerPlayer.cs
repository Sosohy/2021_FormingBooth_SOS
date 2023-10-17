using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityStandardAssets.Utility;

public class ServerPlayer : MonoBehaviour, IPunObservable
{
    private new Rigidbody rigidbody;
    private PhotonView pv;

    private float v;
    private float h;
    private float r;

    private Vector3 currPos;

    public float moveSpeed = 8.0f;
    public float turnSpeed = 0.0f;
    public float jumpPower = 5.0f;

    private float turnSpeedValue = 200.0f;

    RaycastHit hit;
    private Transform tr;

    private IEnumerator Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
        tr = GetComponent<Transform>();

        turnSpeed = 0.0f;
        yield return new WaitForSeconds(0.5f);

        if (pv.IsMine)
        {
            Camera.main.GetComponent<SmoothFollow>().target = transform.Find("CamPivot").transform;
        }
        else
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
        turnSpeed = turnSpeedValue;
    }

    private void Update()
    {
        if (!pv.IsMine)
            return;
        v = Input.GetAxis("Vertical");
        h = Input.GetAxis("Horizontal");
        r = Input.GetAxis("Mouse X");
    }

    private void FixedUpdate()
    {
        if (pv.IsMine)
        {
            Vector3 dir = (Vector3.forward * v) + (Vector3.right * h);
            tr.Translate(dir.normalized * Time.deltaTime * moveSpeed, Space.Self);
            tr.Rotate(Vector3.up * Time.smoothDeltaTime * turnSpeed * r);
        }
        else
        {
            tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * 10.0f);
        }

    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(tr.position);

        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
        }
    }
}
