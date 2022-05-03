using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// <summary>
/// A component based on Unity's <a href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.1/api/UnityEngine.InputSystem.UI.VirtualMouseInput.html">VirtualMouseInput</a> 
/// example that creates a virtual <see cref="Mouse"/> device and updates its position based on either Gamepad input or Mouse input (or both).This adds a software mouse cursor 
/// whose only state change is position.
/// </summary>
/// <remarks>
/// The only input the Virtual Mouse Postion device provides will be Position and Delta, along with any associated inputs with those fields. This contrasts with the 
/// <a href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.1/api/UnityEngine.InputSystem.UI.VirtualMouseInput.html">VirtualMouseInput</a> class, which is
/// necessary in order to allow the input of the compoment to be driven by a mouse. 
/// 
/// The reason for this is because a known limitation of the Unity InputSystem package is an inability to differentiate between different mouse devices. Due to this 
/// limitation, if one were to bind the left button on the Virtual Mouse component to the left button on the physical Mouse component, it would result in the 
/// Virtual Mouse's left button never being released after being pressed. This occurs because a left button press on the physical Mouse triggers a left button
/// press on the Virtual Mouse, which is then picked up by the Input System as a left click which is then picked up by the Virtual Mouse once more, resulting in 
/// the left click being registered until the click event is forcibly cancelled by a window change. Similar problems will arise with all other Virtual Mouse buttons.
/// 
/// One possible solution to this problem could be adding a processor to the Virtual Mouse that tells it to ignore input from itself, but this is not 
/// possible currently due to the limitation mentioned earlier that prevents input from different Mouse devices from being differentiated.
/// 
/// The problem does not arise with the Position attribute of this newly created <see cref="Mouse"/>, since the input is taken from the hardware <see cref="Mouse"/>'s 
/// delta rather than the hardware <see cref="Mouse"/>'s position, which keeps the loop discussed previously from occuring.
/// 
/// See Unity's <a href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.1/api/UnityEngine.InputSystem.UI.VirtualMouseInput.html">VirtualMouseInput</a>
/// for more information as to the structure of this class, and how this class is structured.
/// </remarks>
/// <seealso cref="Gamepad"/>
/// <seealso cref="Mouse"/>
[AddComponentMenu("Input/Virtual Mouse (Position Only)")]
public class VirtualMousePositionInput : MonoBehaviour
{

    [Header("Cursor")]
    [SerializeField] private CursorMode m_CursorMode;
    [SerializeField] private CursorMovementMode m_CursorMovementMode;
    [SerializeField] private Graphic m_CursorGraphic;
    [SerializeField] private RectTransform m_CursorTransform;

    [Header("Motion")]
    [SerializeField] private float m_CursorSpeed = 400;
    [SerializeField] private float m_MouseSensitivity = 1.0f;

    [Space(10)]
    [SerializeField] private InputActionProperty m_StickAction;
    [SerializeField] private InputActionProperty m_MouseMoveAction;

    private Canvas m_Canvas; // Canvas that gives the motion range for the software cursor.
    private Mouse m_VirtualMouse;
    private Mouse m_SystemMouse;
    private Action m_AfterInputUpdateDelegate;
    private Action<InputAction.CallbackContext> m_ButtonActionTriggeredDelegate;
    private double m_LastTime;
    private Vector2 m_LastStickValue;


    /// <summary>
    /// Optional transform that will be updated to correspond to the current mouse position.
    /// </summary>
    /// <value>Transform to update with mouse position.</value>
    /// <remarks>
    /// This is useful for having a UI object that directly represents the mouse cursor. Simply add both the
    /// <c>VirtualMouseInput</c> component and an <a href="https://docs.unity3d.com/Manual/script-Image.html">Image</a>
    /// component and hook the <a href="https://docs.unity3d.com/ScriptReference/RectTransform.html">RectTransform</a>
    /// component for the UI object into here. The object as a whole will then follow the generated mouse cursor
    /// motion.
    /// </remarks>
    public RectTransform cursorTransform
    {
        get => m_CursorTransform;
        set => m_CursorTransform = value;
    }

