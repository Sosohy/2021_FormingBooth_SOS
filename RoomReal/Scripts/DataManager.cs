using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using System;

public class DataManager : MonoBehaviour
{
    static string host = "ec2-13-124-121-242.ap-northeast-2.compute.amazonaws.com";
    static int port = 3000;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static IEnumerator getDataFromServer(string api, System.Action<string> callback)
    {
        var url = string.Format("{0}:{1}/{2}", host, port, api);
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("네트워크 환경이 안좋아서 통신을 할수 없습니다.");
        }
        else
        {
            Debug.LogFormat("{0}\n{1}\n{2}", webRequest.responseCode, webRequest.downloadHandler.data, webRequest.downloadHandler.text);
            callback(webRequest.downloadHandler.text);
        }
    }


    public static IEnumerator sendDataToServer(string api, string json, System.Action<string> callback)
    {
        var url = string.Format("{0}:{1}/{2}", host, port, api);
        var webRequest = UnityWebRequest.Get(url);
        var bodyRaw = Encoding.UTF8.GetBytes(json); //직렬화 (문자열 -> 바이트 배열)

        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("네트워크 환경이 안좋아서 통신을 할수 없습니다.");
        }
        else
        {
            Debug.LogFormat("{0}\n{1}\n{2}", webRequest.responseCode, webRequest.downloadHandler.data, webRequest.downloadHandler.text);
            callback(webRequest.downloadHandler.text);
        }
    }

    public static int GetCurrentRound()
    {
        int currentRound = 0;
        string currentHour = DateTime.Now.ToString("HH");
        int currentMin = DateTime.Now.Minute;

        if (currentHour == "12" || currentHour == "13") // 베리어프리
            currentRound = 1;
        else if (currentHour == "14")
            currentRound = 2;
        else if (currentHour == "16")
            currentRound = 3;
        else if (currentHour == "18")
            currentRound = 4;
        else if (currentHour == "19" || currentHour == "20") // 7시와 7시 반 타임
            currentRound = 5;
        else if (currentHour == "21" || currentHour == "22") // 베리어프리
            currentRound = 6;

        // 7시 반 타임 처리
        if (currentHour == "19" && (currentMin < 30))
            currentRound = 0;
        if(currentHour == "20" && (currentMin > 30))
            currentRound = 0;

        // 7시 타임 처리
        if (DateTime.Now.ToString("d").Equals("2022-02-07") && currentHour == "19" && (currentMin < 30))
            currentRound = 5;
        else if (DateTime.Now.ToString("d").Equals("2022-02-07") && currentHour == "20")
            currentRound = 0;

        // 베리어프리 처리
        if (currentHour == "13" && (currentMin > 30))
            currentRound = 0;
        if (currentHour == "22" && (currentMin > 30))
            currentRound = 0;

        return currentRound;
    }
}
