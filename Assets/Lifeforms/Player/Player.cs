/* Contains player data, movement logic, action logic, and the input system to initiate those actions.
 * Right now the player can only move, collide with physics objects, and shoot a stunning bullet at animals.
 * 
 * Dependent on no classes */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Lifeform
{
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

        playerInput.Player.Push.performed += PushPerformed;
        playerInput.Player.Shoot.performed += _ => shooting = true;
        playerInput.Player.Shoot.canceled += _ => shooting = false;
    }

    private void OnDisable()
    {
        playerInput.Disable();

        playerInput.Player.Push.performed -= PushPerformed;
        playerInput.Player.Shoot.performed -= _ => shooting = true;
        playerInput.Player.Shoot.canceled -= _ => shooting = false;
    }

    private void PushPerformed(InputAction.CallbackContext context)
    {
        print("Push");
    }

    private void ShootPerformed()
    {
        if (shootTimer <= 0 && !UIManager.Instance.PointerOverUI())
        {
            shootTimer = shootTimerMax;

            Vector2 mousePosition = playerInput.Player.Point.ReadValue<Vector2>();

            GameObject bulletObject = Instantiate(bulletPrefab, rigidBody.position,
                                      Movement.QuaternionAngle2D(rigidBody.position, mousePosition));
            bulletObject.GetComponent<Bullet>().SetUpBullet(10f);
        }
    }
}
