using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Comp_SMBEventCurrator : MonoBehaviour
{
    
    [SerializeField] private bool m_debug = false;
    [SerializeField] private UnityEvent<string> m_event = new UnityEvent<string>();
    public UnityEvent<string> Event { get => m_event; }

    private void Awake()
    {
        m_event.AddListener(ONSMBEvent);
    }

    private void ONSMBEvent(string eventName) 
    {
        if (m_debug) 
        {
            Debug.Log(eventName);
        }
    }
}
