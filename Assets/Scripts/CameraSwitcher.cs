using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    // caméra principale du jeu
    public Camera currentCamera;

    // caméra POV trouvée automatiquement dans le joueur
    private Camera povCamera;

    // joueur actuellement suivi
    private GameObject currentPlayer;

    // true = caméra normale, false = caméra POV
    private bool isCurrentView = true;

    void Start()
    {
        // si la caméra principale n'est pas reliée dans l'Inspector, on prend Camera.main
        if (currentCamera == null)
        {
            currentCamera = Camera.main;
        }

        // on force toujours la vue normale au lancement
        isCurrentView = true;

        // on cherche le joueur et la caméra POV
        RefreshPlayerAndPovCamera();

        // on s'assure que la caméra principale suit le bon joueur
        UpdateMainCameraTarget();

        ApplyCameraState();
    }

    void Update()
    {
        // on vérifie régulièrement si le joueur a changé
        // utile quand GameManager détruit / recrée Player(Clone)
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != currentPlayer)
        {
            currentPlayer = foundPlayer;
            povCamera = null;
            RefreshPlayerAndPovCamera();
            UpdateMainCameraTarget();
            ResetToMainCamera();
        }

        // si la caméra POV manque, on réessaie de la retrouver
        if (povCamera == null)
        {
            RefreshPlayerAndPovCamera();
        }

        // si la caméra POV a disparu pendant qu'elle était active, on revient en caméra normale
        if (povCamera == null && !isCurrentView)
        {
            ResetToMainCamera();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
        }
    }

    // cette fonction retrouve le joueur et sa caméra POV
    private void RefreshPlayerAndPovCamera()
    {
        currentPlayer = GameObject.FindGameObjectWithTag("Player");

        if (currentPlayer == null)
        {
            povCamera = null;
            return;
        }

        // on cherche dans tous les enfants, même si la caméra est désactivée
        Camera[] cameras = currentPlayer.GetComponentsInChildren<Camera>(true);

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].gameObject.name == "POV Camera")
            {
                povCamera = cameras[i];
                return;
            }
        }

        povCamera = null;
    }

    // cette fonction remet la caméra principale sur le joueur actuel
    private void UpdateMainCameraTarget()
    {
        if (currentCamera == null)
        {
            currentCamera = Camera.main;
        }

        if (currentCamera == null || currentPlayer == null)
        {
            return;
        }

        CameraFollow follow = currentCamera.GetComponent<CameraFollow>();
        if (follow != null)
        {
            follow.target = currentPlayer.transform;
        }
    }

    // cette fonction applique l'état actuel des deux caméras
    private void ApplyCameraState()
    {
        // si la caméra POV n'existe pas, on force la caméra normale
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

    // cette fonction remet le jeu en caméra normale
    public void ResetToMainCamera()
    {
        isCurrentView = true;

        RefreshPlayerAndPovCamera();
        UpdateMainCameraTarget();

        // on coupe toujours la POV quand on revient en vue normale
        if (povCamera != null)
        {
            povCamera.gameObject.SetActive(false);
        }

        ApplyCameraState();
    }

    // cette fonction alterne entre caméra normale et caméra POV
    public void SwitchCamera()
    {
        RefreshPlayerAndPovCamera();
        UpdateMainCameraTarget();

        if (currentCamera == null || povCamera == null)
        {
            Debug.LogWarning("Switch caméra impossible : caméra principale ou POV manquante");
            return;
        }

        isCurrentView = !isCurrentView;
        ApplyCameraState();
    }
}