using System;
using System.Collections.Generic;
using UnityEngine;


public class UIStateBase<T> where T : Enum
{
    private T m_stateKey;
    public T stateKey {  get { return m_stateKey; } }

    public UIStateBase(T key, Action onEntryAction = null, Action onUpdateAction = null, Action onExitAction = null)
    {
        m_stateKey = key;
        OnEntry = onEntryAction;
        OnUpdate = onUpdateAction;
        OnExit = onExitAction;
    }

    public Action OnEntry;
    public Action OnUpdate;
    public Action OnExit;
}

public class UIStateMachineBase<T> where T : Enum
{
    private Dictionary<T, UIStateBase<T>> m_states = new Dictionary<T, UIStateBase<T>>();
    private UIStateBase<T> m_curState;
    private bool m_canUpdate = false;

    public void RegisterState(UIStateBase<T> state) => m_states[state.stateKey] = state;

    public void ChangeStateTo(T nextStateKey)
    {
        if(!m_states.ContainsKey(nextStateKey))
        {
            Debug.LogError($"UISateMachine change state Error :: has dont register state(\"{nextStateKey.ToString()}\"), check plz!");
            return;
        }

        if(m_curState != null && m_curState.stateKey.Equals(nextStateKey))
        {
            Debug.Log($"UISateMachine :: Enter the state(\"{nextStateKey.ToString()}\") repeatedly, and the switching operation is cancelled.");
            return;
        }

        if(m_curState != null)
        {
            m_canUpdate = false;
            m_curState?.OnExit?.Invoke();
        }

        m_curState = m_states[nextStateKey];
        m_curState?.OnEntry?.Invoke();
        m_canUpdate = true;

    }

    public void Update()
    {
        if(m_curState != null && m_canUpdate)
        {
            m_curState?.OnUpdate?.Invoke();
        }
    }
}
