using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Media;
using System.Diagnostics;
using System.Threading;

namespace TreasureHunt
{
    class Program
    {
        //Empêche le joueur d'entrer autre chose qu'un chiffre lors d'une saisie sans passer par une classe
        static string GererExecptionFormat(string nb)
        {
            while (Int32.TryParse(nb, out int resultat) == false)
            {
                Console.Write("\nCe n'est pas un chiffre ! Veuillez reproposer une réponse : ");
                nb = Console.ReadLine();
            }
            return nb;
        }
        //Copie les valeurs d'un tableau dans un autre tableau de même dimension
        static void TransfererValeursTableau(int[,] tab1, int[,] tab2)
        {
            for (int i = 0; i < tab1.GetLength(0); i++)
                for (int j = 0; j < tab1.GetLength(1); j++)
                {
                    tab1[i, j] = tab2[i, j];
                }
        }
        //Détermine le maximum entre deux entiers
        static int DeterminerMaximum(int a, int b)
        {
            if (a < b)
                return b;
            else
                return a;
        }



        //Associe une taille minimale de grille au niveau choisi par le joueur
        static int AttribuerTailleSelonNiveau(int choixNiveau)
        {
            int tailleMini;
            if (choixNiveau == 1)
                tailleMini = 5;
            else if (choixNiveau == 2)
                tailleMini = 8;
            else
                tailleMini = 12;
            return tailleMini;
        }
        //Associe chaque musique à son niveau de jeu et la lance 
        static void ChoisirMusique(int choixNiveau)
        {
            if (choixNiveau == 1)
            {
                SoundPlayer player = new SoundPlayer("./assets/sounds/easy.wav");
                player.PlayLooping();
            }
            else if (choixNiveau == 2)
            {
                SoundPlayer player = new SoundPlayer("./assets/sounds/moyen.wav");
                player.PlayLooping();
            }
            else
            {
                SoundPlayer player = new SoundPlayer("./assets/sounds/crypteque.wav");
                player.PlayLooping();
            }
        }
        //Calcule le temps imparti pour le mode difficile en fonction de la taille de la grille
        static int ObtenirTempsImparti(string[,] grilleRef)
        {
            int tempsImparti = 8, tailleGrilleLigne = grilleRef.GetLength(0), tailleGrilleColonne = grilleRef.GetLength(1);
            if (tailleGrilleColonne == tailleGrilleLigne)
                tempsImparti += tailleGrilleColonne - 12;
            else
                tempsImparti += DeterminerMaximum(tailleGrilleLigne, tailleGrilleColonne) - 12;
            return tempsImparti;
        }
        //Actualise la grille de jeu en mode facile au cas où la bombe détruite par le trésor se trouve proche des cases déjà découvertes
        static string[,] ActualiserGrilleJeu(string[,] grilleJeu, string[,] grilleRef)
        {
            for (int i = 0; i < grilleJeu.GetLength(0); i++)
            {
                for (int j = 0; j < grilleJeu.GetLength(1); j++)
                {
                    if (grilleJeu[i, j] != "? ")
                        grilleJeu[i, j] = grilleRef[i, j];
                }
            }
            return grilleJeu;
        }
        //Demande au joueur la ligne/colonne qu'il souhaite explorer et lui donne le nombre de mines qui s'y trouve
        static string AfficherRecompenseNiveauMoyen(string[,] grilleRef)
        {
            string messageRecompenseMoyen = "";
            Console.Write("Souhaitez vous explorer une ligne (1) ou une colonne (2) ? ");
            int choixLigneColonne = int.Parse(Console.ReadLine());
            if (choixLigneColonne == 1)
            {
                Console.WriteLine("Saisissez un numéro de ligne. Nous vous révèlerons le nombre de mines qui s'y trouve.");
                int numeroLigneExplore = int.Parse(Console.ReadLine()) - 1, nbMinesLigneChoisie = 0;
                while (numeroLigneExplore < 0 || numeroLigneExplore >= grilleRef.GetLength(0))
                {
                    Console.WriteLine("Il faut que la ligne soit dans la grille ! Rentrez une autre valeur svp");
                    numeroLigneExplore = int.Parse(Console.ReadLine()) - 1;
                }
                for (int i = 0; i < grilleRef.GetLength(1); i++)
                {
                    if (grilleRef[numeroLigneExplore, i] == "M ")
                        nbMinesLigneChoisie += 1;

                }
                messageRecompenseMoyen = "Sur cette ligne, il y a " + nbMinesLigneChoisie + " mine(s). Bonne chance !";
            }
            else if (choixLigneColonne == 2)
            {
                Console.WriteLine("Saisissez un numéro de colonne. Nous vous révèlerons le nombre de mines qui s'y trouvent.");
                int numeroColonneExplore = int.Parse(Console.ReadLine()) - 1, nbMinesColonneChoisie = 0;
                while (numeroColonneExplore < 0 || numeroColonneExplore >= grilleRef.GetLength(0))
                {
                    Console.WriteLine("Il faut que la colonne soit dans la grille ! Rentrez une autre valeur svp");
                    numeroColonneExplore = int.Parse(Console.ReadLine()) - 1;
                }
                for (int i = 0; i < grilleRef.GetLength(0); i++)
                {
                    if (grilleRef[i, numeroColonneExplore] == "M ")
                        nbMinesColonneChoisie += 1;

                }
                messageRecompenseMoyen = "Sur cette colonne, il y a " + nbMinesColonneChoisie + " mine(s). Bonne chance !";
            }
            return messageRecompenseMoyen;
        }
        //Affiche la grille de référence pendant une demie seconde 
        static void AfficherRecompenseNiveauDifficile(string[,] grilleRef)
        {
            Console.Clear();
            string regardezGrille = File.ReadAllText("./assets/sprites/RegardezGrille.txt");
            Console.WriteLine(regardezGrille);
            Thread.Sleep(2000);
            Console.Clear();
            AfficherGrille(grilleRef);
            Thread.Sleep(450);
            Console.Clear();
        }
        //Retire une mine de la grille de référence et recalcule le décompte de chaque case 
        static string[,] DetruireMine(string[,] grilleRef, int[,] positionMine)
        {
            Console.Clear();
            Random hasard = new Random();
            int randPos = hasard.Next(0, positionMine.GetLength(0));
            Console.WriteLine("Vous avez détruit une mine : BOOM");
            int ligneMineDetruite = positionMine[randPos, 0];
            int colonneMineDetruite = positionMine[randPos, 1];
            grilleRef[ligneMineDetruite, colonneMineDetruite] = "  ";
            grilleRef = AttribuerChiffresGrilleRef(grilleRef);
            return grilleRef;
        }
        //Répertorie les coordonnées des cases contenant des mines
        static int[,] PositionsMines(string[,] grilleRef, int nbMines)
        {
            int[,] positionMine = new int[nbMines, 2];
            int cpt = 0;
            for (int i = 0; i < grilleRef.GetLength(0); i++)
            {
                for (int j = 0; j < grilleRef.GetLength(1); j++)
                {
                    if (grilleRef[i, j] == "M ")
                    {
                        positionMine[cpt, 0] = i;
                        positionMine[cpt, 1] = j;
                        cpt += 1;
                    }
                }
            }
            return positionMine;
        }



