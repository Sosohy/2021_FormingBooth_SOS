using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/playerData", order = 1)]
public class PlayerData : ScriptableObject
{
    public string email; // 사용자 이메일
    public string date; // 공연 날짜
    public int round; // 공연 회차
    public string currentScene; // 현재 사용자가 있는 scene
    public bool isExist; // 현재 사용자가 들어와 있는 지
}