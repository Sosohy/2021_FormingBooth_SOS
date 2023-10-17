using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/playerData", order = 1)]
public class PlayerData : ScriptableObject
{
    public string email; // ����� �̸���
    public string date; // ���� ��¥
    public int round; // ���� ȸ��
    public string currentScene; // ���� ����ڰ� �ִ� scene
    public bool isExist; // ���� ����ڰ� ���� �ִ� ��
}