        //Crée la grille selon la taille choisie par le joueur 
        static string[,] InitialiserGrille(int choixNiveau)
        {
            int tailleGrilleMini = AttribuerTailleSelonNiveau(choixNiveau);
            Console.Write("Combien de lignes voulez-vous dans votre grille de jeu ? Attention ! Pour ce niveau, il doit être d'au moins {0}.\n", tailleGrilleMini);
            //S'assure que la saisie pour le nombre de lignes est un chiffre supérieur à la taille de grille minimale
            string reponseJoueur = Console.ReadLine();
            int nbLignes = Convert.ToInt32(GererExecptionFormat(reponseJoueur));
            while (nbLignes < tailleGrilleMini)
            {
                Console.Write("Ce n'est pas supérieur à {0} ça ... Veuillez reproposez un chiffre : ", tailleGrilleMini);
                reponseJoueur = Console.ReadLine();
                nbLignes = Convert.ToInt32(GererExecptionFormat(reponseJoueur));
            }
            Console.Write("Combien de colonnes voulez-vous dans votre grille de jeu ? Attention ! Pour ce niveau, il doit être d'au moins {0}.\n", tailleGrilleMini);
            //S'assure que la saisie pour le nombre de colonnes est un chiffre supérieur à la taille de grille minimale
            reponseJoueur = Console.ReadLine();
            int nbColonnes = Convert.ToInt32(GererExecptionFormat(reponseJoueur));
            while (nbColonnes < tailleGrilleMini)
            {
                Console.Write("Ce n'est pas supérieur à {0} ça ... Veuillez reproposez un chiffre : ", tailleGrilleMini);
                nbColonnes = int.Parse(Console.ReadLine());
            }
            string[,] grille = new string[nbLignes, nbColonnes];
            //Remplit la grille de "?"
            for (int i = 0; i < nbLignes; i++)
            {
                for (int j = 0; j < nbColonnes; j++)
                {
                    grille[i, j] = "? ";
                }
            }
            return grille;
        }
        //Dessine et affiche la grille 
        static void AfficherGrille(string[,] grille)
        {
            //Affichage des numéros de ligne
            Console.Write("      ");
            for (int j = 0; j < grille.GetLength(1); j++)
                Console.Write("   {0}  ", j + 1);
            Console.WriteLine("");
            Console.Write("      ");
            //Affichage de la première ligne
            for (int j = 0; j < grille.GetLength(1); j++)
                Console.Write(" _____");
            Console.WriteLine("");
            //Affichage du quadrillage et du contenu de la grille
            for (int i = 0; i < grille.GetLength(0); i++)
            {
                if (i < 9)
                    Console.Write("  {0}   |", i + 1);
                else
                    Console.Write("  {0}  |", i + 1);
                for (int j = 0; j < grille.GetLength(1); j++)
                    Console.Write("  {0} |", grille[i, j]);
                Console.WriteLine();
                Console.Write("      |");
                //Affichage de la dernière ligne
                for (int j = 0; j < grille.GetLength(1); j++)
                    Console.Write("_____|");
                Console.WriteLine(" ");
            }

        }
        //Place les mines et trésors dans la grille
        static string[,] GenererMinesTresors(string[,] grille, int[] coordChoix, int nbMines, int nbTresors)
        {
            int randLigne, randColonne, cptMines = 0, cptTresors = 0, lignes = grille.GetLength(0), colonnes = grille.GetLength(1);
            bool caseLibre = true;
            int[,] coordMines = new int[nbMines + 1, 2], coordTresors = new int[nbTresors + 1, 2];
            Random aleatoire = new Random();
            //Positionnement des mines
            while (cptMines < nbMines)
            {
                randLigne = aleatoire.Next(0, lignes);
                randColonne = aleatoire.Next(0, colonnes);
                coordMines[0, 0] = coordChoix[0];
                coordMines[0, 1] = coordChoix[1];
                for (int i = 0; i < nbMines + 1; i++)
                {
                    if (randLigne == coordMines[i, 0] && randColonne == coordMines[i, 1])
                        caseLibre = false;
                }
                if (caseLibre)
                {
                    grille[randLigne, randColonne] = "M ";
                    cptMines++;
                    coordMines[cptMines, 0] = randLigne;
                    coordMines[cptMines, 1] = randColonne;
                }
                caseLibre = true;
            }
            //Positionnement des trésors
            while (cptTresors < nbTresors)
            {
                randLigne = aleatoire.Next(0, lignes);
                randColonne = aleatoire.Next(0, colonnes);
                coordTresors[0, 0] = coordChoix[0];
                coordTresors[0, 1] = coordChoix[1];
                for (int i = 0; i < nbTresors + 1; i++)
                {
                    if (randLigne == coordTresors[i, 0] && randColonne == coordTresors[i, 1])
                        caseLibre = false;
                }
                if (caseLibre)
                {
                    grille[randLigne, randColonne] = "T ";
                    cptTresors++;
                    coordTresors[cptTresors, 0] = randLigne;
                    coordTresors[cptTresors, 1] = randColonne;
                }
                caseLibre = true;
            }
            return grille;
        }



