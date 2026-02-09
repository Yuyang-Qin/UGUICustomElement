using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.ScrollRect;

[AddComponentMenu("YGUI/Touch List", 1)]
[SelectionBase]
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class YTouchList : UIBehaviour
{
    public enum MovingDirection
    {
        Horizontal,
        Vertical,
        Free
    }

    private enum state
    {
        Idle,
        Dragging,
        Inertia
    }

    [SerializeField]
    private MovingDirection m_movingDirection = MovingDirection.Vertical;
    /// <summary>
    /// Limit scrollable directions based on settings
    /// </summary>
    public MovingDirection movingDirection { get { return m_movingDirection; } set { m_movingDirection = value; } }

    [SerializeField]
    private MovementType m_movementType = MovementType.Elastic;

    /// <summary>
    /// The behavior to use when the content moves beyond the scroll rect.
    /// </summary>
    public MovementType movementType { get { return m_movementType; } set { m_movementType = value; } }

    private UIStateMachineBase<state> m_stateMachine = null;

    protected override void Awake()
    {
        base.Awake();

        m_stateMachine = new UIStateMachineBase<state>();
        m_stateMachine.RegisterState(new UIStateBase<state>(state.Idle));   //TODO
        m_stateMachine.RegisterState(new UIStateBase<state>(state.Dragging));   //TODO
        m_stateMachine.RegisterState(new UIStateBase<state>(state.Inertia));    //TODO
        m_stateMachine.ChangeStateTo(state.Idle);
    }
}
