using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    // caméra actuelle du jeu
    public Camera currentCamera;

    // caméra POV trouvée automatiquement dans le joueur
    private Camera povCamera;

    // true = caméra normale, false = caméra POV
    private bool isCurrentView = true;

    void Start()
    {
        // on force toujours la vue normale au lancement
        isCurrentView = true;

        // on cherche une première fois la caméra POV
        TryFindPovCamera();

        ApplyCameraState();
    }

    void Update()
    {
        // si la caméra POV n'a pas encore été trouvée, on réessaie
        if (povCamera == null)
        {
            TryFindPovCamera();
            ApplyCameraState();
        }

        // si la caméra POV a disparu pendant qu'elle était active (ex: joueur détruit),
        // on revient automatiquement à la caméra principale
        if (povCamera == null && !isCurrentView)
        {
            isCurrentView = true;
            ApplyCameraState();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
        }
    }

    // cette fonction cherche automatiquement la caméra POV dans le joueur
    void TryFindPovCamera()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            return;
        }

        // on cherche dans tous les enfants, même plus profondément
        Camera[] cameras = player.GetComponentsInChildren<Camera>(true);

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].gameObject.name == "POV Camera")
            {
                povCamera = cameras[i];
                return;
            }
        }
    }

    // cette fonction applique l'état actuel des deux caméras
    void ApplyCameraState()
    {
        // sécurité : si la caméra POV a été détruite avec le joueur,
        // Unity peut garder une ancienne référence cassée
        if (povCamera == null)
        {
            isCurrentView = true;
        }

        if (currentCamera != null)
        {
            currentCamera.gameObject.SetActive(isCurrentView);
            AudioListener currentListener = currentCamera.GetComponent<AudioListener>();
            if (currentListener != null)
            {
                currentListener.enabled = isCurrentView;
            }
        }

        if (povCamera != null)
        {
            povCamera.gameObject.SetActive(!isCurrentView);
            AudioListener povListener = povCamera.GetComponent<AudioListener>();
            if (povListener != null)
            {
                povListener.enabled = !isCurrentView;
            }
        }
    }

    public void ResetToMainCamera()
    {
        // si la caméra POV n'existe plus ou est en train d'être détruite,
        // on oublie proprement cette référence
        if (povCamera == null)
        {
            povCamera = null;
        }

        isCurrentView = true;
        ApplyCameraState();
    }

    public void SwitchCamera()
    {
        // sécurité : on réessaie au moment du switch si la caméra POV manque encore
        if (povCamera == null)
        {
            TryFindPovCamera();
        }

        if (currentCamera == null || povCamera == null)
        {
            return;
        }

        isCurrentView = !isCurrentView;
        ApplyCameraState();
    }
}