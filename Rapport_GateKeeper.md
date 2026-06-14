# GATE KEEPER — Rapport final

**Jeu sérieux adaptatif sur les portes logiques**
Fatima Alamé & Mikael Sahakyan
Projet réalisé sous Unity (3D Core)

---

## 1. But et principes du jeu

GATE KEEPER est un jeu sérieux adaptatif en 3D dont le but est de faire comprendre les portes logiques de base (AND, OR, NOT, XOR) par la pratique plutôt qu'avec une table de vérité abstraite.

Le joueur se déplace dans un labyrinthe et doit ouvrir une porte de sortie qui se comporte comme une porte logique. Pour ça, il active des entrées A et B en marchant sur des boutons posés au sol, observe l'état de la sortie sur le panneau logique, et finit par déduire la règle du niveau. L'idée est que le joueur teste, se trompe, recommence et comprenne la logique tout seul.

Le jeu combine donc une intention pédagogique claire (la logique booléenne) avec de vraies mécaniques de jeu : exploration, réflexion, gestion du risque et progression. Le domaine éducatif visé est l'informatique et la logique, pour un public lycéen ou étudiant débutant.

## 2. Règles du jeu et gameplay

Le joueur contrôle un avatar (une capsule) dans un labyrinthe vu de dessus. Il peut basculer en vue première personne avec la touche C.

La boucle de jeu est la suivante :

- se déplacer dans le labyrinthe jusqu'aux boutons logiques,
- activer ou désactiver les entrées A et B en marchant dessus,
- comprendre quelle combinaison ouvre la porte selon le type du niveau,
- éviter les ennemis qui se déplacent dans le labyrinthe,
- atteindre la sortie une fois la porte ouverte.

Chaque porte logique a sa propre règle. Pour AND, il faut A et B actifs. Pour OR, il suffit de A ou B. Pour NOT, la sortie s'ouvre quand A est désactivé (l'entrée est active au départ, le joueur doit donc la couper). Pour XOR, il faut qu'une seule entrée soit active.

Le joueur dispose de 5 vies. Quand un ennemi le touche, il perd une vie et revient au point de départ du niveau. Quand il atteint la sortie avec la bonne logique, il gagne des points et passe au niveau suivant. Quand il n'a plus de vies, c'est le game over avec affichage du score final.

Le jeu est divisé en deux parties. D'abord un tutoriel de 4 niveaux qui introduit chaque porte une par une (OR, AND, NOT, XOR). Ensuite le vrai mode, lancé par le bouton Enter the Maze, où le joueur entre dans le labyrinthe adaptatif.

## 3. Règles d'adaptativité

L'adaptativité est le cœur du projet. Le jeu ajuste sa difficulté en continu pour garder le joueur dans une zone stimulante, ni trop facile ni trop dure. Il agit sur deux axes en même temps.

**Adaptation par la performance.** Si le joueur réussit un niveau deux fois de suite, le niveau adaptatif monte d'un cran (de 2 jusqu'à 5). S'il meurt deux fois dans le même niveau, le niveau redescend d'un cran. Au niveau 5, il faut réussir trois fois de suite pour gagner la partie, ce qui évite qu'une réussite chanceuse suffise.

**Adaptation par le temps.** Le jeu chronomètre chaque niveau et le compare à un temps cible qui dépend de la difficulté (90 s, 60 s, 45 s puis 30 s). Deux réussites rapides d'affilée font monter la difficulté, et un temps vraiment trop long (plus de 1,5 fois la cible) la fait baisser.

Le niveau adaptatif courant fait varier quatre paramètres d'un coup :

- la taille du labyrinthe, qui passe de 9 à 11, 13 puis 15 cases de côté,
- la vitesse des ennemis, calculée à partir d'une vitesse de base augmentée à chaque niveau,
- le nombre d'ennemis, qui passe de 1 à 2, 3 puis 4,
- le score gagné par niveau (10, 15, 20 puis 30 points), pour valoriser les niveaux durs.

On a donc un système qui n'est pas juste un labyrinthe plus grand au hasard, mais une vraie difficulté multi-paramètres pilotée par ce que fait le joueur.

## 4. Structure de l'application

Le projet est organisé en plusieurs scènes : le menu principal, l'écran Comment jouer, la scène du tutoriel, la scène du vrai jeu adaptatif (AdaptiveMazeScene), plus les panneaux de game over et de victoire.

Les GameObjects principaux sont le joueur, les murs et le sol, les boutons A et B, la porte, les ennemis, le trigger de sortie, la caméra, la minimap et l'interface.

