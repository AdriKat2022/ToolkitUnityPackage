using UnityEngine;

public class RotationAnimationSineUI : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector2 _speed = new(1, 1);
    [SerializeField] private Vector2 _amplitude = new(1, 1);

    [Header("Hover properties")]
    [Tooltip("REQUIRES a RECTTRANSFROM instead of a regular transform.")]
    [SerializeField] private bool _hoverEffect = false;
    [SerializeField] private Vector2 _tiltStrength = new(1, 1);
    [Tooltip("If true, the hover effect only takes place if the mouse is over the target.\n" +
        "If false, the hover effect will always be active.")]
    [SerializeField] private bool _requireHover = false;

    private RectTransform _rectTransformTarget;
    private bool isHovered = false;

    private void Start()
    {
        if (_hoverEffect)
        {
            _rectTransformTarget = _target.GetComponent<RectTransform>();
            if (_rectTransformTarget == null)
            {
                Debug.LogError("Hover effect requires a RectTransform component.");
                _hoverEffect = false;
            }
        }
    }

    private void Update()
    {
        if (_target == null)
            return;

        float time = Time.time;
        float x = 0;
        float y = 0;

        if (_hoverEffect && (!_requireHover || (_requireHover && isHovered)))
        {
            Vector2 mouseDif = _rectTransformTarget.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            x = mouseDif.y * _tiltStrength.y;
            y = mouseDif.x * _tiltStrength.x;
        }
        else
        {
            x = Mathf.Sin(time * _speed.x) * _amplitude.x;
            y = Mathf.Cos(time * _speed.y) * _amplitude.y;
        }

        _target.localRotation = Quaternion.Euler(x, y, 0);
    }
}
