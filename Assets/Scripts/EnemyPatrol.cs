using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    // point A du déplacement
    public Transform pointA;

    // point B du déplacement
    public Transform pointB;

    // vitesse de déplacement
    public float speed = 2f;

    // point actuellement visé
    private Transform target;

    // évite de lancer le game over plusieurs fois
    private bool hasHitPlayer = false;

    void Start()
    {
        // au départ, l'ennemi va vers B
        target = pointB;
    }

    void Update()
    {
        if (pointA == null || pointB == null || target == null)
            return;

        // on déplace l'ennemi vers sa cible
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // quand il arrive près du point cible, il repart vers l'autre
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            if (target == pointA)
            {
                target = pointB;
            }
            else
            {
                target = pointA;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
    // on vérifie que c'est bien le joueur
    if (!other.CompareTag("Player")) return;

    // son quand l'ennemi touche le joueur
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayGameOver();
    }

    // quand l'ennemi touche le joueur, on enlève une vie
    if (GameManager.Instance != null)
    {
        GameManager.Instance.OnPlayerHitByEnemy();
    }

    Debug.Log("L'ennemi a touché le joueur : -1 vie");
    }
}