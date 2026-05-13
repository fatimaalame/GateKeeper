using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    // singleton simple pour accéder au GameManager depuis les autres scripts
    public static GameManager Instance;

    [Header("Prefabs")]
    // prefab du sol
    public GameObject floorPrefab;

    // prefab du mur
    public GameObject wallPrefab;

    // prefab du joueur
    public GameObject playerPrefab;

    // prefab du bouton A
    public GameObject buttonAPrefab;

    // prefab du bouton B
    public GameObject buttonBPrefab;

    // prefab de la porte
    public GameObject doorPrefab;

    // prefab de l'ennemi
    public GameObject enemyPrefab;

    [Header("Scene References")]
    // parent qui contient les objets du niveau
    public Transform levelParent;

    // trigger de sortie déjà placé dans la scène
    public Transform exitTriggerTransform;

    [Header("Settings")]
    // taille d'une case du labyrinthe
    public float cellSize = 2f;

    // vitesse de déplacement actuelle de l'ennemi
    public float enemyMoveSpeed = 2f;

    // vitesse de base de l'ennemi au niveau 2
    public float baseEnemyMoveSpeed = 2f;

    // augmentation de vitesse à chaque niveau supérieur
    public float enemySpeedStep = 0.5f;

    // distance minimale entre l'ennemi et le départ / la sortie
    public float enemySafeDistanceMultiplier = 3f;

    // longueur de la zone protégée autour du couloir critique (départ / sortie)
    public float enemyCriticalCorridorLength = 6f;

    // distance minimale entre deux ennemis au spawn
    public float enemySpacingMultiplier = 2.5f;

    [Header("UI")]
    // script qui gère l'affichage logique à l'écran
    public LogicUI logicUI;

    // script qui gère les vies et le niveau en haut à gauche
    public StatusUI statusUI;

    // script qui gère la mini-map
    public MiniMapUI miniMapUI;

    // script qui gère le timer affiché
    public Timer timerUI;

    // texte d'instruction affiché en haut au centre
    public TMP_Text levelInstructionText;

    // panel du haut qui contient les instructions
    public GameObject instructionsPanel;

    public GameObject tutorialMessagePanel;
    public TextMeshProUGUI tutorialMessageText;

    public GameObject tutorialCompletePanel;
    public int tutorialLevelCount = 4;

    // panel de game over du vrai jeu
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;

    // panel de victoire du vrai jeu
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryText;


    [Header("Logic State")]
    
    // état du bouton A
    public bool inputA = false;

    // nombre de vies du joueur
    public int lives = 5;

    // score du joueur
    public int score = 0;
    
    // état du bouton B
    public bool inputB = false;

    // référence vers le joueur créé dans la scène
    private GameObject currentPlayer;

    // position de départ du joueur dans le niveau courant
    private Vector3 currentPlayerStartPosition;

    // référence vers la porte créée dans la scène
    private GameObject currentDoor;

    // liste des ennemis courants dans la scène
    private List<GameObject> currentEnemies = new List<GameObject>();

    // liste des cibles actuelles des ennemis
    private List<Vector3> currentEnemyTargetPoints = new List<Vector3>();

    // petit cooldown pour éviter de perdre plusieurs vies d'un coup
    private float lastEnemyHitTime = -10f;
    private float enemyHitCooldown = 1f;

    // maze actuellement utilisé
    private int[,] currentMaze;    

    // liste des niveaux
    public List<LogicLevelRuntime> levels = new List<LogicLevelRuntime>();

    // index du niveau actuel
    private int currentLevelIndex = 0;

    // dit si on a dépassé le dernier niveau
    private bool gameFinished = false;

    // évite de terminer un niveau deux fois à cause d'un double trigger
    private bool isLoadingNextLevel = false;

    // nombre de réussites consécutives au niveau 5
    private int level5WinStreak = 0;

    [Header("Adaptive Difficulty")]
    // indique si on est dans le vrai mode adaptatif
    public bool isAdaptiveMode = false;

    // niveau adaptatif du vrai jeu (commence à 2 après le tuto)
    public int adaptiveLevel = 2;

    // niveau de difficulté actuel affiché dans le HUD
    public int difficultyLevel = 1;

    // nombre de réussites rapides d'affilée
    private int consecutiveFastSuccesses = 0;

    // temps de départ du niveau actuel
    private float levelStartTime = 0f;

    // temps mis pour finir le dernier niveau
    public float lastCompletionTime = 0f;

    // nombre de réussites dans le niveau adaptatif courant
    private int winsInCurrentAdaptiveLevel = 0;

    // nombre de morts dans le niveau adaptatif courant
    private int deathsInCurrentAdaptiveLevel = 0;

    private void Awake()
    {
        // singleton basique
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // on garde une vitesse de base propre pour les ennemis
        baseEnemyMoveSpeed = enemyMoveSpeed;
    }

    private void Start()
    {
        // on crée les niveaux
        CreateLevels();

        // si on est dans la scène du vrai jeu, on lance directement le mode adaptatif
        if (SceneManager.GetActiveScene().name == "AdaptiveMazeScene")
        {
            StartAdaptiveGame();
            return;
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }

        if (tutorialMessagePanel != null)
        {
            tutorialMessagePanel.SetActive(true);
        }

        if (tutorialCompletePanel != null)
        {
            tutorialCompletePanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        Time.timeScale = 1f;

        // on charge le premier niveau du tuto
        LoadLevel(0);
    }
    private void Update()
    {
        UpdateMiniMapPlayer();
        UpdateEnemyMovement();

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R détecté");
            SceneManager.LoadScene("MainMenu");
        }
    }

    void CreateLevels()
    {
        // niveau 1 : porte OR
        levels.Add(new LogicLevelRuntime(
            GateType.OR,
            "Porte OR : Active A OU B pour ouvrir la porte !",
            new int[,]
            {
                {1,1,1,1,1,1,1,1,1},
                {1,5,0,0,1,0,0,0,1},
                {1,0,1,0,1,0,1,0,1},
                {1,0,1,0,0,0,1,0,1},
                {1,0,1,1,1,1,1,0,1},
                {1,0,0,0,2,0,0,0,1},
                {1,1,1,0,1,0,1,1,1},
                {1,0,0,0,0,0,0,3,1},
                {1,0,1,1,4,1,1,0,1},
                {1,1,1,1,1,1,1,1,1}
            }
        ));

        // niveau 2 : porte AND
        levels.Add(new LogicLevelRuntime(
            GateType.AND,
            "Porte AND : Active A ET B pour ouvrir la porte !",
            new int[,]
            {
                {1,1,1,1,1,1,1,1,1},
                {1,5,0,1,0,0,0,0,1},
                {1,0,0,1,0,1,1,0,1},
                {1,0,1,1,0,0,1,0,1},
                {1,0,0,0,0,1,1,0,1},
                {1,1,1,0,1,1,0,0,1},
                {1,2,0,0,0,0,0,1,1},
                {1,1,1,0,0,1,0,3,1},
                {1,0,0,0,4,0,0,1,1},
                {1,1,1,1,1,1,1,1,1}
            }
        ));

        // niveau 3 : porte NOT
        levels.Add(new LogicLevelRuntime(
            GateType.NOT,
            "Porte NOT : La sortie s'ouvre quand A est désactivé !",
            new int[,]
            {
                {1,1,1,1,1,1,1,1,1},
                {1,5,0,0,0,1,0,0,1},
                {1,1,1,1,0,1,0,1,1},
                {1,0,0,0,0,0,0,0,1},
                {1,0,1,1,1,1,1,0,1},
                {1,0,0,2,1,0,0,0,1},
                {1,1,1,0,1,0,1,1,1},
                {1,0,0,0,0,0,0,0,1},
                {1,0,1,1,4,1,1,0,1},
                {1,1,1,1,1,1,1,1,1}
            }
        ));

        // niveau 4 : porte XOR
        levels.Add(new LogicLevelRuntime(
            GateType.XOR,
            "Porte XOR : La porte s'ouvre quand UNE SEULE entrée est activée !",
            new int[,]
            {
                {1,1,1,1,1,1,1,1,1},
                {1,5,0,0,1,0,0,0,1},
                {1,0,1,0,1,0,1,0,1},
                {1,0,1,0,0,0,1,0,1},
                {1,0,1,1,1,1,1,0,1},
                {1,0,0,0,2,0,0,0,1},
                {1,1,1,0,1,0,1,1,1},
                {1,0,0,0,0,0,0,3,1},
                {1,0,1,1,4,1,1,0,1},
                {1,1,1,1,1,1,1,1,1}
            }
        ));
    }

    public void LoadLevel(int index)
    {
        // on garde en mémoire quel niveau est chargé
        currentLevelIndex = index;

        // si un panel de game over était affiché, on le cache
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // comme on charge un vrai niveau, le jeu n'est pas fini
        gameFinished = false;
        isLoadingNextLevel = false;

        // on démarre le chrono du niveau
        levelStartTime = Time.time;

        // on remet aussi le timer visuel à zéro
        if (timerUI == null)
        {
            timerUI = FindFirstObjectByType<Timer>();
        }

        if (timerUI != null)
        {
            timerUI.ResetTimer();
        }

        // on supprime les anciens objets du niveau
        foreach (Transform child in levelParent)
        {
            Destroy(child.gameObject);
        }

        // on remet les états logiques à zéro
        inputA = false;
        inputB = false;

        // pendant un niveau normal, le bouton A n'est pas activé
        // exception pour NOT : on le met activé au départ pour forcer le joueur à le désactiver
        if (levels[index].gateType == GateType.NOT)
        {
            inputA = true;
        }

        // on vide l'ancienne référence de porte
        currentDoor = null;

        // on détruit aussi les anciens ennemis s'ils existent
        for (int i = 0; i < currentEnemies.Count; i++)
        {
            if (currentEnemies[i] != null)
            {
                Destroy(currentEnemies[i]);
            }
        }

        currentEnemies.Clear();
        currentEnemyTargetPoints.Clear();

        // on récupère le niveau pour garder sa logique (OR / AND / NOT)
        LogicLevelRuntime level = levels[index];

        // on génère un nouveau maze selon la difficulté actuelle
        int[,] generatedMaze = MazeGenerator.GenerateMaze(
            GetMazeSizeForCurrentDifficulty(),
            level.gateType
        );

        // on garde ce maze en mémoire pour la mini-map
        currentMaze = generatedMaze;

        Debug.Log("Maze généré : " + generatedMaze.GetLength(0) + " x " + generatedMaze.GetLength(1));
        
        // debug pour voir la taille cible du maze selon la difficulté actuelle
        Debug.Log("Difficulté actuelle : " + difficultyLevel);
        Debug.Log("Taille cible du maze : " + GetMazeSizeForCurrentDifficulty());

        int rows = generatedMaze.GetLength(0);
        int cols = generatedMaze.GetLength(1);

        // on parcourt toutes les cases du tableau
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int cell = generatedMaze[r, c];
                Vector3 pos = new Vector3(c * cellSize, 0, r * cellSize);

                // on met du sol partout
                Instantiate(floorPrefab, pos, Quaternion.identity, levelParent);

                // si la case vaut 1, on place un mur
                if (cell == 1)
                {
                    Instantiate(wallPrefab, pos + new Vector3(0, 0.9f, 0), Quaternion.identity, levelParent);
                }

                // si la case vaut 2, on place le bouton A
                if (cell == 2)
                {
                    Instantiate(buttonAPrefab, pos, Quaternion.identity, levelParent);
                }

                // si la case vaut 3, on place le bouton B
                if (cell == 3)
                {
                    Instantiate(buttonBPrefab, pos, Quaternion.identity, levelParent);
                }

                // si la case vaut 4, on place la porte
                if (cell == 4)
                {
                    Vector3 doorWorldPos = pos + new Vector3(0, 1f, 0);

                    currentDoor = Instantiate(
                        doorPrefab,
                        doorWorldPos,
                        Quaternion.identity,
                        levelParent
                    );

                    // on déplace aussi le trigger de sortie sur la porte générée
                    if (exitTriggerTransform != null)
                    {
                        exitTriggerTransform.position = doorWorldPos;
                    }
                }

                // si la case vaut 5, on place le joueur au départ
                if (cell == 5)
                {
                    if (currentPlayer != null)
                        Destroy(currentPlayer);

                    currentPlayer = Instantiate(
                        playerPrefab,
                        new Vector3(c * cellSize, 1f, r * cellSize),
                        Quaternion.identity
                    );

                    // on garde en mémoire la position de départ du joueur
                    currentPlayerStartPosition = currentPlayer.transform.position;

                    // la caméra suit le joueur
                    CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
                    if (cam != null)
                    {
                        cam.target = currentPlayer.transform;
                    }
                }
            }
        }

        // une fois la porte créée, on met son état visuel à jour
        UpdateDoorState();

        // on met aussi l'interface logique à jour
        UpdateLogicUI();

        // on met à jour les vies et le niveau affichés
        UpdateStatusUI();
        
        // on met à jour la phrase d'instruction du niveau
        UpdateLevelInstructionUI();

        // on remet le panel d'instructions visible pendant les niveaux
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }

        // on remet aussi le panel de tuto visible pendant les niveaux tuto
        if (tutorialMessagePanel != null)
        {
            tutorialMessagePanel.SetActive(true);
        }

        // le panel de fin de tuto doit rester caché pendant un niveau
        if (tutorialCompletePanel != null)
        {
            tutorialCompletePanel.SetActive(false);
        }

        // on met à jour le message du tuto selon le niveau actuel
        UpdateTutorialMessage();

        // on construit la mini-map avec le maze généré
        if (miniMapUI != null)
        {
            miniMapUI.BuildMiniMap(generatedMaze);
        }

        // on fait apparaître un ennemi sur une case libre du niveau
        SpawnEnemy();
    }

    // cette fonction inverse l'état de A
    public void ToggleA()
    {
        inputA = !inputA;
        Debug.Log("Etat de A : " + inputA);

        // à chaque changement, on met à jour la porte
        UpdateDoorState();

        // on met aussi l'interface à jour
        UpdateLogicUI();
    }

    // cette fonction inverse l'état de B
    public void ToggleB()
    {
        inputB = !inputB;
        Debug.Log("Etat de B : " + inputB);

        // à chaque changement, on met à jour la porte
        UpdateDoorState();

        // on met aussi l'interface à jour
        UpdateLogicUI();
    }

    // cette fonction calcule le résultat de la porte logique du niveau
    public bool EvaluateGate(GateType gate, bool a, bool b)
    {
        // si la porte est AND, il faut A et B
        if (gate == GateType.AND)
        {
            return a && b;
        }

        // si la porte est OR, il faut A ou B
        if (gate == GateType.OR)
        {
            return a || b;
        }

        // si la porte est NOT, on inverse A
        if (gate == GateType.NOT)
        {
            return !a;
        }

        // si la porte est XOR, une seule entrée doit être active
        if (gate == GateType.XOR)
        {
            return a != b;
        }

        // sécurité au cas où
        return false;
    }

    // renvoie le type de porte logique du niveau actuel
    public GateType GetCurrentGateType()
    {
        // sécurité si l'index sort de la liste
        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Count)
        {
            return GateType.OR;
        }

        return levels[currentLevelIndex].gateType;
    }

    // cette fonction met à jour l'interface logique
    public void UpdateLogicUI()
    {
        // si on a dépassé le dernier niveau, on arrête ici
        if (gameFinished) return;

        // sécurité si aucune UI n'est reliée
        if (logicUI == null) return;

        // on récupère le type de porte logique du niveau actuel
        GateType gateType = GetCurrentGateType();

        // on calcule la sortie logique
        bool output = EvaluateGate(gateType, inputA, inputB);

        // on met à jour le panneau
        logicUI.RefreshUI(gateType, inputA, inputB, output);
    }
    
    // cette fonction met à jour l'affichage des vies, du score, du niveau et de la difficulté
    public void UpdateStatusUI()
    {
        // sécurité si aucune UI n'est reliée
        if (statusUI == null) return;

        // pendant le vrai jeu, on affiche le niveau adaptatif
        if (isAdaptiveMode)
        {
            statusUI.RefreshStatus(lives, score, adaptiveLevel, difficultyLevel);
            return;
        }

        // pendant le tuto, on affiche le niveau normal de la liste
        statusUI.RefreshStatus(lives, score, currentLevelIndex + 1, difficultyLevel);
    }

    // cette fonction met à jour la phrase d'instruction du niveau
    public void UpdateLevelInstructionUI()
    {
        if (levelInstructionText == null) return;

        switch (currentLevelIndex)
        {
            case 0:
                levelInstructionText.text = "Porte OR : Active A OU B pour ouvrir la porte !";
                break;

            case 1:
                levelInstructionText.text = "Niveau réussi ! Porte AND : Active A ET B pour ouvrir la porte !";
                break;

            case 2:
                levelInstructionText.text = "Niveau réussi ! Porte NOT : La sortie s'ouvre quand A est DÉSACTIVÉ !";
                break;

            case 3:
                levelInstructionText.text = "Niveau réussi ! Porte XOR : La porte s'ouvre quand UNE SEULE entrée est activée !";
                break;

            default:
                levelInstructionText.text = "";
                break;
        }
    }

    // cette fonction met à jour le message du tuto selon le niveau actuel
    public void UpdateTutorialMessage()
    {
        if (tutorialMessageText == null) return;

        switch (currentLevelIndex)
        {
            case 0:
                tutorialMessageText.text = "Porte OR : Active A OU B pour ouvrir la porte !";
                break;

            case 1:
                tutorialMessageText.text = "Porte AND : Active A ET B pour ouvrir la porte !";
                break;

            case 2:
                tutorialMessageText.text = "Porte NOT : La sortie s'ouvre quand A est DÉSACTIVÉ !";
                break;

            case 3:
                tutorialMessageText.text = "Porte XOR : La porte s'ouvre quand UNE SEULE entrée est activée !";
                break;

            default:
                tutorialMessageText.text = "";
                break;
        }
    }

    // cette fonction affiche le panel de fin de tuto
    public void ShowTutorialCompletePanel()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        if (tutorialMessagePanel != null)
        {
            tutorialMessagePanel.SetActive(false);
        }

        if (tutorialCompletePanel != null)
        {
            tutorialCompletePanel.SetActive(true);
        }

        isLoadingNextLevel = false;
    }

    // cette fonction met à jour le marqueur joueur sur la mini-map
    public void UpdateMiniMapPlayer()
    {
        if (miniMapUI == null) return;
        if (currentPlayer == null) return;
        if (currentMaze == null) return;

        miniMapUI.UpdatePlayerMarker(currentPlayer.transform.position, currentMaze.GetLength(0));
    }
    
    // cette fonction met à jour l'état visuel de la porte
    public void UpdateDoorState()
    {
        // si on a dépassé le dernier niveau, on arrête ici
        if (gameFinished) return;

        // sécurité : si aucune porte n'est créée, on arrête
        if (currentDoor == null) return;

        // on récupère le type de porte logique du niveau actuel
        GateType gateType = GetCurrentGateType();

        // on calcule si la porte doit être ouverte ou fermée
        bool result = EvaluateGate(gateType, inputA, inputB);

        // on récupère le renderer de la porte
        Renderer doorRenderer = currentDoor.GetComponent<Renderer>();

        // on récupère aussi le collider de la porte
        Collider doorCollider = currentDoor.GetComponent<Collider>();

        // sécurité si jamais il manque un composant
        if (doorRenderer == null) return;

        // si la logique est vraie, la porte est ouverte
        if (result)
        {
            // vert translucide
            doorRenderer.material.color = new Color(0f, 166f / 255f, 0f, 0.6f);

            // la porte ouverte ne bloque plus le joueur
            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }

            Debug.Log("Porte ouverte ✅");
        }
        else
        {
            // rouge translucide
            doorRenderer.material.color = new Color(0.55f, 0.05f, 0.05f, 0.6f);

            // la porte fermée rebloque le passage
            if (doorCollider != null)
            {
                doorCollider.enabled = true;
            }

            Debug.Log("Porte fermée 🔒");
        }
    }
    
    // cette fonction donne le temps cible selon la difficulté actuelle
    public float GetTargetTimeForCurrentDifficulty()
    {
        // pendant le vrai jeu, on suit les niveaux adaptatifs 2 à 5
        if (isAdaptiveMode)
        {
            if (difficultyLevel == 2)
            {
                return 90f;
            }

            if (difficultyLevel == 3)
            {
                return 60f;
            }

            if (difficultyLevel == 4)
            {
                return 45f;
            }

            // niveau 5 ou plus
            return 30f;
        }

        // pendant le tuto, on garde des temps larges pour laisser apprendre
        return 90f;
    }
    
        // cette fonction donne la taille du maze selon la difficulté
    public int GetMazeSizeForCurrentDifficulty()
    {
        // pendant le vrai jeu, on suit les niveaux adaptatifs 2 à 5
        if (isAdaptiveMode)
        {
            if (difficultyLevel == 2)
            {
                return 9;
            }

            if (difficultyLevel == 3)
            {
                return 11;
            }

            if (difficultyLevel == 4)
            {
                return 13;
            }

            // niveau 5 ou plus
            return 15;
        }

        // pendant le tuto, on garde un maze simple et constant
        return 9;
    }


        // cette fonction ajuste la difficulté selon le temps du joueur
    public void AdjustDifficulty()
    {
        // temps mis pour finir le niveau
        lastCompletionTime = Time.time - levelStartTime;

        // temps cible pour la difficulté actuelle
        float targetTime = GetTargetTimeForCurrentDifficulty();

        // si le joueur a été rapide
        if (lastCompletionTime <= targetTime)
        {
            consecutiveFastSuccesses += 1;

            Debug.Log("Niveau fini rapidement");
            Debug.Log("Temps : " + lastCompletionTime + " / cible : " + targetTime);
        }
        else
        {
            // si le joueur a été lent, on casse la série de réussites rapides
            consecutiveFastSuccesses = 0;

            Debug.Log("Niveau réussi, mais trop lent");
            Debug.Log("Temps : " + lastCompletionTime + " / cible : " + targetTime);
        }

        // si le joueur réussit 2 niveaux rapides d'affilée, on monte
        if (consecutiveFastSuccesses >= 2)
        {
            difficultyLevel = Mathf.Min(difficultyLevel + 1, 4);
            consecutiveFastSuccesses = 0;

            Debug.Log("Difficulté augmentée ⬆");
            Debug.Log("Nouvelle difficulté : " + difficultyLevel);
        }

        // si le joueur est vraiment trop lent, on baisse
        if (lastCompletionTime > targetTime * 1.5f)
        {
            difficultyLevel = Mathf.Max(difficultyLevel - 1, 1);
            consecutiveFastSuccesses = 0;

            Debug.Log("Difficulté baissée ⬇");
            Debug.Log("Nouvelle difficulté : " + difficultyLevel);
        }
    }

    // cette fonction charge le niveau suivant
    public void LoadNextLevel()
    {
        // sécurité pour éviter un double passage au niveau suivant
        if (isLoadingNextLevel)
        {
            return;
        }

        isLoadingNextLevel = true;

        // si on est dans le vrai jeu, on ne suit plus la logique du tuto
        if (isAdaptiveMode)
        {
            CompleteAdaptiveLevel();
            return;
        }

        // avant de changer de niveau, on ajuste la difficulté
        AdjustDifficulty();

        // le joueur gagne des points quand il termine un niveau
        score += 20;

        // on met à jour le HUD tout de suite
        UpdateStatusUI();

        // on passe au niveau suivant
        currentLevelIndex++;

        // si on a fini les niveaux du tuto, on affiche le panel final
        if (currentLevelIndex >= tutorialLevelCount)
        {
            gameFinished = true;
            ShowTutorialCompletePanel();
            Debug.Log("Tutoriel terminé !");
            return;
        }

        // sécurité si jamais l'index dépasse la liste
        if (currentLevelIndex >= levels.Count)
        {
            gameFinished = true;
            Debug.Log("Tous les niveaux sont terminés hihi");
            isLoadingNextLevel = false;
            return;
        }

        // sinon on charge le niveau suivant
        LoadLevel(currentLevelIndex);
        isLoadingNextLevel = false;
    }
    // cette fonction démarre le vrai jeu après le tuto
    public void StartAdaptiveGame()
    {
        // on active le vrai mode adaptatif
        isAdaptiveMode = true;

        // le vrai jeu commence au niveau 2
        adaptiveLevel = 2;
        difficultyLevel = adaptiveLevel;

        // on remet les ressources du joueur à zéro
        score = 0;
        lives = 5;
        level5WinStreak = 0;
        winsInCurrentAdaptiveLevel = 0;
        deathsInCurrentAdaptiveLevel = 0;

        // on repart du premier layout de base pour le vrai jeu
        currentLevelIndex = 0;
        gameFinished = false;

        // on cache les éléments du tuto
        if (tutorialCompletePanel != null)
        {
            tutorialCompletePanel.SetActive(false);
        }

        if (tutorialMessagePanel != null)
        {
            tutorialMessagePanel.SetActive(false);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        // on cache aussi le panel de game over si besoin
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        Time.timeScale = 1f;

        // on remet le HUD à jour
        UpdateStatusUI();

        // on lance le premier niveau du vrai jeu
        GenerateAdaptiveLevel();
    }

    // cette fonction génère un niveau du vrai jeu en fonction de la difficulté
    public void GenerateAdaptiveLevel()
    {
        // on cale la difficulté affichée sur le niveau adaptatif courant
        difficultyLevel = adaptiveLevel;

        // on met aussi à jour la vitesse des ennemis selon le niveau courant
        UpdateEnemySpeedForAdaptiveLevel();

        // pour l'instant on réutilise le système existant de chargement
        // plus tard on pourra spécialiser encore plus les labyrinthes selon adaptiveLevel
        currentLevelIndex = Mathf.Clamp(adaptiveLevel - 2, 0, levels.Count - 1);

        Debug.Log("Vrai jeu lancé - niveau adaptatif : " + adaptiveLevel);

        LoadLevel(currentLevelIndex);

        // une fois le niveau chargé, on recache les panneaux de tuto
        if (tutorialMessagePanel != null)
        {
            tutorialMessagePanel.SetActive(false);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        if (tutorialCompletePanel != null)
        {
            tutorialCompletePanel.SetActive(false);
        }

        isLoadingNextLevel = false;
        UpdateStatusUI();
    }

    // cette fonction ajoute les points selon le niveau adaptatif atteint
    public void AddScoreForCompletedAdaptiveLevel()
    {
        if (adaptiveLevel == 2)
        {
            score += 10;
        }
        else if (adaptiveLevel == 3)
        {
            score += 15;
        }
        else if (adaptiveLevel == 4)
        {
            score += 20;
        }
        else if (adaptiveLevel >= 5)
        {
            score += 30;
        }

        Debug.Log("Score adaptatif ajouté = " + score + " | niveau = " + adaptiveLevel);
        UpdateStatusUI();
    }

    // cette fonction ajuste le niveau adaptatif selon la réussite du joueur
    // ici la progression se fait avec 2 réussites pour monter et 2 morts pour descendre
    public void AdjustAdaptiveLevel(bool success, bool withinTime, bool lostLife)
    {
        // cette fonction est gardée pour compatibilité, mais la vraie logique
        // de montée / descente est maintenant gérée dans CompleteAdaptiveLevel et FailAdaptiveLevel
        adaptiveLevel = Mathf.Clamp(adaptiveLevel, 2, 5);
        difficultyLevel = adaptiveLevel;
        UpdateEnemySpeedForAdaptiveLevel();
    }

    // cette fonction termine un niveau du vrai jeu et prépare le suivant
    public void CompleteAdaptiveLevel()
    {
        // on mesure le temps mis pour finir
        lastCompletionTime = Time.time - levelStartTime;

        // on ajoute les points avant de passer au niveau suivant
        AddScoreForCompletedAdaptiveLevel();
        UpdateStatusUI();

        // une réussite remet le compteur de morts du niveau courant à zéro
        deathsInCurrentAdaptiveLevel = 0;

        // si le joueur réussit un niveau 5, on augmente la série de victoires extrêmes
        if (adaptiveLevel == 5)
        {
            level5WinStreak++;
            Debug.Log("Victoire niveau 5 : " + level5WinStreak + "/3");

            if (level5WinStreak >= 3)
            {
                ShowVictoryPanel();
                return;
            }

            // au niveau 5, on reste au même niveau tant que la victoire finale n'est pas atteinte
            GenerateAdaptiveLevel();
            return;
        }

        // pour les niveaux 2 à 4, il faut réussir 2 fois avant de monter
        winsInCurrentAdaptiveLevel++;
        Debug.Log("Réussites dans le niveau " + adaptiveLevel + " : " + winsInCurrentAdaptiveLevel + "/2");

        if (winsInCurrentAdaptiveLevel >= 2)
        {
            adaptiveLevel = Mathf.Clamp(adaptiveLevel + 1, 2, 5);
            difficultyLevel = adaptiveLevel;
            winsInCurrentAdaptiveLevel = 0;
            deathsInCurrentAdaptiveLevel = 0;
            UpdateEnemySpeedForAdaptiveLevel();
            Debug.Log("Montée au niveau adaptatif : " + adaptiveLevel);
        }

        GenerateAdaptiveLevel();
    }

   // cette fonction sert pour le game over du vrai jeu
    public void FailAdaptiveLevel()
    {
        level5WinStreak = 0;
        winsInCurrentAdaptiveLevel = 0;
        deathsInCurrentAdaptiveLevel = 0;

        if (lives <= 0)
        {
            lives = 0;
            UpdateStatusUI();
            ShowGameOverPanel();
            return;
        }

        GenerateAdaptiveLevel();
    }
    
    // cette fonction ajuste la vitesse des ennemis selon le niveau adaptatif courant
    public void UpdateEnemySpeedForAdaptiveLevel()
    {
        enemyMoveSpeed = baseEnemyMoveSpeed + ((adaptiveLevel - 2) * enemySpeedStep);
    }


    // cette fonction est appelée quand un ennemi touche le joueur
public void OnPlayerHitByEnemy()
    {
        // petit cooldown pour éviter de perdre plusieurs vies instantanément
        if (Time.time - lastEnemyHitTime < enemyHitCooldown)
        {
            return;
        }

        lastEnemyHitTime = Time.time;

        lives -= 1;

        if (lives < 0)
        {
            lives = 0;
        }

        // en mode adaptatif, chaque vie perdue compte comme une mort dans le niveau courant
        if (isAdaptiveMode)
        {
            level5WinStreak = 0;
            winsInCurrentAdaptiveLevel = 0;
            deathsInCurrentAdaptiveLevel++;
            Debug.Log("Morts dans le niveau " + adaptiveLevel + " : " + deathsInCurrentAdaptiveLevel + "/2");
        }

        // on met ensuite le HUD à jour
        UpdateStatusUI();

        // si le joueur n'a plus de vie, on utilise la logique de game over
        if (lives <= 0)
        {
            if (isAdaptiveMode)
            {
                FailAdaptiveLevel();
            }
            else
            {
                ShowGameOverPanel();
            }
            return;
        }

        // si le joueur meurt 2 fois dans le même niveau, il redescend au niveau précédent
        if (isAdaptiveMode && deathsInCurrentAdaptiveLevel >= 2)
        {
            adaptiveLevel = Mathf.Clamp(adaptiveLevel - 1, 2, 5);
            difficultyLevel = adaptiveLevel;
            deathsInCurrentAdaptiveLevel = 0;
            winsInCurrentAdaptiveLevel = 0;
            UpdateEnemySpeedForAdaptiveLevel();
            Debug.Log("Descente au niveau adaptatif : " + adaptiveLevel);
            GenerateAdaptiveLevel();
            return;
        }

        // sinon on remet simplement le joueur au départ du niveau courant
        ResetPlayerToStart();
    }

    // cette fonction replace le joueur à la case de départ du niveau courant
    public void ResetPlayerToStart()
    {
        if (currentPlayer == null)
        {
            return;
        }

        // sécurité contre les positions invalides
        if (!IsFiniteVector(currentPlayerStartPosition))
        {
            currentPlayerStartPosition = new Vector3(cellSize, 1f, cellSize);
        }

        CharacterController controller = currentPlayer.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        currentPlayer.transform.position = currentPlayerStartPosition;

        if (controller != null)
        {
            controller.enabled = true;
        }

        Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // cette fonction donne le nombre d'ennemis selon la difficulté actuelle
    public int GetEnemyCountForCurrentDifficulty()
    {
        if (!isAdaptiveMode)
        {
            return 1;
        }

        if (adaptiveLevel == 2)
        {
            return 1;
        }
        else if (adaptiveLevel == 3)
        {
            return 2;
        }
        else if (adaptiveLevel == 4)
        {
            return 3;
        }
        else if (adaptiveLevel >= 5)
        {
            return 4;
        }

        return 1;
    }

    // cette fonction fait apparaître les ennemis du niveau selon la difficulté actuelle
    public void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            return;
        }

        if (currentMaze == null)
        {
            return;
        }

        int enemyCount = GetEnemyCountForCurrentDifficulty();
        List<Vector3> usedSpawnPositions = new List<Vector3>();

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = GetRandomSafeEnemyPositionFarFromOthers(usedSpawnPositions);

            // sécurité absolue : si le spawn est invalide, on annule seulement cet ennemi
            if (!IsFiniteVector(spawnPosition))
            {
                Debug.LogWarning("Spawn ennemi annulé : position invalide");
                continue;
            }

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, levelParent);
            currentEnemies.Add(enemy);
            usedSpawnPositions.Add(spawnPosition);

            Vector3 targetPoint = GetRandomSafeEnemyPositionDifferentFrom(spawnPosition);

            // sécurité : si la cible est invalide ou trop proche, on force une cible plus loin
            if (!IsFiniteVector(targetPoint) || Vector3.Distance(targetPoint, spawnPosition) < cellSize)
            {
                targetPoint = spawnPosition + new Vector3(cellSize * 2f, 0f, 0f);
            }

            currentEnemyTargetPoints.Add(targetPoint);

            Debug.Log("Enemy " + i + " spawn OK at " + spawnPosition + " | target=" + targetPoint + " | total enemies=" + currentEnemies.Count);
        }
    }

    // cette fonction cherche une position sûre en évitant les spawns déjà utilisés
    public Vector3 GetRandomSafeEnemyPositionFarFromOthers(List<Vector3> usedPositions)
    {
        List<Vector3> possiblePositions = new List<Vector3>();
        float minSpacing = cellSize * enemySpacingMultiplier;

        int rows = currentMaze.GetLength(0);
        int cols = currentMaze.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int cell = currentMaze[r, c];

                if (cell == 0)
                {
                    Vector3 worldPos = new Vector3(c * cellSize, 1f, r * cellSize);

                    if (!IsSafeEnemyPosition(worldPos) || !IsFiniteVector(worldPos))
                    {
                        continue;
                    }

                    bool tooCloseToAnotherEnemy = false;
                    for (int i = 0; i < usedPositions.Count; i++)
                    {
                        if (Vector3.Distance(worldPos, usedPositions[i]) < minSpacing)
                        {
                            tooCloseToAnotherEnemy = true;
                            break;
                        }
                    }

                    if (!tooCloseToAnotherEnemy)
                    {
                        possiblePositions.Add(worldPos);
                    }
                }
            }
        }

        if (possiblePositions.Count == 0)
        {
            // fallback : on revient au système normal si on ne trouve pas assez d'espace
            return GetRandomSafeEnemyPosition();
        }

        int randomIndex = Random.Range(0, possiblePositions.Count);
        return possiblePositions[randomIndex];
    }

    // cette fonction choisit une cible de déplacement aléatoire différente de la position actuelle
    // ici l'ennemi peut traverser les murs, donc la cible n'a pas besoin d'être sur une case libre
    public Vector3 GetRandomSafeEnemyPositionDifferentFrom(Vector3 blockedPosition)
    {
        if (currentMaze == null)
        {
            return new Vector3(cellSize * 2f, 1f, cellSize * 2f);
        }

        int rows = currentMaze.GetLength(0);
        int cols = currentMaze.GetLength(1);

        float maxX = Mathf.Max(cellSize, (cols - 1) * cellSize);
        float maxZ = Mathf.Max(cellSize, (rows - 1) * cellSize);

        for (int i = 0; i < 60; i++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(0f, maxX),
                1f,
                Random.Range(0f, maxZ)
            );

            if (IsFiniteVector(candidate) && Vector3.Distance(candidate, blockedPosition) > cellSize * 2f)
            {
                return candidate;
            }
        }

        Vector3 fallback = blockedPosition + new Vector3(cellSize * 2f, 0f, 0f);
        if (!IsFiniteVector(fallback))
        {
            fallback = new Vector3(cellSize * 2f, 1f, cellSize * 2f);
        }
        return fallback;
    }

    // cette fonction choisit une position sûre aléatoire pour l'ennemi
    public Vector3 GetRandomSafeEnemyPosition()
    {
        List<Vector3> possiblePositions = new List<Vector3>();

        int rows = currentMaze.GetLength(0);
        int cols = currentMaze.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int cell = currentMaze[r, c];

                if (cell == 0)
                {
                    Vector3 worldPos = new Vector3(c * cellSize, 1f, r * cellSize);

                    if (IsSafeEnemyPosition(worldPos) && IsFiniteVector(worldPos))
                    {
                        possiblePositions.Add(worldPos);
                    }
                }
            }
        }

        if (possiblePositions.Count == 0)
        {
            return new Vector3(cellSize * 2f, 1f, cellSize * 2f);
        }

        int randomIndex = Random.Range(0, possiblePositions.Count);
        return possiblePositions[randomIndex];
    }

    // cette fonction évite de faire apparaître l'ennemi trop près du départ, des boutons ou de la sortie
    public bool IsSafeEnemyPosition(Vector3 worldPos)
    {
        float safeDistance = cellSize * enemySafeDistanceMultiplier;
        float corridorLength = cellSize * enemyCriticalCorridorLength;

        int rows = currentMaze.GetLength(0);
        int cols = currentMaze.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int cell = currentMaze[r, c];

                if (cell == 2 || cell == 3 || cell == 4 || cell == 5)
                {
                    Vector3 specialPos = new Vector3(c * cellSize, 1f, r * cellSize);

                    if (Vector3.Distance(worldPos, specialPos) <= safeDistance)
                    {
                        return false;
                    }

                    if (IsOnProtectedCorridor(worldPos, specialPos, corridorLength))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    // cette fonction évite que l'ennemi apparaisse sur le même couloir critique que le départ ou la sortie
    public bool IsOnProtectedCorridor(Vector3 worldPos, Vector3 anchorPos, float protectedLength)
    {
        float halfCell = cellSize * 0.5f;

        bool sameColumn = Mathf.Abs(worldPos.x - anchorPos.x) <= halfCell && Mathf.Abs(worldPos.z - anchorPos.z) <= protectedLength;
        bool sameRow = Mathf.Abs(worldPos.z - anchorPos.z) <= halfCell && Mathf.Abs(worldPos.x - anchorPos.x) <= protectedLength;

        return sameColumn || sameRow;
    }

    // cette fonction fait bouger tous les ennemis librement vers des cibles aléatoires
    public void UpdateEnemyMovement()
    {
        if (currentEnemies == null || currentEnemies.Count == 0)
        {
            return;
        }

        for (int i = 0; i < currentEnemies.Count; i++)
        {
            GameObject enemy = currentEnemies[i];

            if (enemy == null)
            {
                continue;
            }

            if (i >= currentEnemyTargetPoints.Count)
            {
                currentEnemyTargetPoints.Add(GetRandomSafeEnemyPositionDifferentFrom(enemy.transform.position));
            }

            Vector3 currentTarget = currentEnemyTargetPoints[i];

            if (!IsFiniteVector(enemy.transform.position))
            {
                enemy.transform.position = GetRandomSafeEnemyPosition();
            }

            if (!IsFiniteVector(currentTarget) || currentTarget == Vector3.zero || Vector3.Distance(enemy.transform.position, currentTarget) < 0.1f)
            {
                currentTarget = GetRandomSafeEnemyPositionDifferentFrom(enemy.transform.position);
                currentEnemyTargetPoints[i] = currentTarget;
            }

            enemy.transform.position = Vector3.MoveTowards(
                enemy.transform.position,
                currentTarget,
                enemyMoveSpeed * Time.deltaTime
            );

            enemy.transform.position = new Vector3(
                enemy.transform.position.x,
                1f,
                enemy.transform.position.z
            );

            if (Vector3.Distance(enemy.transform.position, currentTarget) < 0.1f)
            {
                currentEnemyTargetPoints[i] = GetRandomSafeEnemyPositionDifferentFrom(enemy.transform.position);
            }
        }
    }

    // cette fonction affiche le panel de game over avec le score final
    public void ShowGameOverPanel()
    {
        gameFinished = true;
        isLoadingNextLevel = false;

        Debug.Log("ShowGameOverPanel appelée");

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "Score : " + score.ToString();
            Debug.Log("Score text OK");
        }
        else
        {
            Debug.Log("gameOverScoreText est NULL");
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("gameOverPanel activé");
        }
        else
        {
            Debug.Log("gameOverPanel est NULL");
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        if (tutorialMessagePanel != null)
        {
            tutorialMessagePanel.SetActive(false);
        }

        if (tutorialCompletePanel != null)
        {
            tutorialCompletePanel.SetActive(false);
        }

        Time.timeScale = 0f;
        currentEnemies.Clear();
        currentEnemyTargetPoints.Clear();
        winsInCurrentAdaptiveLevel = 0;
        deathsInCurrentAdaptiveLevel = 0;
        level5WinStreak = 0;
    }

    // cette fonction affiche le panel de victoire avec le score final
    public void ShowVictoryPanel()
    {
        gameFinished = true;
        isLoadingNextLevel = false;

        if (victoryText != null)
        {
            victoryText.text = "Victory !";
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        if (tutorialMessagePanel != null)
        {
            tutorialMessagePanel.SetActive(false);
        }

        if (tutorialCompletePanel != null)
        {
            tutorialCompletePanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Time.timeScale = 0f;
        currentEnemies.Clear();
        currentEnemyTargetPoints.Clear();
        winsInCurrentAdaptiveLevel = 0;
        deathsInCurrentAdaptiveLevel = 0;
        level5WinStreak = 0;
    }

    // cette fonction est appelée par le bouton du panel de game over
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    // cette fonction vérifie qu'une position est finie (pas NaN / pas Infinity)
    public bool IsFiniteVector(Vector3 value)
    {
        return !(float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z)
              || float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z));
    }
}