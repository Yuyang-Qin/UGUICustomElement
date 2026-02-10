using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.ScrollRect;

[AddComponentMenu("YGUI/Touch List", 1)]
[SelectionBase]
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class YTouchList : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public enum MovingDirection
    {
        Horizontal,
        Vertical,
        Free
    }

    protected enum state
    {
        Idle,
        Dragging,
        Inertia
    }

    //================================================================================  Getter/Setter  ================================================================================
    [SerializeField]
    private RectTransform m_content;
    public RectTransform content { get { return m_content; } set { m_content = value; } }

    [SerializeField]
    private MovingDirection m_movingDirection = MovingDirection.Free;
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

    [SerializeField]
    private float m_inertiaThreshold = 0.1f;
    /// <summary>
    /// Sliding threshold, the larger the value, the harder it is to slide
    /// </summary>
    public float inertiaThreshold { get { return m_inertiaThreshold; } set { m_inertiaThreshold = value; } }

    [SerializeField, Range(0.1f, 0.9f)]
    private float m_decelerationRate = 0.8f;
    /// <summary>
    /// The smaller the value, the faster the deceleration
    /// </summary>
    public float decelerationRate { get { return m_decelerationRate; } set { m_decelerationRate = Mathf.Clamp(value, 0.1f, 0.9f); } }

    //================================================================================  Variable  ================================================================================
    protected UIStateMachineBase<state> m_stateMachine = null;

    private Vector2 m_lastDragDelta = Vector2.zero;
    private Vector2 m_smoothedDragDelta = Vector2.zero;
    private Vector2 m_inertiaDelta = Vector2.zero;
    private bool isDragging = false;
    private bool isInertiaing = false;
    private float velocity = 0;

    //================================================================================  LifeCycle  ================================================================================
    protected override void Awake()
    {
        base.Awake();

        m_stateMachine = new UIStateMachineBase<state>();
        m_stateMachine.RegisterState(new UIStateBase<state>(state.Idle, onIdleStateEntryAction));
        m_stateMachine.RegisterState(new UIStateBase<state>(state.Dragging, onDraggingStateEntryAction, onDraggingStateUpdateAction, onDraggingStateExitAction));
        m_stateMachine.RegisterState(new UIStateBase<state>(state.Inertia, onInertiaStateEntryAction, onInertiaUpdateAction, onInertiaExitAction));
        m_stateMachine.ChangeStateTo(state.Idle);
    }

    private void Update()
    {
        if (isDragging || isInertiaing)
        {
            m_stateMachine.Update();
        }
    }

    //================================================================================  Interface Implementation  ================================================================================
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        m_stateMachine.ChangeStateTo(state.Idle);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_stateMachine.ChangeStateTo(state.Dragging);
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        m_inertiaDelta = m_smoothedDragDelta;
        m_smoothedDragDelta = Vector2.zero;
        m_stateMachine.ChangeStateTo(state.Inertia);
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        m_lastDragDelta += eventData.delta;     //Prevent update and onDrag from being out of sync and causing data errors
        m_smoothedDragDelta = Vector2.Lerp(m_smoothedDragDelta, eventData.delta, 0.5f);
    }

    //================================================================================  Functions  ================================================================================
    protected void onIdleStateEntryAction()
    {
        isDragging = isInertiaing = false;
    }

    protected void onDraggingStateEntryAction()
    {
        isDragging = true;
        velocity = 0f;
    }

    protected void onDraggingStateUpdateAction()
    {
        Vector2 move = m_lastDragDelta;
        m_lastDragDelta = Vector2.zero;

        if (m_movingDirection == MovingDirection.Horizontal)
        {
            move.y = 0;
        }
        else if (m_movingDirection == MovingDirection.Vertical)
        {
            move.x = 0;
        }

        controlContentMove(move);

        velocity = Mathf.Max(move.magnitude / Time.deltaTime, velocity);
    }

    protected void onDraggingStateExitAction()
    {
        isDragging = false;
    }

    protected void onInertiaStateEntryAction()
    {
        if(velocity < m_inertiaThreshold)
        {
            m_stateMachine.ChangeStateTo(state.Idle);
            return;
        }

        isInertiaing = true;
    }

    protected void onInertiaUpdateAction()
    {
        Vector2 move = Vector2.zero;

        move = m_inertiaDelta.normalized * velocity * Time.deltaTime;
        if (m_movingDirection == MovingDirection.Horizontal)
        {
            move.y = 0;
        }
        else if (m_movingDirection == MovingDirection.Vertical)
        {
            move.x = 0;
        }

        controlContentMove(move);

        velocity *= m_decelerationRate;
        if (velocity <= 0.1f)
        {
            velocity = 0;
            m_stateMachine.ChangeStateTo(state.Idle);
        }
    }

    protected void onInertiaExitAction()
    {
        isInertiaing = false;
    }

    protected void controlContentMove(Vector2 move)
    {
        if (m_content == null)
        {
            Debug.LogError("TouchList :: controlContentMove has Error :: dont found content, check plz!");
            return;
        }

        m_content.anchoredPosition += move;
    }
}