        //Demande au joueur la case qu'il souhaite découvrir
        static int[] RecupererChoixJoueur(string[,] grilleRef)
        {
            string reponseJoueur;
            int[] coordChoix = new int[2];
            Console.WriteLine("\nIndiquez la ligne de votre choix :");
            //S'assure que la saisie pour la ligne choisie est un chiffre se trouvant dans la grille
            reponseJoueur = Console.ReadLine();
            coordChoix[0] = Convert.ToInt32(GererExecptionFormat(reponseJoueur)) - 1;
            while (coordChoix[0] < 0 || coordChoix[0] > grilleRef.GetLength(0) - 1)
            {
                Console.Write("Vous êtes hors de la grille ! Veuillez reproposer une réponse : ");
                reponseJoueur = Console.ReadLine();
                coordChoix[0] = Convert.ToInt32(GererExecptionFormat(reponseJoueur)) - 1;
            }
            Console.WriteLine("\nIndiquez la colonne de votre choix :");
            //S'assure que la saisie pour la colonne choisie est un chiffre se trouvant dans la grille
            reponseJoueur = Console.ReadLine();
            coordChoix[1] = Convert.ToInt32(GererExecptionFormat(reponseJoueur)) - 1;
            while (coordChoix[1] < 0 || coordChoix[1] > grilleRef.GetLength(1) - 1)
            {
                Console.Write("Vous êtes hors de la grille ! Veuillez reproposer une réponse : ");
                reponseJoueur = Console.ReadLine();
                coordChoix[1] = Convert.ToInt32(GererExecptionFormat(reponseJoueur)) - 1;
            }
            return coordChoix;
        }
        //Vérifie si la case choisie est un coin de la grille
        static bool EtreDansUnCoin(string[,] grilleJeu, int ligne, int colonne)
        {
            bool dansUnCoin = false;
            if ((ligne == 0 && colonne == 0) || (ligne == grilleJeu.GetLength(0) - 1 && colonne == 0) || (ligne == grilleJeu.GetLength(0) - 1 && colonne == grilleJeu.GetLength(1) - 1) || (ligne == 0 && colonne == grilleJeu.GetLength(1) - 1))
                dansUnCoin = true;
            return dansUnCoin;
        }
        //Vérifie si la case choisie est sur un bord de la grille
        static bool EtreSurUnBord(string[,] grilleJeu, int ligne, int colonne)
        {
            bool surUnBord = false;
            //Bord gauche
            if ((ligne > 0 && ligne < grilleJeu.GetLength(0) - 1) && colonne == 0)
            {
                surUnBord = true;
            }
            //Bord haut
            else if ((colonne > 0 && colonne < grilleJeu.GetLength(1) - 1) && ligne == 0)
            {
                surUnBord = true;
            }
            //Bord droit
            else if ((ligne > 0 && ligne < grilleJeu.GetLength(0) - 1) && colonne == grilleJeu.GetLength(1) - 1)
            {
                surUnBord = true;
            }
            //Bord bas
            else if (((colonne > 0 && colonne < grilleJeu.GetLength(1) - 1) && ligne == grilleJeu.GetLength(0) - 1))
            {
                surUnBord = true;
            }

            return surUnBord;
        }
        //Répertorie les positions voisines possibles pour chaque coin de la grille
        static int[,] ObtenirCoordonneesVoisinesCoin(string[,] grilleJeu, int ligne, int colonne)
        {
            int[,] coordonneesVoisines = new int[3, 2];
            //Coin supérieur gauche
            if (ligne == 0 && colonne == 0)
            {
                int[,] tab = { { ligne, colonne + 1 }, { ligne + 1, colonne + 1 }, { ligne + 1, colonne } };
                TransfererValeursTableau(coordonneesVoisines, tab);
            }
            //Coin inférieur gauche
            else if (ligne == grilleJeu.GetLength(0) - 1 && colonne == 0)
            {
                int[,] tab = { { ligne - 1, colonne }, { ligne - 1, colonne + 1 }, { ligne, colonne + 1 } };
                TransfererValeursTableau(coordonneesVoisines, tab);

            }
            //Coin inférieur droit
            else if (ligne == grilleJeu.GetLength(0) - 1 && colonne == grilleJeu.GetLength(1) - 1)
            {
                int[,] tab = { { ligne - 1, colonne }, { ligne - 1, colonne - 1 }, { ligne, colonne - 1 } };
                TransfererValeursTableau(coordonneesVoisines, tab);

            }
            //Coin supérieur droit
            else if (ligne == 0 && colonne == grilleJeu.GetLength(1) - 1)
            {
                int[,] tab = { { ligne, colonne - 1 }, { ligne + 1, colonne - 1 }, { ligne + 1, colonne } };
                TransfererValeursTableau(coordonneesVoisines, tab);


            }
            return coordonneesVoisines;
        }
        //Répertorie les positions voisines possibles pour chaque bord de la grille
        static int[,] ObtenirCoordonneesVoisinesBord(string[,] grilleJeu, int ligne, int colonne)
        {
            int[,] coordonneesVoisines = new int[5, 2];
            //Bord gauche
            if ((ligne > 0 && ligne < grilleJeu.GetLength(0) - 1) && colonne == 0)
            {
                int[,] tab = { { ligne - 1, colonne }, { ligne - 1, colonne + 1 }, { ligne, colonne + 1 }, { ligne + 1, colonne + 1 }, { ligne + 1, colonne } };
                TransfererValeursTableau(coordonneesVoisines, tab);
            }
            //Bord haut
            else if ((colonne > 0 && colonne < grilleJeu.GetLength(1) - 1) && ligne == 0)
            {
                int[,] tab = { { ligne, colonne - 1 }, { ligne + 1, colonne - 1 }, { ligne + 1, colonne }, { ligne + 1, colonne + 1 }, { ligne, colonne + 1 } };
                TransfererValeursTableau(coordonneesVoisines, tab);
            }
            //Bord droit
            else if ((ligne > 0 && ligne < grilleJeu.GetLength(0) - 1) && colonne == grilleJeu.GetLength(1) - 1)
            {
                int[,] tab = { { ligne - 1, colonne }, { ligne - 1, colonne - 1 }, { ligne, colonne - 1 }, { ligne + 1, colonne - 1 }, { ligne + 1, colonne } };
                TransfererValeursTableau(coordonneesVoisines, tab);
            }
            //Bord bas
            else if (((colonne > 0 && colonne < grilleJeu.GetLength(1) - 1) && ligne == grilleJeu.GetLength(0) - 1))
            {
                int[,] tab = { { ligne, colonne - 1 }, { ligne - 1, colonne - 1 }, { ligne - 1, colonne }, { ligne - 1, colonne + 1 }, { ligne, colonne + 1 } };
                TransfererValeursTableau(coordonneesVoisines, tab);
            }
            return coordonneesVoisines;
        }
        //Répertorie les positions voisines possibles pour une case qui n'est ni un bord ni un coin
        static int[,] ObtenirCoordonneesVoisinesAutre(string[,] grilleJeu, int ligne, int colonne)
        {
            int[,] coordonneesVoisines = { { ligne - 1, colonne - 1 }, { ligne - 1, colonne }, { ligne - 1, colonne + 1 }, { ligne, colonne + 1 }, { ligne + 1, colonne + 1 }, { ligne + 1, colonne }, { ligne + 1, colonne - 1 }, { ligne, colonne - 1 } };
            return coordonneesVoisines;
        }
        //Détermine les coordonnées des cases voisines à la case choisie par le joueur
        static int[,] ObtenirCoordonneesVoisines(bool dansUnCoin, bool surUnBord, string[,] grilleJeu, int ligne, int colonne)
        {
            if (dansUnCoin == true)
            {
                return ObtenirCoordonneesVoisinesCoin(grilleJeu, ligne, colonne);
            }
            else if (surUnBord == true)
            {
                return ObtenirCoordonneesVoisinesBord(grilleJeu, ligne, colonne);
            }
            else
            {
                return ObtenirCoordonneesVoisinesAutre(grilleJeu, ligne, colonne);
            }
        }
        //Calcule le décompte pour chaque case de la grille 
        static int CompterMinesTresorsVoisins(bool dansUnCoin, bool surUnBord, string[,] grilleRef, int ligne, int colonne)
        {
            int nbMinesVoisines = 0;
            int[,] coordonneesVoisines = ObtenirCoordonneesVoisines(dansUnCoin, surUnBord, grilleRef, ligne, colonne);
            for (int i = 0; i < coordonneesVoisines.GetLength(0); i++)
            {

                if (grilleRef[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]] == "M ")
                {
                    nbMinesVoisines += 1;
                }
                else if (grilleRef[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]] == "T ")
                {
                    nbMinesVoisines += 2;
                }
            }
            return nbMinesVoisines;
        }
        //Modifie le contenu de la grille avec les décomptes obtenus 
        static string[,] AttribuerChiffresGrilleRef(string[,] grilleRef)
        {
            int nbMinesTresorsVoisins;
            for (int i = 0; i < grilleRef.GetLength(0); i++)
            {
                for (int j = 0; j < grilleRef.GetLength(1); j++)
                {
                    bool dansUnCoin = EtreDansUnCoin(grilleRef, i, j);
                    bool surUnBord = EtreSurUnBord(grilleRef, i, j);
                    if (grilleRef[i, j] != "M " && grilleRef[i, j] != "T ")
                    {

                        nbMinesTresorsVoisins = CompterMinesTresorsVoisins(dansUnCoin, surUnBord, grilleRef, i, j);
                        if (nbMinesTresorsVoisins == 0)
                            grilleRef[i, j] = "  ";
                        else
                            grilleRef[i, j] = nbMinesTresorsVoisins.ToString() + " ";
                    }
                }
            }
            return grilleRef;
        }
        //Découvre les cases voisines à la case choisie par le joueur 
        static string[,] DecouvrirCases(bool dansUnCoin, bool surUnBord, string[,] grilleRef, string[,] grilleJeu, bool[,] estDecouverte, int ligne, int colonne, int nbMines, int choixNiveau)
        {
            int[,] coordonneesVoisines;
            //La case choisie n'est pas une mine ou un trésor mais il y a au moins une mine ou un trésor autour d'elle
            if (estDecouverte[ligne, colonne] == false && grilleRef[ligne, colonne] != "  " && grilleRef[ligne, colonne] != "M " && grilleRef[ligne, colonne] != "T ")
            {
                grilleJeu[ligne, colonne] = grilleRef[ligne, colonne];
                estDecouverte[ligne, colonne] = true;
            }
            //La case choisie est un trésor
            else if (estDecouverte[ligne, colonne] == false && grilleRef[ligne, colonne] == "T ")
            {
                //Récompense si le joueur joue en mode difficile
                if (choixNiveau == 3)
                {
                    grilleJeu[ligne, colonne] = "T ";
                    AfficherRecompenseNiveauDifficile(grilleRef);
                }
                //Récompense si le joueur joue en mode facile
                else if (choixNiveau == 1)
                {
                    grilleRef = DetruireMine(grilleRef, PositionsMines(grilleRef, nbMines));
                    grilleJeu = ActualiserGrilleJeu(grilleJeu, grilleRef);
                    grilleJeu[ligne, colonne] = "T ";
                }
                //Récompense si le joueur jour en mode moyen
                else
                {
                    grilleJeu[ligne, colonne] = "T ";
                    Console.WriteLine("Vous avez trouvé un trésor ! Vous avez le droit à une récompense");
                    Console.WriteLine(AfficherRecompenseNiveauMoyen(grilleRef));
                }
                estDecouverte[ligne, colonne] = true;
            }
            //La case choisie n'est ni une mine ni un trésor et il n'y a pas de mine ou de trésor autour d'elle
            else if (estDecouverte[ligne, colonne] == false && grilleRef[ligne, colonne] == "  ")
            {
                grilleJeu[ligne, colonne] = "  ";
                coordonneesVoisines = ObtenirCoordonneesVoisines(dansUnCoin, surUnBord, grilleJeu, ligne, colonne);
                estDecouverte[ligne, colonne] = true;
                //Parcourt les voisins de la case choisie
                for (int i = 0; i < coordonneesVoisines.GetLength(0); i++)
                {
                    dansUnCoin = EtreDansUnCoin(grilleJeu, coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]);
                    surUnBord = EtreSurUnBord(grilleJeu, coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]);
                    //La case n'est ni une mine ni un trésor et il n'y a pas de mine ou de trésor autour d'elle
                    if (grilleRef[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]] == "  ")
                        DecouvrirCases(dansUnCoin, surUnBord, grilleRef, grilleJeu, estDecouverte, coordonneesVoisines[i, 0], coordonneesVoisines[i, 1], nbMines, choixNiveau);
                    else
                    {
                        //La case n'est ni une mine ni un trésor mais il y a une mine ou un trésor parmi ses voisins
                        if (grilleRef[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]] != "M " && grilleRef[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]] != "T ")
                            grilleJeu[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]] = grilleRef[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]];
                        //La case est une mine ou un trésor
                        else if (grilleRef[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]] == "T " || grilleRef[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]] == "M ")
                            grilleJeu[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]] = "? ";
                        estDecouverte[coordonneesVoisines[i, 0], coordonneesVoisines[i, 1]] = true;
                    }
                }
            }
            return grilleJeu;
        }



        //Affiche l'interface d'accueil contenant les règles du jeu
        static void AccueillirJoueur()
        {
            string bvn = File.ReadAllText("./assets/sprites/bvn.txt");
            string regles = File.ReadAllText("./assets/sprites/regles.txt");
            string bonnechance = File.ReadAllText("./assets/sprites/bonnechance.txt");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(bvn);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(regles);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(bonnechance);
            Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();


        }
        //Demande au joueur de choisir le niveau auquel il veut jouer et affiche le graphisme associé
        static int ObtenirChoixNiveau()
        {
            string niveau = File.ReadAllText("./assets/sprites/niveau.txt");
            Console.Write(niveau);
            //S'assure que la saisie pour le niveau est un chiffre parmis 1, 2 et 3
            string reponseJoueur = Console.ReadLine();
            int choixNiveau = Convert.ToInt32(GererExecptionFormat(reponseJoueur));
            while (choixNiveau != 1 && choixNiveau != 2 && choixNiveau != 3)
            {
                Console.Write("\nCe nombre ne conduit pas à une possibilité de jeu. Veuiller reproposer une réponse : ");
                reponseJoueur = Console.ReadLine();
                choixNiveau = Convert.ToInt32(GererExecptionFormat(reponseJoueur));
            }
            Console.Clear();
            return choixNiveau;
        }
        //Crée et remplie la grille de référence (mines, trésors, décompte) et la grille de jeu 
        static void InitialiserPartie(string[,] grilleRef, string[,] grilleJeu, bool[,] estDecouverte, int nbMines, int nbTresors, int[] coordonneesJoueur)
        {
            for (int i = 0; i < grilleJeu.GetLength(0); i++)
            {
                for (int j = 0; j < grilleJeu.GetLength(1); j++)
                {
                    grilleRef[i, j] = " ";
                    estDecouverte[i, j] = false;
                }
            }
            grilleRef = GenererMinesTresors(grilleRef, coordonneesJoueur, nbMines, nbTresors);
            grilleRef = AttribuerChiffresGrilleRef(grilleRef);
        }
        //Vérifie la position de la case, découvre ses voisins et affiche la grille de jeu 
        static void JouerTourDeJeu(bool dansUnCoin, bool surUnBord, bool partiePerdue, bool partieGagnee, int nbMines, int[] coordonneesJoueur, string[,] grilleRef, string[,] grilleJeu, bool[,] estDecouverte, int choixNiveau)
        {
            dansUnCoin = EtreDansUnCoin(grilleJeu, coordonneesJoueur[0], coordonneesJoueur[1]);
            surUnBord = EtreSurUnBord(grilleJeu, coordonneesJoueur[0], coordonneesJoueur[1]);
            DecouvrirCases(dansUnCoin, surUnBord, grilleRef, grilleJeu, estDecouverte, coordonneesJoueur[0], coordonneesJoueur[1], nbMines, choixNiveau);
            AfficherGrille(grilleJeu);
        }
        //Affiche une animation graphique qui évolue à chaque tour
        static void AffichageEntreTours(int tour)
        {
            string boutDeMeche = "=";
            string meche = "";

            for (int i = 1; i <= 10 - tour % 10; i++)
            {
                meche += boutDeMeche;
            }
            if (tour % 10 == 0)
            {
                string ExplosionEntreTours = File.ReadAllText("/assets/sprites/ExplosionEntreTours.txt");
                Console.WriteLine("\n\n" + ExplosionEntreTours + "\n\n");
            }
            else
            {
                Console.WriteLine("      ______________________________    ");
                Console.WriteLine(@"     /                            / \     \ \ / /");
                Console.WriteLine("    |                            | " + meche + "~");
                Console.Write(@"     \____________________________\_/     / / \ \");
                Console.WriteLine("\n");
            }


        }
        //Déclare la partie perdue quand le joueur découvre une mine 
        static bool Perdre(string[,] grilleRef, int ligne, int colonne, bool partiePerdue)
        {
            if (grilleRef[ligne, colonne] == "M ")
                partiePerdue = true;
            return partiePerdue;
        }
        //Déclare la partie gagnée s'il ne reste que des mines parmi les cases non découvertes
        static bool Gagner(string[,] grilleJeu, bool partieGagnee, string[,] grilleRef)
        {
            int cptNonDecouvert = 0;
            int nbMines = 0;
            for (int i = 0; i < grilleJeu.GetLength(0); i++)
            {
                for (int j = 0; j < grilleJeu.GetLength(1); j++)
                {
                    if (grilleJeu[i, j] == "? ")
                        cptNonDecouvert += 1;
                    if (grilleRef[i, j] == "M ")
                        nbMines += 1;
                }
            }
            if (cptNonDecouvert == nbMines)
                partieGagnee = true;
            return partieGagnee;
        }
        //Regroupe toutes les étapes d'une partie entière de jeu
        static void JouerPartieDeJeu(int choixNiveau)
        {
            //Démarre le chronomètre
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            //Déclaration des variables
            bool partiePerdue = false, partieGagnee = false, dansUnCoin = false, surUnBord = false;
            string[,] grilleJeu = InitialiserGrille(choixNiveau), grilleRef = new string[grilleJeu.GetLength(0), grilleJeu.GetLength(1)];
            bool[,] estDecouverte = new bool[grilleJeu.GetLength(0), grilleJeu.GetLength(1)];
            Random aleatoire = new Random();
            int nbMines = aleatoire.Next((grilleJeu.GetLength(0) / 2), (grilleJeu.GetLength(0) * grilleJeu.GetLength(1)) / 2 + 1), nbTresors = aleatoire.Next(1, 3), tempsImparti = 1;
            //Définie le temps imparti en mode difficile
            if (choixNiveau == 3)
                tempsImparti = ObtenirTempsImparti(grilleRef);

            AfficherGrille(grilleJeu);
            int[] coordonneesJoueur = RecupererChoixJoueur(grilleRef);
            InitialiserPartie(grilleRef, grilleJeu, estDecouverte, nbMines, nbTresors, coordonneesJoueur);
            int tour = 1;
            //Jouer tant que la partie n'est ni gagnée ni perdue
            while (partiePerdue == false && partieGagnee == false)
            {
                Console.Clear();
                AffichageEntreTours(tour);
                JouerTourDeJeu(dansUnCoin, surUnBord, partiePerdue, partieGagnee, nbMines, coordonneesJoueur, grilleRef, grilleJeu, estDecouverte, choixNiveau);
                AffichageEntreTours(tour);
                partiePerdue = Perdre(grilleRef, coordonneesJoueur[0], coordonneesJoueur[1], partiePerdue);
                partieGagnee = Gagner(grilleJeu, partieGagnee, grilleRef);
                //Cas de défaite
                if (partiePerdue)
                {
                    string defaite = File.ReadAllText("./assets/sprites/Defaite.txt");
                    Console.WriteLine(defaite);
                    string bombe = File.ReadAllText("./assets/sprites/Bombe.txt");
                    Console.WriteLine(bombe);
                    SoundPlayer laFinApproche = new SoundPlayer("./assets/sounds/SonBombe.Wav");
                    laFinApproche.Play();
                    Thread.Sleep(2000);
                    SoundPlayer mort = new SoundPlayer("./assets/sounds/LaMuerte.Wav");
                    mort.Play();
                    AffichageDefaite();
                }
                //Cas de victoire
                else if (partieGagnee)
                {

                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    string tempsEcoule = string.Format("{0:00} mn et {1:00} s", ts.Minutes, ts.Seconds);
                    if (choixNiveau != 3)
                        AfficherVictoire(tempsEcoule);
                    else
                    {
                        if (ts.Minutes >= tempsImparti)
                            AfficherFausseVictoire(tempsEcoule, tempsImparti);

                        else
                            AfficherVictoire(tempsEcoule);
                    }
                }
                else
                    coordonneesJoueur = RecupererChoixJoueur(grilleRef);
                tour += 1;
            }
        }




        //Affiche les graphismes et lance le son liées à la victoire
        static void AfficherVictoire(string tempsEcoule)
        {
            SoundPlayer bravo = new SoundPlayer("./assets/sounds/bravo.wav");
            bravo.Play();
            string money = File.ReadAllText("./assets/sprites/money.txt");
            string win = File.ReadAllText("./assets/sprites/win.txt");
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(win);
            Console.Write(tempsEcoule);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(money);
            Console.Write("\n");
            Console.ForegroundColor = ConsoleColor.White;




        }
        //Affiche les graphismes et lance le son liées à la défaite
        static void AffichageDefaite()
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Clear();
            Thread.Sleep(100);
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            Thread.Sleep(100);
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Clear();
            Thread.Sleep(100);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Thread.Sleep(100);
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            Thread.Sleep(100);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Thread.Sleep(100);


            Console.BackgroundColor = ConsoleColor.Gray;
            Console.Clear();
            Thread.Sleep(100);
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.Clear();
            Thread.Sleep(100);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Thread.Sleep(200);
            string explosion = File.ReadAllText("./assets/sprites/Explosion.txt");
            Console.WriteLine("{0}", explosion);
            Console.WriteLine("");
            string gameOver = File.ReadAllText("./assets/sprites/Game Over.txt");
            Console.WriteLine("{0}", gameOver);


        }
        //Affiche les graphismes et lance le son liées à la défaite par dépassement du temps imparti en mode difficile
        static void AfficherFausseVictoire(string tempsEcoule, int tempsImparti)
        {
            string faussealerte = File.ReadAllText("./assets/sprites/faussealerte.txt");
            Console.WriteLine("Bravo, vous avez découvert toutes les cases !\nMalheuresement votre temps est de {0} et  le temps imparti était de {1} min, alors mauvaise nouvelle ...", tempsEcoule, tempsImparti);
            Console.ReadLine();
            Console.Clear();
            SoundPlayer loose = new SoundPlayer("./assets/sounds/loose.wav");
            loose.Play();
            Console.Write(faussealerte);


        }



        //Programme principal
        static void Main(string[] args)
        {
            AccueillirJoueur();
            int choixNiveau = ObtenirChoixNiveau();
            ChoisirMusique(choixNiveau);
            JouerPartieDeJeu(choixNiveau);
            //Permet de rejouer ou quitter la console en fin de partie 
            Console.WriteLine("Voulez vous rejouer ? Si oui, appuyez sur la barre espace. Si non, appuyez sur Échap");
            ConsoleKeyInfo rejouerOuArreter;
            rejouerOuArreter = Console.ReadKey();
            while (rejouerOuArreter.Key != ConsoleKey.Escape)
            {
                //Relance une partie tant que le joueur choisie d'appuyer sur espace (et donc de rejouer)
                if (rejouerOuArreter.Key == ConsoleKey.Spacebar)
                {
                    Console.ResetColor();
                    Console.Clear();
                    choixNiveau = ObtenirChoixNiveau();
                    Console.Clear();
                    ChoisirMusique(choixNiveau);
                    JouerPartieDeJeu(choixNiveau);
                }
                //S'assure que le joueur appuie bien soit sur la barre d'espace soit sur echap
                while (rejouerOuArreter.Key != ConsoleKey.Spacebar && rejouerOuArreter.Key != ConsoleKey.Escape)
                {
                    Console.WriteLine("\n Vous n'avez pas entré une touche valide, veuillez recommencer svp \n");
                    Console.WriteLine("Voulez vous rejouer ? Si oui, appuyez sur la barre espace. Si non, appuyez sur Échap");
                    rejouerOuArreter = Console.ReadKey();

                }
                Console.WriteLine("Voulez vous rejouer ? Si oui, appuyez sur la barre espace. Si non, appuyez sur Échap");
                rejouerOuArreter = Console.ReadKey();
            }
        }
    }
}
