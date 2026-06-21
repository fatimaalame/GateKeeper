# GATE KEEPER

**Un jeu sérieux adaptatif en 3D pour comprendre les portes logiques en jouant**

*Run and solve the logic*

Projet réalisé sur Unity (3D Core) à l'université de Genève.
Par Fatima Alame & Mikael Sahakyan

---

## Le concept

GATE KEEPER apprend les portes logiques (AND, OR, NOT, XOR) par la pratique, au lieu d'une table de vérité abstraite. Le joueur se déplace dans un labyrinthe et doit ouvrir une porte de sortie qui se comporte comme une porte logique. Pour l'ouvrir, il active des entrées **A** et **B** en marchant sur des boutons posés au sol, observe l'état de la sortie sur le panneau logique, et déduit petit à petit la règle du niveau. L'idée : tester, se tromper, recommencer, et comprendre tout seul.

C'est un jeu sérieux : il mélange une intention pédagogique (la logique booléenne) avec de vraies mécaniques de jeu (exploration, réflexion, gestion du risque, progression).

---

## Du coup, comment jouer ?

| Action | Touche |
|---|---|
| Se déplacer | Flèches directionnelles |
| Vue première personne (POV) | `C` |
| Recommencer le niveau | `R` |
| Activer une entrée A / B | Marcher sur le bouton |

**Boucle de jeu :** se déplacer jusqu'aux boutons logiques -> activer/désactiver A et B -> comprendre quelle combinaison ouvre la porte -> éviter les ennemis -> atteindre la sortie une fois la porte ouverte.

**Règles des portes :**
- **AND** : A et B actifs
- **OR** : A ou B suffit
- **NOT** : la sortie s'ouvre quand A est désactivé (A est actif au départ, il faut donc le couper)
- **XOR** : une seule entrée doit être active

Le joueur a 5 vies. Un contact avec un ennemi coûte une vie et renvoie au départ du niveau. Atteindre la sortie avec la bonne logique rapporte des points et fait passer au niveau suivant. Plus aucune vie = game over avec le score final.

Deux modes :un tutoriel de 4 niveaux qui présente les portes une par une, puis le labyrinthe adaptatif (bouton « Enter the Maze »).

---

## L'adaptativité

Le jeu garde le joueur dans une zone ni trop facile, ni trop dure, avec deux systèmes complémentaires.

### Adaptativité digitale (par niveau) 
Le niveau va de 2 à 5 et bouge par sauts : **2 réussites de suite** font monter d'un cran, **2 morts** dans le niveau font redescendre, et au **niveau 5** il faut réussir **3 fois de suite** pour gagner.

| Niveau | Labyrinthe | Ennemis | Score | Temps cible |
|:---:|:---:|:---:|:---:|:---:|
| 2 | 9×9 | 1 | 10 | 90 s |
| 3 | 11×11 | 2 | 15 | 60 s |
| 4 | 13×13 | 3 | 20 | 45 s |
| 5 | 15×15 | 4 | 30 | 30 s |

La logique se complexifie aussi : `OR` -> `A AND (NOT B)` -> `((A AND B) OR A) XOR B` -> `(((A AND B) OR A) XOR B) AND A`. (On proposera de randomiser ces circuits plus tard)

### Adaptativité analogique avec la pression
Une valeur continue entre 0 et 1, affichée à l'écran, qui suit le joueur d'un niveau à l'autre. Elle monte de **+0,10** par réussite (+0,06 si rapide, +0,06 si sans dégât) et descend de **−0,18** à chaque collision. Elle module la **vitesse des ennemis** (×0,7 → ×1,7) et peut **ajouter jusqu'à 2 ennemis**. En clair : bien jouer rend le jeu plus nerveux, se faire toucher le calme.

---

## Architecture du code

Un cerveau central (`GameManager`) et des scripts spécialisés autour de lui (tous reliés via `GameManager.Instance`).

| Script | Rôle |
|---|---|
| `GameManager.cs` | Niveaux, génération, score, vies, adaptativité, ennemis, transitions |
| `MazeGenerator.cs` | Génère le labyrinthe (DFS récursif), place départ/porte/boutons |
| `LogicLevelRuntime.cs` | Structure de données d'un niveau (porte, description, grille) |
| `LogicTrigger.cs` | Active/désactive A ou B au passage du joueur |
| `ExitTrigger.cs` | Valide la sortie et charge le niveau suivant |
| `DoorTrigger.cs` | Teste l'état de la porte (debug) |
| `PlayerMovement.cs` | Déplacement du joueur (Character Controller) |
| `CameraFollow.cs` / `CameraSwitcher.cs` | Suivi caméra et bascule vue de dessus / POV |
| `EnemyPatrol.cs` | Collision ennemi-joueur et patrouille simple |
| `LogicUI.cs` / `StatusUI.cs` / `MiniMapUI.cs` | Interface : portes, vies, score, minimap |
| `AudioManager.cs` / `Timer.cs` / `MenuManager.cs` | Sons, chrono, navigation des menus |
| `SkyboxRotator.cs` | Rotation du fond étoilé |