Les scripts principaux et leur rôle :

- **GameManager.cs** : le cerveau du jeu. Il crée et charge les niveaux, génère le labyrinthe, fait apparaître le joueur et les ennemis, évalue les portes logiques, gère le score, les vies, toute la logique d'adaptativité et les transitions vers game over ou victoire.
- **MazeGenerator.cs** : génère le labyrinthe de façon procédurale avec un DFS récursif, puis place le départ, la porte et les boutons logiques.
- **LogicLevelRuntime.cs** : structure de données qui stocke le type de porte, la description et la grille d'un niveau.
- **LogicTrigger.cs** : active ou désactive A ou B quand le joueur entre dans la zone d'un bouton.
- **DoorTrigger.cs et ExitTrigger.cs** : évaluent la porte logique du niveau. ExitTrigger charge le niveau suivant si la porte est ouverte.
- **PlayerMovement.cs** : déplacement du joueur via un Character Controller.
- **CameraFollow.cs et CameraSwitcher.cs** : caméra qui suit le joueur, et bascule entre vue d'ensemble et vue première personne (touche C).
- **EnemyPatrol.cs** : comportement des ennemis (le déplacement libre est piloté par GameManager).
- **LogicUI.cs et StatusUI.cs** : affichage du panneau logique (porte, A, B, sortie) et du HUD (vies, score, niveau, difficulté).
- **MiniMapUI.cs** : construit la minimap à partir de la grille du labyrinthe.
- **AudioManager.cs, Timer.cs, MenuManager.cs** : sons, chrono et navigation entre les menus.

Conformément à la consigne, chaque membre du binôme a modélisé un objet sous OpenSCAD. Le bouton interactif a été modélisé dans OpenSCAD, exporté en STL puis converti en OBJ pour pouvoir l'importer dans Unity. On a gardé le modèle OpenSCAD pour le visuel et créé un objet trigger séparé pour porter la logique d'interaction.

## 5. Tests et évaluation par un autre groupe

Le jeu a été testé par un autre groupe (à compléter : nom et groupe du testeur). Les retours sont les suivants.

Côté points forts, le jeu est jugé intuitif, beau et amusant à jouer. Le concept des portes logiques passe bien et la prise en main est rapide.

Côté critiques, le testeur relève surtout qu'aux niveaux élevés, on passe plus de temps à éviter les ennemis qu'à réfléchir aux portes logiques, ce qui éloigne du but pédagogique. Le niveau 5 avec quatre ennemis est jugé trop punitif. Il signale aussi un délai sur la zone de sortie où le passage au niveau suivant ne se déclenche pas tout de suite, des contrôles incohérents (le WASD ne fonctionne pas alors qu'il est annoncé), et le fait que les niveaux NOT et XOR n'affichent qu'un seul bouton alors que le panneau montre A et B.

**Notre analyse.** Ces retours sont justifiés et on les a vérifiés dans le code. La pression des ennemis qui prend le dessus sur la logique à haute difficulté est un vrai problème d'équilibrage : l'adaptativité augmente le nombre et la vitesse des ennemis, ce qui finit par masquer l'objectif d'apprentissage. Le bug de contrôle vient du fait que le déplacement a été recodé en flèches directionnelles, ce qui a cassé le support WASD d'origine. Le souci du bouton manquant en XOR vient du générateur, qui ne place le bouton B que pour OR et AND : c'est correct pour NOT (une seule entrée), mais pas pour XOR, qui a besoin des deux entrées pour montrer tous les cas. Le délai sur la sortie est probablement lié au repositionnement du trigger de sortie à chaque niveau.

## 6. Conclusion et perspectives

GATE KEEPER atteint son objectif de départ : transformer la logique booléenne en une expérience de jeu jouable, avec un tutoriel progressif et un vrai mode adaptatif qui ajuste la difficulté aux performances du joueur. Le point le plus abouti est l'adaptativité, qui agit sur plusieurs paramètres à la fois.

Les pistes d'amélioration, en partie issues de l'évaluation, sont :

- rééquilibrer la pression des ennemis pour qu'elle ne dépasse pas le défi logique aux niveaux élevés,
- corriger le déplacement WASD et le délai sur la zone de sortie,
- placer le bouton B en XOR pour couvrir tous les cas de la table de vérité,
- ajouter les portes NAND et NOR, déjà annoncées sur l'écran titre,
- proposer des combinaisons de portes et un compteur de sous-niveau pour aller plus loin.
