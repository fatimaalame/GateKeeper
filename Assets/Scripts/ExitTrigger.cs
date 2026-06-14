using UnityEngine;

// script pour valider la fin du niveau
public class ExitTrigger : MonoBehaviour
{
    // petit délai pour éviter que la sortie se déclenche 20 fois d'un coup
    private float nextAllowedTriggerTime = 0f;

    // délai entre deux essais de validation de la sortie
    private float triggerCooldown = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        TryValidateExit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryValidateExit(other);
    }

    private void TryValidateExit(Collider other)
    {
        // on vérifie que c'est bien le joueur
        if (!other.CompareTag("Player")) return;

        // évite de relancer le test trop souvent
        if (Time.time < nextAllowedTriggerTime) return;

        // on met directement un petit cooldown, même si la porte est fermée
        nextAllowedTriggerTime = Time.time + triggerCooldown;

        // on récupère le type de porte logique du niveau actuel
        GateType gateType = GameManager.Instance.GetCurrentGateType();

        // on calcule si la porte est ouverte
        bool result = GameManager.Instance.EvaluateGate(
            gateType,
            GameManager.Instance.inputA,
            GameManager.Instance.inputB
        );

        // si la porte est ouverte, on passe au niveau suivant
        if (result)
        {
            // on bloque quelques secondes pour éviter un double chargement accidentel
            nextAllowedTriggerTime = Time.time + 2f;

            // son de réussite du niveau avant de charger le suivant
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLevelComplete();
            }

            Debug.Log("Niveau réussi ✅");
            GameManager.Instance.LoadNextLevel();
        }
        else
        {
            // son de porte bloquée si le joueur essaie de sortir trop tôt
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDoorLocked();
            }

            Debug.Log("La sortie est encore bloquée 🔒");
        }
    }
}