---

## Installation

> Nécessite **Unity [6000.3.11f1]** sur unityhub

```bash
git clone [https://github.com/fatimaalame/GateKeeper.git]
```

## Objet 3D (OpenSCAD)

Le bouton au sol a été modélisé sous **OpenSCAD** de façon paramétrique base + bouton arrondis, puis exporté en STL → OBJ et importé dans Unity. Un trigger lui a été ajouté pour gérer l'interaction avec le joueur.

---

## Utilisation de l'IA

Un assistant IA (Claude, d'Anthropic) a été utilisé comme outil d'aide pendant le projet. Concrètement : relecture et correction du rapport en français, mise au propre de certaines explications (notamment la partie adaptativité), aide à la mise en forme de la présentation, et rédaction de la documentation des scripts.

Côté code, il a aidé à comprendre les fonctions, trouver facilements les bugs, proposer des solutions quand on avait des problèmes / questionnements. Il a également été très utiles pour les commentaires.

Tout ce qui a été produit avec l'aide de l'IA a été relu, vérifié et validé par nos soins. Nous assumons le contenu final du projet.

## Crédits & assets

Le projet réutilise des assets de l'Unity Asset Store (polices pixel, skybox étoilée, textures, corridor sci-fi), des sons libres, et s'appuie sur des tutoriels de génération de labyrinthe et la documentation Unity. La liste complète est dans le rapport (Annexe — Bibliographie des imports).

Assets Unity utilisés

- Unity Asset Store, Health System Lite: Health Bar & Hitbox. Available at: https://assetstore.unity.com/packages/tools/
game-toolkits/health-system-lite-health-bar-hitbox-248090
- Unity Asset Store, BoldPixels Font. Available at: https://assetstore.unity.com/packages/2d/fonts/boldpixels-
font-332078
- Unity Asset Store, Free Retro Pixel Font GNF. Available at: https://assetstore.unity.com/packages/p/free-retro-pixel-
font-gnf-322855
- Unity Asset Store, Real Stars Skybox Lite. Available at: https://assetstore.unity.com/packages/p/real-stars-skybox-
lite-116333
- Unity Asset Store, Free Stylized PBR Textures Pack. Available at: https://assetstore.unity.com/packages/p/free-
stylized-pbr-textures-pack-111778 (Accessed: 18 June 2026).
- Unity Asset Store, Modular Sci-Fi Corridor. Available at: https://assetstore.unity.com/packages/3d/environments/sci-fi/
modular-sci-fi-corridor-142811

Génération procédurale et structure du labyrinthe

- Unity Discussions, Best Way to Design Chunk System. Available at: https://discussions.unity.com/t/best-way-to-
design-chunk-system/841934
- Catlike Coding, Hex Map. Available at: https://catlikecoding.com/unity/tutorials/hex-map/
- YouTube, Maze Generator Tutorial. Available at: https://www.youtube.com/watch?v=_aeYq5BmDMg
- YouTube, Procedural Generation Playlist. Available at: https://www.youtube.com/playlist?
list=PLQMQNmwN3FvzfmC4HoVhhBZbSdKeHCAH5
- YouTube, How to Make a Maze Generation Algorithm in Unity. Available at: https://www.youtube.com/watch?
v=OutlTTOm17M

Interface, mini-map et éléments visuels, audio

- YouTube, Minimap Tutorial. Available at: https://www.youtube.com/watch?v=5GySJHXajso
- YouTube, TIMER & COUNTDOWN Available at: https://www.youtube.com/watch?v=POq1i8FyRyQ
- Freepik, Heart Star Images, Vectors and Photos. Available at: https://www.freepik.com/free-photos-vectors/heart-star
- https://pixabay.com/sound-eﬀects/search/sci%20fi%20video%20game/

Documentation Unity et programmation

- Unity Cheat Sheet, Scene Loading. Available at: https://unitycheatsheet.com/#/scripting/asyncawait?id=scene-
loading
- Unity Cheat Sheet, Rigidbody. Available at: https://unitycheatsheet.com/#/physics/rigidbody
- Unity Cheat Sheet, Moving Rigidbodies. Available at: https://unitycheatsheet.com/#/physics/rigidbody?id=moving-
rigidbodies
- Unity Cheat Sheet, Camera Follow / Orbit Example. Available at: https://unitycheatsheet.com/#/practical-use-cases/
camera-follow-orbit?id=example
- Unity Cheat Sheet, Unity Transform: Position, Rotation and Scale. Available at: https://unitycheatsheet.com/#/basics/
transform?id=unity-transform-position-rotation-and-scale
- M. Ansley, Variables: Public, Private, and Naming Conventi

---
