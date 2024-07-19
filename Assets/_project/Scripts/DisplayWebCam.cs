using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayWebCam : MonoBehaviour
{
    [SerializeField] private RawImage img = default;
    private WebCamTexture webCam;

    void Start()
    {
        webCam = new WebCamTexture();
        if(!webCam.isPlaying) webCam.Play();
        img.texture = webCam;
    }
}
