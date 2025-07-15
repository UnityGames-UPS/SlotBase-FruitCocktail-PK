using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Popup Stack GameObject List")]
    [SerializeField] private List<GameObject> m_ObjectStackOpen = new List<GameObject>();

    [Header("Reference For UI Manager")]
    [SerializeField] private UIManager m_UIManager;

    [Header("References For Bonus Game Manager")]
    [SerializeField] private BonusGame m_BonusGame;

    [Header("Reference For Audio Controller")]
    [SerializeField] internal AudioController m_AudioController;

    [Header("Menu Buttons References")]
    [SerializeField] private Button m_Paytable_Button;
    [SerializeField] private Button m_Rules_Button;
    [SerializeField] private Button m_Settings_Button;
    [SerializeField] private Button m_Quit_Button;

    [Header("Quit Popup Button References")]
    [SerializeField] private Button m_Quit_Confirm_Button;

    [Header("Bonus Start Button References")]
    [SerializeField] private Button m_Start_Bonus_Button;

    [Header("Menu Panel References")]
    [SerializeField] private GameObject m_Paytable_Object;
    [SerializeField] private GameObject m_Rules_Object;
    [SerializeField] private GameObject m_Settings_Object;
    [SerializeField] private GameObject m_Quit_Object;

    [SerializeField] internal GameObject m_Bonus_Start_Object;

    //NOTE: internal Booleans
    internal bool isExit = false;

    private void Start()
    {
       // OnClickDetection();
    }

    private void OnClickDetection()
    {
        if (m_Paytable_Button) m_Paytable_Button.onClick.RemoveAllListeners();
        if (m_Paytable_Button) m_Paytable_Button.onClick.AddListener(() => { m_PushObject(m_Paytable_Object); m_AudioController.m_Click_Audio.Play(); m_UIManager.CurrentIndex = 0; m_UIManager.ActivatePaytable(0); });

        if (m_Rules_Button) m_Rules_Button.onClick.RemoveAllListeners();
        if (m_Rules_Button) m_Rules_Button.onClick.AddListener(() => { m_PushObject(m_Rules_Object); m_AudioController.m_Click_Audio.Play(); });

        if (m_Settings_Button) m_Settings_Button.onClick.RemoveAllListeners();
        if (m_Settings_Button) m_Settings_Button.onClick.AddListener(() => { m_PushObject(m_Settings_Object); m_AudioController.m_Click_Audio.Play(); });

        if (m_Quit_Button) m_Quit_Button.onClick.RemoveAllListeners();
        if (m_Quit_Button) m_Quit_Button.onClick.AddListener(() => { m_PushObject(m_Quit_Object); m_AudioController.m_Click_Audio.Play(); });

        if (m_Start_Bonus_Button) m_Start_Bonus_Button.onClick.RemoveAllListeners();
        if (m_Start_Bonus_Button) m_Start_Bonus_Button.onClick.AddListener(() => { m_PopObject(); m_BonusGame.StartBonusGame(); m_AudioController.m_Click_Audio.Play(); });

        if (m_Quit_Confirm_Button) m_Quit_Confirm_Button.onClick.RemoveAllListeners();
        if (m_Quit_Confirm_Button) m_Quit_Confirm_Button.onClick.AddListener(() => { m_UIManager.CallOnExitFunction(); isExit = true; m_AudioController.m_Click_Audio.Play(); });

    }

    #region [[===POPUP GAMEOBJECT STACK MANAGEMENT===]]

    internal void m_PushObject(GameObject m_Object)
    {
        // if(m_ObjectStackOpen.Count > 0)
        // {
        //     m_Object.SetActive(true);
        //     m_ObjectStackOpen.Add(m_Object);
        // }
        // else
        // {
        //     m_Object.SetActive(true);
        //     m_ObjectStackOpen.Add(m_Object);
        //     if (!m_UIManager.MainPopup_Object.activeSelf)
        //     {
        //         m_UIManager.MainPopup_Object.SetActive(true);
        //     }
        // }

        m_Object.SetActive(true);
        m_ObjectStackOpen.Add(m_Object);
        if (!m_UIManager.MainPopup_Object.activeSelf)
        {
            m_UIManager.MainPopup_Object.SetActive(true);
        }
    }

    internal void m_PopObject()
    {
        GameObject m_obj = m_ObjectStackOpen[m_ObjectStackOpen.Count - 1];
        if (m_ObjectStackOpen.Count > 1)
        {
            m_obj.SetActive(false);
            m_ObjectStackOpen.Remove(m_obj);
        }
        else
        {
            m_obj.SetActive(false);
            m_ObjectStackOpen.Remove(m_obj);
            if (m_UIManager.MainPopup_Object.activeSelf)
            {
                m_UIManager.MainPopup_Object.SetActive(false);
            }
        }
    }

    #endregion


}
