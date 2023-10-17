using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BubbleTrigger : MonoBehaviour
{
    [Range(0, 10)]
    public int mSesitivity = 1;
    private Camera mCamera = null;
    private GameObject bubble;

    private void Awake()
    {
        MeasureDepth.onTriggerPoints += OnTriggerPoints;

        mCamera = GameObject.Find("OfflinePivot").transform.Find("BubbleCamera").GetComponent<Camera>();
    }

    private void OnDestroy()
    {
        MeasureDepth.onTriggerPoints -= OnTriggerPoints;
    }

    private void OnTriggerPoints(List<Vector2> triggerPoints)
    {
        if (!enabled)
            return;


        foreach (Vector2 point in triggerPoints)
        {
            bool isOnce = true;
            Vector2 flippedY = new Vector3(point.x, mCamera.pixelHeight - point.y);
            int layerMask = 1 << LayerMask.NameToLayer("bubble");
            Ray ray = mCamera.ScreenPointToRay(flippedY);
            if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                
                if (isOnce)
                {
                    bubble = hit.collider.gameObject;
                    hit.collider.gameObject.GetComponent<Bubble>().BubbleDestroyEffect();
                    isOnce = false;
                    //Destroy(hit.collider.gameObject);
                    PhotonNetwork.Destroy(hit.collider.gameObject);
                }
            }
        }
    }
}
