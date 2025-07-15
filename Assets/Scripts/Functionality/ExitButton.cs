using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    private GameManager m_GameManager;
    private Button m_GetCurrentButton;

    private void Start()
    {
        m_GameManager = FindObjectOfType<GameManager>();
        m_GetCurrentButton = GetComponent<Button>();
        OnClickDetection();
    }

    private void OnClickDetection()
    {
        m_GetCurrentButton.onClick.RemoveAllListeners();
        m_GetCurrentButton.onClick.AddListener(() =>
        {
            m_GameManager.m_AudioController.m_Click_Audio.Play();
            m_GameManager.m_PopObject();
            // if (!m_GameManager.isExit)
            // {
            //     m_GameManager.m_PopObject();
            // }
        });
    }
}
