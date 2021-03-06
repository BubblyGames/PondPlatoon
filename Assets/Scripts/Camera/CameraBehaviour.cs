using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//https://www.youtube.com/watch?v=zVX9-c_aZVg
public class CameraBehaviour : MonoBehaviour
{
    public static CameraBehaviour instance;

    private float _rotationY = 45f;
    private float _rotationX = 45f;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    internal float _distanceFromTarget = 250f;
    [SerializeField]
    private float maxDistance = 300f;
    [SerializeField]
    private float minDistance = 100f;

    private Vector3 _currentRotation;
    private Vector3 _smoothVelocity = Vector3.zero;

    [SerializeField]
    private float _smoothTime = 0.2f;

    [SerializeField]
    private Vector2 _rotationXMinMax = new Vector2(-40, 40);

    [SerializeField]
    private bool freeMovement = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _target = WorldManager.instance.GetCenter();
    }

    void Update()
    {
        if (!LevelManager.instance.ready)
            return;
            

        // Apply clamping for x rotation 
        _rotationX = Mathf.Clamp(_rotationX, _rotationXMinMax.x, _rotationXMinMax.y);

        Vector3 nextRotation = new Vector3(_rotationX, _rotationY);

        // Apply damping between rotation changes
        _currentRotation = Vector3.SmoothDamp(_currentRotation, nextRotation, ref _smoothVelocity, _smoothTime);
        transform.localEulerAngles = _currentRotation;

        // Substract forward vector of the GameObject to point its forward vector to the target
        transform.position = _target.position - transform.forward * _distanceFromTarget;
    }

    public void Zoom(float amount)
    {
        _distanceFromTarget = Mathf.Clamp(_distanceFromTarget - amount, minDistance, maxDistance);
    }

    public void Rotate(float mouseX, float mouseY)
    {
        if (freeMovement && Input.GetButton("Fire1"))
        {
            _rotationY += mouseX;
            _rotationX -= mouseY;
        }
    }

    public void RotateRight()
    {
        _rotationY -= 90;
    }

    public void RotateLeft()
    {
        _rotationY += 90;
    }

    public void RotateUp()
    {
        _rotationX += 180;
    }

    public void RotateDown()
    {
        _rotationX -= 180;
    }
}
