using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // vitesse de déplacement du joueur
    public float moveSpeed = 5f;

    // vitesse de rotation de la caméra POV
    public float cameraRotationSpeed = 120f;

    // petite gravité pour que le joueur reste bien au sol
    public float gravity = -9.81f;

    // caméra POV à faire glisser dans l'Inspector si besoin
    public Transform povCameraTransform;

    // composant Unity qui gère le déplacement du personnage
    private CharacterController controller;

    // vitesse verticale (surtout utile pour la gravité)
    private Vector3 velocity;

    void Start()
    {
        // on récupère le Character Controller attaché au joueur
        controller = GetComponent<CharacterController>();

        // si on n'a pas relié la caméra POV à la main,
        // on essaie de la trouver automatiquement dans les enfants du joueur
        if (povCameraTransform == null)
        {
            Transform found = transform.Find("POV Camera");
            if (found != null)
            {
                povCameraTransform = found;
            }
        }
    }

    void Update()
    {
        // déplacement du joueur comme avant, avec les flèches
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1f;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveZ = 1f;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            moveZ = -1f;
        }

        // mouvement "normal" dans le monde, sans tourner le perso
        Vector3 move = new Vector3(moveX, 0f, moveZ);

        // pour éviter d'aller plus vite en diagonale
        move = Vector3.ClampMagnitude(move, 1f);

        controller.Move(move * moveSpeed * Time.deltaTime);

        // rotation de la caméra POV avec A / D
        if (povCameraTransform != null)
        {
            float cameraTurn = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q))
            {
                cameraTurn = -1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                cameraTurn = 1f;
            }

            povCameraTransform.Rotate(0f, cameraTurn * cameraRotationSpeed * Time.deltaTime, 0f, Space.Self);
        }

        // si le joueur touche le sol et tombe encore, on remet une petite valeur
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // on applique la gravité
        velocity.y += gravity * Time.deltaTime;

        // on déplace le joueur verticalement
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerHitByEnemy();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerHitByEnemy();
            }
        }
    }
}