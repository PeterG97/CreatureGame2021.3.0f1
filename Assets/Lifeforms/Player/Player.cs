using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    /* Dependent on no Classes */

    [SerializeField] private GameObject bulletPrefab;
    [NonSerialized] public Rigidbody2D rigidBody;
    [NonSerialized] public PlayerInputActions playerInput;

    public float moveSpeed = 5f;

    private bool shooting = false;
    private float shootTimerMax = 0.5f;
    private float shootTimer = 2f;
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        playerInput = new PlayerInputActions();

        //Subscribe to input event
        playerInput.Player.Push.performed += PushPerformed;
        playerInput.Player.Shoot.performed += _ => shooting = true;
        playerInput.Player.Shoot.canceled += _ => shooting = false;
    }

    private void FixedUpdate()
    {
        Vector2 moveDirection = playerInput.Player.Move.ReadValue<Vector2>();
        rigidBody.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);

        shootTimer -= Time.deltaTime;
        if (shooting)
            ShootPerformed();
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    private void PushPerformed(InputAction.CallbackContext context)
    {
        print("Push");
    }

    private void ShootPerformed()
    {
        if (shootTimer <= 0 && !UIManager.Instance.PointerOverUIObject())
        {
            shootTimer = shootTimerMax;

            Vector2 mousePosition = playerInput.Player.Point.ReadValue<Vector2>();

            GameObject bulletObject = Instantiate(bulletPrefab, rigidBody.position,
                                      Movement.QuaternionAngle2D(rigidBody.position, mousePosition));
            bulletObject.GetComponent<Bullet>().SetUpBullet(10f);
        }
    }
}
