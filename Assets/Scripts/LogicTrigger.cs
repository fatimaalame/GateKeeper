using UnityEngine;

// script pour les zones logiques
// ici il sert à activer A ou B quand le joueur entre dans la zone
public class LogicTrigger : MonoBehaviour
{
    // écrire A ou B dans l'Inspector
    public string triggerType;

    // petit verrou pour éviter de déclencher plusieurs fois d'affilée
    private bool playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        // on vérifie que c'est bien le joueur
        if (!other.CompareTag("Player")) return;

        // si le joueur est déjà dans la zone, on ne refait rien
        if (playerInside) return;

        // on note que le joueur est dedans
        playerInside = true;

        // si c'est le bouton A, on change l'état de A
        if (triggerType == "A")
        {
            GameManager.Instance.ToggleA();

            // son d'activation du bouton logique
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayActivate();
            }

            Debug.Log("Le joueur a activé le bouton A");
        }
        // si c'est le bouton B, on change l'état de B
        else if (triggerType == "B")
        {
            GameManager.Instance.ToggleB();

            // son d'activation du bouton logique
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayActivate();
            }

            Debug.Log("Le joueur a activé le bouton B");
        }
        else
        {
            Debug.Log("Trigger type inconnu. Mets A ou B.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // quand le joueur sort, le bouton pourra être réutilisé
        if (!other.CompareTag("Player")) return;

        playerInside = false;
    }
}