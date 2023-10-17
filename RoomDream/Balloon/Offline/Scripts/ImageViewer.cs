using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour
{
    public MultiSourceManager multiSource;
    public MeasureDepth measureDepth;

    public RawImage rawImage;
    public RawImage rawDepth;

    void Update()
    {
        rawImage.texture = multiSource.GetColorTexture();

        rawDepth.texture = measureDepth.mDepthTexture;
    }
}
