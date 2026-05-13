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
}