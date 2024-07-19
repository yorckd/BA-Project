using UnityEngine;
using UnityEngine.Networking;
using Unity.WebRTC;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class WebRTCClient : MonoBehaviour
{
    private RTCPeerConnection localConnection;

    private string signallingServerUrl = "http://localhost:8888";
    private string answererId = "answerer01";
    private RTCDataChannel receiveChannel;

    private string remoteSdp;

    void Start()
    {
        Debug.Log("Starting");
        StartCoroutine(WebRTC.Update());
        StartCoroutine(setupPeerConnection());
    }

    private IEnumerator setupPeerConnection()
    {
        localConnection = new RTCPeerConnection();
        localConnection.OnDataChannel = ReceiveChannelCallback;

        // get offer from signalling server
        var getOfferURI = (signallingServerUrl + "/get_offer");
        yield return StartCoroutine(getRemoteOffer(getOfferURI));
        var remoteDescription = new RTCSessionDescription { sdp = remoteSdp, type = RTCSdpType.Offer };
        Debug.Log("Received SDP: " + remoteDescription.sdp);

        localConnection.SetRemoteDescription(ref remoteDescription);

        //create answer and send to signalling server
        var answer = localConnection.CreateAnswer();
        yield return answer;

        var localDescription = answer.Desc;
        localConnection.SetLocalDescription(ref localDescription);
        var answerDict = new Dictionary<string, string>
        {
            { "id", answererId },
            { "type", "answer" },
            { "sdp", localDescription.sdp }
        };

        /* setup video track
        WebCamTexture webCamFrame = new WebCamTexture();
        webCamFrame.Play();
        var videoTrack = new VideoStreamTrack(webCamFrame);
        localConnection.AddTrack(videoTrack);*/

        var camera = Camera.main;
        var videoTrack = camera.CaptureStreamTrack(1280, 720);

        localConnection.AddTrack(videoTrack);

        var message = JsonConvert.SerializeObject(answerDict);
        Debug.Log(message.GetType());
        Debug.Log("Message: " + message);
        var answerURI = (signallingServerUrl + "/answer");
        StartCoroutine(uploadLocalAnswer(answerURI, answerDict));

        localConnection.OnIceConnectionChange = state =>
        {
            Debug.Log(state);
        };
        localConnection.OnTrack = state =>
        {
            Debug.Log("Track: " + state);
        };
    }

    private IEnumerator getRemoteOffer(string uri)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + request.error);
            }
            else
            {
                Debug.Log("Received: " + request.downloadHandler.text);
                var receivedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
                Debug.Log(receivedData);
                remoteSdp = receivedData["sdp"];
            }
        }
    }

    private IEnumerator uploadLocalAnswer(string uri, Dictionary<string, string> message)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(uri, message))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + request.error);
            }
            else
            {
                Debug.Log("SDP upload completed!");
            }
        }
    }

    void ReceiveChannelCallback(RTCDataChannel channel)
    {
        receiveChannel = channel;
        receiveChannel.OnMessage = HandleReceiveMessage;
    }
    void HandleReceiveMessage(byte[] bytes)
    {
        var message = System.Text.Encoding.UTF8.GetString(bytes);
        Debug.Log(message);
    }

    private void OnDestroy()
    {
        receiveChannel.Close();

        localConnection.Close();
    }
}
