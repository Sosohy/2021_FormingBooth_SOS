using UnityEngine;

[CreateAssetMenu(fileName = "TelephoneObjData", menuName = "ScriptableObjects/telephoneObjData", order = 1)]
public class TelephoneObjData : ScriptableObject
{
    public int count;
    public int[] numbers;
}