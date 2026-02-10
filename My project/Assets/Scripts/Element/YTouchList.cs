using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    //================================================================================  Constant  ================================================================================
    private readonly static int VELOCITY_SMOOTHING_FRAMES = 3;

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
    protected UIStateMachineBase<state> _stateMachine = null;

    private Vector2 _lastDragDelta = Vector2.zero;
    private Vector2 _smoothedDragDelta = Vector2.zero;
    private Vector2 _inertiaDelta = Vector2.zero;
    private bool _isDragging = false;
    private bool _isInertiaing = false;
    private Queue<float> _velocityQueue = new Queue<float>(VELOCITY_SMOOTHING_FRAMES);
    private float _velocity = 0;

    //================================================================================  LifeCycle  ================================================================================
    protected override void Awake()
    {
        base.Awake();

        _stateMachine = new UIStateMachineBase<state>();
        _stateMachine.RegisterState(new UIStateBase<state>(state.Idle, onIdleStateEntryAction));
        _stateMachine.RegisterState(new UIStateBase<state>(state.Dragging, onDraggingStateEntryAction, onDraggingStateUpdateAction, onDraggingStateExitAction));
        _stateMachine.RegisterState(new UIStateBase<state>(state.Inertia, onInertiaStateEntryAction, onInertiaUpdateAction, onInertiaExitAction));
        _stateMachine.ChangeStateTo(state.Idle);
    }

    private void Update()
    {
        if (_isDragging || _isInertiaing)
        {
            _stateMachine.Update();
        }
    }

    //================================================================================  Interface Implementation  ================================================================================
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        _stateMachine.ChangeStateTo(state.Idle);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _stateMachine.ChangeStateTo(state.Dragging);
        _isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _inertiaDelta = _smoothedDragDelta;
        _smoothedDragDelta = Vector2.zero;
        _stateMachine.ChangeStateTo(state.Inertia);
        _isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _lastDragDelta += eventData.delta;     //Prevent update and onDrag from being out of sync and causing data errors
        _smoothedDragDelta = Vector2.Lerp(_smoothedDragDelta, eventData.delta, 0.5f);
    }

    //================================================================================  Functions  ================================================================================
    protected void onIdleStateEntryAction()
    {
        _isDragging = _isInertiaing = false;
    }

    protected void onDraggingStateEntryAction()
    {
        _isDragging = true;
        _velocity = 0f;
    }

    protected void onDraggingStateUpdateAction()
    {
        Vector2 move = _lastDragDelta;
        _lastDragDelta = Vector2.zero;

        if (m_movingDirection == MovingDirection.Horizontal)
        {
            move.y = 0;
        }
        else if (m_movingDirection == MovingDirection.Vertical)
        {
            move.x = 0;
        }

        controlContentMove(move);

        if (_velocityQueue.Count >= VELOCITY_SMOOTHING_FRAMES)
            _velocityQueue.Dequeue();
        _velocityQueue.Enqueue(move.magnitude / Time.deltaTime);
    }

    protected void onDraggingStateExitAction()
    {
        _isDragging = false;
    }

    protected void onInertiaStateEntryAction()
    {
        var count = _velocityQueue.Count;
        while(_velocityQueue.Count > 0)
        {
            _velocity += _velocityQueue.Dequeue();
        }
        _velocity /= count;
        _velocityQueue.Clear();

        if(_velocity < m_inertiaThreshold)
        {
            _stateMachine.ChangeStateTo(state.Idle);
            return;
        }

        if (m_movingDirection == MovingDirection.Horizontal)
        {
            _inertiaDelta.y = 0;
        }
        else if (m_movingDirection == MovingDirection.Vertical)
        {
            _inertiaDelta.x = 0;
        }

        _isInertiaing = true;
    }

    protected void onInertiaUpdateAction()
    {
        Vector2 move = _inertiaDelta.normalized * _velocity * Time.deltaTime;

        controlContentMove(move);

        _velocity *= m_decelerationRate;
        if (_velocity <= m_inertiaThreshold)
        {
            _velocity = 0;
            _stateMachine.ChangeStateTo(state.Idle);
        }
    }

    protected void onInertiaExitAction()
    {
        _isInertiaing = false;
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