    /// <summary>
    /// How many pixels per second the cursor travels in one axis when the respective axis from
    /// <see cref="stickAction"/> is 1.
    /// </summary>
    /// <value>Mouse speed in pixels per second.</value>
    public float cursorSpeed
    {
        get => m_CursorSpeed;
        set => m_CursorSpeed = value;
    }

    public float mouseSensitivity
    {
        get => m_MouseSensitivity;
        set => m_MouseSensitivity = value;
    }

    /// <summary>
    /// Determines which cursor representation to use. If this is set to <see cref="CursorMode.SoftwareCursor"/>
    /// (the default), then <see cref="cursorGraphic"/> and <see cref="cursorTransform"/> define a software cursor
    /// that is made to correspond to the position of <see cref="virtualMouse"/>. If this is set to <see
    /// cref="CursorMode.HardwareCursorIfAvailable"/> and there is a native <see cref="Mouse"/> device present,
    /// the component will take over that mouse device and disable it (so as for it to not also generate position
    /// updates). It will then use <see cref="Mouse.WarpCursorPosition"/> to move the system mouse cursor to
    /// correspond to the position of the <see cref="virtualMouse"/>. In this case, <see cref="cursorGraphic"/>
    /// will be disabled and <see cref="cursorTransform"/> will not be updated.
    /// </summary>
    /// <value>Whether the system mouse cursor (if present) should be made to correspond with the virtual mouse position.</value>
    /// <remarks>
    /// Note that regardless of which mode is used for the cursor, mouse input is expected to be picked up from <see cref="virtualMouse"/>.
    ///
    /// Note that if <see cref="CursorMode.HardwareCursorIfAvailable"/> is used, the software cursor is still used
    /// if no native <see cref="Mouse"/> device is present.
    /// </remarks>
    public CursorMode cursorMode
    {
        get => m_CursorMode;
        set
        {
            if (m_CursorMode == value)
                return;

            // If we're turning it off, make sure we re-enable the system mouse.
            if (m_CursorMode == CursorMode.HardwareCursorIfAvailable && m_SystemMouse != null)
            {
                InputSystem.EnableDevice(m_SystemMouse);
                m_SystemMouse = null;
            }

            m_CursorMode = value;

            if (m_CursorMode == CursorMode.HardwareCursorIfAvailable)
            {
                TryEnableHardwareCursor();
                // if enabling the hardware cursor fails, we should have the mode set to software cursor
                // uses side effect, so maybe fix that?
                if(TryEnableHardwareCursor() < 0)
                {
                    m_CursorMode = CursorMode.SoftwareCursor;
                }

                // we'll always enable controller movement over mouse movement here, since we either:
                // 1. have hardware cursor enabled
                // 2. can't find any system mouse
                m_CursorMovementMode = CursorMovementMode.Controller;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            else if (m_CursorGraphic != null)
                m_CursorGraphic.enabled = true;

            if(m_CursorMode == CursorMode.SoftwareCursor)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            }
        }
    }

    /// <summary>
    /// Determines which cursor movement method to use. If this is set to <see cref="CursorMovementMode.Mouse"/>
    /// (the default), then the cursor will be controlled by the main mouse input of the game. This requires 
    /// a native <see cref="Mouse"/> device to be present, and requires the <see cref="cursorMode"/> to be set to
    /// <see cref="CursorMode.SoftwareCursor"/>, since mouse sensitivity requires a changing of mouse 
    /// speed that is not feasible to implement on the hardware cursor.
    /// 
    /// If this is set to <see cref="CursorMovementMode.Controller"/>, gamepad input will drive the movement of the mouse. 
    /// This is the original class's intended purpose - refer to the documentation for usage with that class.
    /// </summary>
    /// <value>Whether movement of the virtaul mouse is being driven by a mouse or a controller.</value>
    /// <remarks>
    /// If this property is set to <see cref="CursorMovementMode.Mouse"/> while the <see cref="cursorMode"/> is set to
    /// <see cref="CursorMode.HardwareCursorIfAvailable"/>, the change will not go through and the property will be reset
    /// to <see cref="CursorMovementMode.Controller"/>.
    /// </remarks>
    public CursorMovementMode cursorMovementMode
    {
        get => m_CursorMovementMode;
        set
        {
            m_CursorMovementMode = value;
            if (m_CursorMode == CursorMode.HardwareCursorIfAvailable)
            {
                m_CursorMovementMode = CursorMovementMode.Controller;
            }
            else
            {
                m_CursorMovementMode = value;
            }
        }
    }

