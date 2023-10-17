using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    private GameObject[] menus;

    private string[] links = {"https://www.instagram.com/forming_booth/", "http://naver.me/F3DUjRCU", "https://m.booking.naver.com/booking/12/bizes/621210" };

    private void Start()
    {
        SettingMenuPanels();
    }

    public void OnClickMenuBtn()
    {
        ResetMenuPanels();

        string name = EventSystem.current.currentSelectedGameObject.name;
        GameObject.Find("Main").transform.Find(name + "Panel").gameObject.SetActive(true);
        Debug.Log(name);
    }

    private void SettingMenuPanels()
    {
        ResetMenuPanels();
        GameObject.Find("Main").transform.Find("HomePanel").gameObject.SetActive(true);
    }

    private void ResetMenuPanels()
    {
        if (menus == null)
            menus = GameObject.FindGameObjectsWithTag("menu");

        foreach (GameObject menu in menus)
        {
            string name = menu.name;
            GameObject.Find("Main").transform.Find(name + "Panel").gameObject.SetActive(false);
        }
    }

    public void OnClickLinkBtn()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        string numString = name.Substring(name.Length - 1, 1);
        Application.OpenURL(links[int.Parse(numString)]);
        Debug.Log(links[int.Parse(numString)]);
    }
}
