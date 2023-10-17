using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using RenderHeads.Media.AVProVideo;

public class GetStreamingLinks : MonoBehaviour, IPunObservable
{
    public string roomReal { get; set; }
    public string roomDream { get; set; }
    public string bubbleZone { get; set; }
    public string closet { get; set; }
    public string attic { get; set; }

    bool real = false;
    public static bool dream = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name.Equals("RoomRealScene") && !real) {
            MediaPlayer mediaPlayer = GameObject.Find("StreamingData").GetComponent<MediaPlayer>();
            mediaPlayer.OpenMedia(new MediaPath(roomReal, MediaPathType.AbsolutePathOrURL), autoPlay: true);

            real = true;
        }

        if ((SceneManager.GetActiveScene().name.Equals("RoomDreamScene") || SceneManager.GetActiveScene().name.Equals("RoomCloset") || SceneManager.GetActiveScene().name.Equals("RoomAttic")) && !dream)
        {
            GameObject dreamStreaming = GameObject.Find("StreamingObjs");

            MediaPlayer mediaPlayer = dreamStreaming.transform.GetChild(0).GetChild(1).GetComponent<MediaPlayer>();
            mediaPlayer.OpenMedia(new MediaPath(roomDream, MediaPathType.AbsolutePathOrURL), autoPlay: true);

            mediaPlayer = dreamStreaming.transform.GetChild(1).GetChild(1).GetComponent<MediaPlayer>();
            mediaPlayer.OpenMedia(new MediaPath(bubbleZone, MediaPathType.AbsolutePathOrURL), autoPlay: true);

            mediaPlayer = dreamStreaming.transform.GetChild(2).GetChild(1).GetComponent<MediaPlayer>();
            mediaPlayer.OpenMedia(new MediaPath(closet, MediaPathType.AbsolutePathOrURL), autoPlay: true);

            mediaPlayer = dreamStreaming.transform.GetChild(3).GetChild(1).GetComponent<MediaPlayer>();
            mediaPlayer.OpenMedia(new MediaPath(attic, MediaPathType.AbsolutePathOrURL), autoPlay: true);

            dream = true;
        }

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(roomReal);
            stream.SendNext(roomDream);
            stream.SendNext(bubbleZone);
            stream.SendNext(closet);
            stream.SendNext(attic);
        }
        else
        {
            // Network player, receive data
            this.roomReal = (string)stream.ReceiveNext();
            this.roomDream = (string)stream.ReceiveNext();
            this.bubbleZone = (string)stream.ReceiveNext();
            this.closet = (string)stream.ReceiveNext();
            this.attic = (string)stream.ReceiveNext();
        }
    }
}