    /// <summary>
    /// The UI graphic element that represents the mouse cursor.
    /// </summary>
    /// <value>Graphic element for the software mouse cursor.</value>
    /// <remarks>
    /// If <see cref="cursorMode"/> is set to <see cref="CursorMode.HardwareCursorIfAvailable"/>, this graphic will
    /// be disabled.
    ///
    /// Also, this UI component implicitly determines the <c>Canvas</c> that defines the screen area for the cursor.
    /// The canvas that this graphic is on will be looked up using <c>GetComponentInParent</c> and then the <c>Canvas.pixelRect</c>
    /// of the canvas is used as the bounds for the cursor motion range.
    /// </remarks>
    /// <seealso cref="CursorMode.SoftwareCursor"/>
    public Graphic cursorGraphic
    {
        get => m_CursorGraphic;
        set
        {
            m_CursorGraphic = value;
            TryFindCanvas();
        }
    }

    /// <summary>
    /// The virtual mouse device that the component feeds with positional input.
    /// </summary>
    /// <value>Instance of virtual mouse or <c>null</c>.</value>
    /// <remarks>
    /// This is only initialized after the component has been enabled for the first time. Note that
    /// when subsequently disabling the component, the property will continue to return the mouse device
    /// but the device will not be added to the system while the component is not enabled.
    /// </remarks>
    public Mouse virtualMouse => m_VirtualMouse;

    /// <summary>
    /// The Vector2 stick input that drives the mouse cursor, i.e. <see cref="Mouse.position"/> on
    /// <see cref="virtualMouse"/> and the <a
    /// href="https://docs.unity3d.com/ScriptReference/RectTransform-anchoredPosition.html">anchoredPosition</a>
    /// on <see cref="cursorTransform"/> (if set).
    /// </summary>
    /// <value>Stick input that drives cursor position.</value>
    /// <remarks>
    /// This should normally be bound to controls such as <see cref="Gamepad.leftStick"/> and/or
    /// <see cref="Gamepad.rightStick"/>.
    /// </remarks>
    public InputActionProperty stickAction
    {
        get => m_StickAction;
        set => SetAction(ref m_StickAction, value);
    }

    /// <summary>
    /// The Vector2 Mouse Delta input to move the mouse cursor. It must take a hardware <see cref="Mouse"/> delta 
    /// binding in order to work properly.
    /// </summary>
    public InputActionProperty mouseMoveAction
    {
        get => m_MouseMoveAction;
        set => SetAction(ref m_MouseMoveAction, value);
    }


    protected void OnEnable()
    {
        // Hijack system mouse, if enabled.
        if (m_CursorMode == CursorMode.HardwareCursorIfAvailable)
            TryEnableHardwareCursor();

        // Add mouse device.
        if (m_VirtualMouse == null)
            m_VirtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        else if (!m_VirtualMouse.added)
            InputSystem.AddDevice(m_VirtualMouse);

        // Set initial cursor position.
        if (m_CursorTransform != null)
        {
            var position = m_CursorTransform.anchoredPosition;
            InputState.Change(m_VirtualMouse.position, position);
            m_SystemMouse?.WarpCursorPosition(position);
        }

        // Hook into input update.
        if (m_AfterInputUpdateDelegate == null)
            m_AfterInputUpdateDelegate = OnAfterInputUpdate;
        InputSystem.onAfterUpdate += m_AfterInputUpdateDelegate;

        if(m_CursorMode == CursorMode.SoftwareCursor)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
        

        // Enable actions.
        m_StickAction.action?.Enable();
        m_MouseMoveAction.action?.Enable();
    }

