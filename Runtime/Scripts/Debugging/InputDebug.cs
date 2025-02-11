using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputDebug : MonoBehaviour
{
    [SerializeField] private InputActionReference debugActionKey1;
    [SerializeField] private UnityEvent debugAction1;
    [SerializeField] private InputActionReference debugActionKey2;
    [SerializeField] private UnityEvent debugAction2;
    [SerializeField] private InputActionReference debugActionKey3;
    [SerializeField] private UnityEvent debugAction3;
    [SerializeField] private InputActionReference debugActionKey4;
    [SerializeField] private UnityEvent debugAction4;

    private void OnEnable()
    {
        debugActionKey1.action.Enable();
        debugActionKey1.action.performed += _ => debugAction1.Invoke();
        debugActionKey2.action.Enable();
        debugActionKey2.action.performed += _ => debugAction2.Invoke();
        debugActionKey3.action.Enable();
        debugActionKey3.action.performed += _ => debugAction3.Invoke();
        debugActionKey4.action.Enable();
        debugActionKey4.action.performed += _ => debugAction4.Invoke();
    }
}
