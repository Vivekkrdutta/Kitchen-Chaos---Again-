using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnBindingRebind;
    private const string INPUTSYSTEM_BINDING_OVERRIDES = "InputSystemBindingOverrides";
    public enum Bindings
    {
        Move_up,
        Move_Down,
        Move_left,
        Move_Right,
        Interact,
        Interact_Alt,
        Pause,
    }

    public event EventHandler OnPauseAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnInteractAction;
    private InputSystem inputSystem;
    public static GameInput Instance { get; private set; }

    private void Awake()
    {
        inputSystem = new InputSystem();
        if(PlayerPrefs.HasKey(INPUTSYSTEM_BINDING_OVERRIDES))
        {
            inputSystem.LoadBindingOverridesFromJson(PlayerPrefs.GetString(INPUTSYSTEM_BINDING_OVERRIDES));
        }

        inputSystem.Player.Enable();
        Instance = this;
    }
    private void Start()
    {

        inputSystem.Player.Interact.performed += Interact_performed;
        inputSystem.Player.InteractAlternate.performed += InteractAlternate_performed;
        inputSystem.Player.PauseAction.performed += PauseAction_performed;

    }

    private void OnDestroy()
    {
        inputSystem.Player.Interact.performed -= Interact_performed;
        inputSystem.Player.InteractAlternate.performed -= InteractAlternate_performed;
        inputSystem.Player.PauseAction.performed -= PauseAction_performed;

        inputSystem.Dispose();
    }
    private void PauseAction_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this,null);
    }

    private void InteractAlternate_performed(InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, null);
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetInputVector()
    {
        Vector2 inputVector = inputSystem.Player.Movement.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }

    public Vector2 GetInputVectorNormalized()
    {
        return GetInputVector().normalized;
    }

    public string GetBindingText(Bindings binding)
    {
        switch (binding)
        {
            case Bindings.Interact: return inputSystem.Player.Interact.bindings[0].ToDisplayString();
            case Bindings.Interact_Alt: return inputSystem.Player.InteractAlternate.bindings[0].ToDisplayString();
            case Bindings.Pause: return inputSystem.Player.PauseAction.bindings[0].ToDisplayString();
            case Bindings.Move_up: return inputSystem.Player.Movement.bindings[1].ToDisplayString();
            case Bindings.Move_Down: return inputSystem.Player.Movement.bindings[2].ToDisplayString();
            case Bindings.Move_left: return inputSystem.Player.Movement.bindings[3].ToDisplayString();
            case Bindings.Move_Right: return inputSystem.Player.Movement.bindings[4].ToDisplayString();
            default: return null;
        }
    }

    public void RebindBindings(Bindings binding,Action onActionRebound)
    {
        InputAction inputAction = null;
        int bindingIndex = 0;

        inputSystem.Player.Disable();
        switch (binding)
        {
            case Bindings.Interact:
                inputAction = inputSystem.Player.Interact;
                bindingIndex = 0;
                break;
            case Bindings.Interact_Alt:
                inputAction = inputSystem.Player.InteractAlternate;
                bindingIndex = 0;
                break;
            case Bindings.Pause:
                inputAction = inputSystem.Player.PauseAction;
                bindingIndex = 0;
                break;
            case Bindings.Move_up:
                inputAction = inputSystem.Player.Movement;
                bindingIndex = 1;
                break;
            case Bindings.Move_Down:
                inputAction = inputSystem.Player.Movement;
                bindingIndex = 2;
                break;
            case Bindings.Move_left:
                inputAction = inputSystem.Player.Movement;
                bindingIndex = 3;
                break;
            case Bindings.Move_Right:
                inputAction = inputSystem.Player.Movement;
                bindingIndex = 4;
                break;

        }

        inputAction?.PerformInteractiveRebinding(bindingIndex).OnComplete(callback =>
        {
            callback.Dispose();
            inputSystem.Player.Enable();
            onActionRebound?.Invoke();

            PlayerPrefs.SetString(INPUTSYSTEM_BINDING_OVERRIDES, inputSystem.SaveBindingOverridesAsJson());
            OnBindingRebind?.Invoke(this, EventArgs.Empty);

        }).Start();

    }
}
