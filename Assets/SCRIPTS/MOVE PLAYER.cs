
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class RunnerController : MonoBehaviour
{
    [Header("Corrida")]
    public float forwardSpeed = 8f;

    [Header("Pistas")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 10f;

    [Header("Pulo")]
    public float jumpHeight = 2f;
    public float gravity = -20f;

    private CharacterController controller;
    private PlayerSons playerSons;
    private int currentLane = 1; // 0 = esquerda, 1 = centro, 2 = direita
    private float startX;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerSons = GetComponent<PlayerSons>();
        startX = transform.position.x;
    }

    void Update()
    {
        // Trocar de pista
        if (Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentLane = Mathf.Max(0, currentLane - 1);
            playerSons.TocarLateral();
        }

        if (Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentLane = Mathf.Min(2, currentLane + 1);
            playerSons.TocarLateral();
        }

        // Pular
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );
                playerSons.TocarPulo();
            }
        }

        // Gravidade
        verticalVelocity += gravity * Time.deltaTime;

        // Posição desejada na pista
        float targetX = startX + (currentLane - 1) * laneDistance;

        // Movimento lateral suave
        float differenceX = targetX - transform.position.x;
        float moveX = differenceX * laneChangeSpeed;

        // Correr para frente, trocar de pista e pular
        Vector3 movement = new Vector3(
            moveX,
            verticalVelocity,
            forwardSpeed
        );

        controller.Move(movement * Time.deltaTime);
    }
}