    protected void OnDisable()
    {
        // Remove mouse device.
        if (m_VirtualMouse != null && m_VirtualMouse.added)
            InputSystem.RemoveDevice(m_VirtualMouse);

        // Let go of system mouse.
        if (m_SystemMouse != null)
        {
            InputSystem.EnableDevice(m_SystemMouse);
            m_SystemMouse = null;
        }

        // Remove ourselves from input update.
        if (m_AfterInputUpdateDelegate != null)
            InputSystem.onAfterUpdate -= m_AfterInputUpdateDelegate;

        // Disable actions.
        m_StickAction.action?.Disable();
        m_MouseMoveAction.action?.Disable();

        m_LastTime = default;
        m_LastStickValue = default;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void TryFindCanvas()
    {
        m_Canvas = m_CursorGraphic?.GetComponentInParent<Canvas>();
    }

    // returns 0 if the hardware cursor is successfully enabled, -1 otherwise
    private int TryEnableHardwareCursor()
    {
        var devices = InputSystem.devices;
        for (var i = 0; i < devices.Count; ++i)
        {
            var device = devices[i];
            if (device.native && device is Mouse mouse)
            {
                m_SystemMouse = mouse;
                break;
            }
        }

        if (m_SystemMouse == null)
        {
            if (m_CursorGraphic != null)
                m_CursorGraphic.enabled = true;
            return -1;
        }

        InputSystem.DisableDevice(m_SystemMouse);

        // Sync position.
        if (m_VirtualMouse != null)
            m_SystemMouse.WarpCursorPosition(m_VirtualMouse.position.ReadValue());

        // Turn off mouse cursor image.
        if (m_CursorGraphic != null)
            m_CursorGraphic.enabled = false;

        return 0;
    }

    private void UpdateMotion()
    {
        if (m_VirtualMouse == null)
            return;

        // if we're using a software cursor, we can control it with either the mouse or the gamepad 
        if(cursorMode == CursorMode.SoftwareCursor)
        {
            if(cursorMovementMode == CursorMovementMode.Controller)
            {
                // drive input with controller
                UpdateMotionStick();
            }
            else
            {
                // drive input with mouse
                UpdateMotionMouse();
            }
        }
        // if we're using the hardware cursor, we only accept positional input from the stick (although the 
        // mouse will still move from the hardware mouse, any spped or sensitivity changes will not apply, 
        // since TryEnableHardwareCursor disables regular mouse input)
        else
        {
            // drive input with controller
            UpdateMotionStick();
        }

    }

    private void UpdateMotionMouse()
    {
        // get the action for the mouse delta
        var mouseDeltaAction = m_MouseMoveAction.action;
        if (mouseDeltaAction == null)
            return;

        // if the delta is (0,0), do nothing
        var mouseDelta = mouseDeltaAction.ReadValue<Vector2>();
        if (mouseDelta.Equals(Vector2.zero))
            return;

        

        // represents the current position of the VIRTUAL mouse pointer
        var currentPosition = m_VirtualMouse.position.ReadValue();
        // calculate the new position based on the delta
        var newPosition = currentPosition + (mouseDelta * m_MouseSensitivity);

        ////REVIEW: for the hardware cursor, clamp to something else?
        // Clamp to canvas.
        if (m_Canvas != null)
        {
            // Clamp to canvas.
            var pixelRect = m_Canvas.pixelRect;
            newPosition.x = Mathf.Clamp(newPosition.x, pixelRect.xMin, pixelRect.xMax);
            newPosition.y = Mathf.Clamp(newPosition.y, pixelRect.yMin, pixelRect.yMax);
            Debug.Log(pixelRect.xMax);
        }

        ////REVIEW: the fact we have no events on these means that actions won't have an event ID to go by; problem?
        InputState.Change(m_VirtualMouse.position, newPosition);
        InputState.Change(m_VirtualMouse.delta, mouseDelta);

        // Update software cursor transform, if any.
        if (m_CursorTransform != null &&
                (m_CursorMode == CursorMode.SoftwareCursor ||
                 (m_CursorMode == CursorMode.HardwareCursorIfAvailable && m_SystemMouse == null)))
            m_CursorTransform.anchoredPosition = newPosition;
    }

    private void UpdateMotionStick()
    {

        // Read current stick value.
        var stickAction = m_StickAction.action;
        if (stickAction == null)
            return;
        var stickValue = stickAction.ReadValue<Vector2>();
        if (Mathf.Approximately(0, stickValue.x) && Mathf.Approximately(0, stickValue.y))
        {
            // Motion has stopped.
            m_LastTime = default;
            m_LastStickValue = default;
        }
        else
        {
            var currentTime = InputState.currentTime;
            if (Mathf.Approximately(0, m_LastStickValue.x) && Mathf.Approximately(0, m_LastStickValue.y))
            {
                // Motion has started.
                m_LastTime = currentTime;
            }

            // Compute delta.
            var deltaTime = (float)(currentTime - m_LastTime);
            var delta = new Vector2(m_CursorSpeed * stickValue.x * deltaTime, m_CursorSpeed * stickValue.y * deltaTime);

            // Update position.
            var currentPosition = m_VirtualMouse.position.ReadValue();
            var newPosition = currentPosition + delta;

            ////REVIEW: for the hardware cursor, clamp to something else?
            // Clamp to canvas.
            if (m_Canvas != null)
            {
                // Clamp to canvas.
                var pixelRect = m_Canvas.pixelRect;
                newPosition.x = Mathf.Clamp(newPosition.x, pixelRect.xMin, pixelRect.xMax);
                newPosition.y = Mathf.Clamp(newPosition.y, pixelRect.yMin, pixelRect.yMax);
            }

            ////REVIEW: the fact we have no events on these means that actions won't have an event ID to go by; problem?
            InputState.Change(m_VirtualMouse.position, newPosition);
            InputState.Change(m_VirtualMouse.delta, delta);

            // Update software cursor transform, if any.
            if (m_CursorTransform != null &&
                (m_CursorMode == CursorMode.SoftwareCursor ||
                    (m_CursorMode == CursorMode.HardwareCursorIfAvailable && m_SystemMouse == null)))
                m_CursorTransform.anchoredPosition = newPosition;

            m_LastStickValue = stickValue;
            m_LastTime = currentTime;

            // Update hardware cursor.
            m_SystemMouse?.WarpCursorPosition(newPosition);
        }
    }

    private static void SetAction(ref InputActionProperty field, InputActionProperty value)
    {
        var oldValue = field;
        field = value;

        if (oldValue.reference == null)
        {
            var oldAction = oldValue.action;
            if (oldAction != null && oldAction.enabled)
            {
                oldAction.Disable();
                if (value.reference == null)
                    value.action?.Enable();
            }
        }
    }

    private void OnAfterInputUpdate()
    {
        UpdateMotion();
    }

    /// <summary>
    /// Determines how the cursor for the virtual mouse is represented.
    /// </summary>
    /// <seealso cref="cursorMode"/>
    public enum CursorMode
    {
        /// <summary>
        /// The cursor is represented as a UI element. See <see cref="cursorGraphic"/>.
        /// This must be the mode in order for input to be driven by the native <see cref="Mouse"/> - otherwise, input
        /// from the hardware <see cref="Mouse"/> will be disabled.
        /// </summary>
        SoftwareCursor,

        /// <summary>
        /// If a native <see cref="Mouse"/> device is present, its cursor will be used and driven
        /// by the virtual mouse using <see cref="Mouse.WarpCursorPosition"/>. The software cursor
        /// referenced by <see cref="cursorGraphic"/> will be disabled, as well as input from
        /// hardware <see cref="Mouse"/> will be disabled.
        ///
        /// Note that if no native <see cref="Mouse"/> is present, behavior will fall back to
        /// <see cref="SoftwareCursor"/>.
        /// </summary>
        HardwareCursorIfAvailable,
    }

    /// <summary>
    /// Determines whether to move the cursor using the mouse or using a gamepad. 
    /// 
    /// <remarks>
    /// <c>cursorMode</c> must be set to <c>SoftwarCursor</c> in order move the cursor using a mouse  because we have 
    /// much better control over a software cursor than over a hardware cursor, due to the tick rate of the hardware 
    /// cursor genrally being much higher than that of our game. 
    /// </remarks>
    /// </summary>
    public enum CursorMovementMode
    {
        Mouse,
        Controller
    }
}
