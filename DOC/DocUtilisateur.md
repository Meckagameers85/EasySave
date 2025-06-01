# EasySave - Guide Utilisateur

## Table des matières
1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Démarrage rapide](#démarrage-rapide)
4. [Interface utilisateur](#interface-utilisateur)
5. [Création d'un travail de sauvegarde](#création-dun-travail-de-sauvegarde)
6. [Types de sauvegarde](#types-de-sauvegarde)
7. [Exécution des sauvegardes](#exécution-des-sauvegardes)
8. [Configuration avancée](#configuration-avancée)
9. [Monitoring et logs](#monitoring-et-logs)
10. [Dépannage](#dépannage)

## Introduction

EasySave est une application de sauvegarde professionnelle qui permet de créer et gérer des travaux de sauvegarde automatisés. L'application prend en charge :

- **Nombre illimité** de travaux de sauvegarde
- **Sauvegardes complètes et différentielles**
- **Interface graphique intuitive** (WPF)
- **Exécution en parallèle** des travaux de sauvegarde
- **Contrôles temps réel** (Play/Pause/Stop)
- **Gestion des fichiers prioritaires**
- **Optimisation de la bande passante**
- **Console déportée** avec communication par sockets
- **Cryptage automatique** avec CryptoSoft (mono-instance)
- **Pause automatique** lors de la détection d'un logiciel métier
- **Monitoring en temps réel** avec barres de progression
- **Logs détaillés** (JSON/XML)
- **Support multilingue** (Français/Anglais)

## Installation

### Prérequis système
- Windows 10 ou supérieur
- .NET Core Runtime
- Droits d'administrateur (pour l'installation)

### Procédure d'installation
1. Téléchargez le package d'installation EasySave
2. Exécutez le fichier d'installation en tant qu'administrateur
3. Suivez les instructions de l'assistant d'installation
4. Redémarrez votre ordinateur si nécessaire

## Démarrage rapide

### Premier lancement
1. Lancez EasySave depuis le menu Démarrer
2. Sélectionnez votre langue (Français/Anglais)
3. L'interface principale s'ouvre avec la liste des travaux de sauvegarde

### Créer votre première sauvegarde
1. Cliquez sur **"Ajouter un travail"**
2. Saisissez un nom pour votre sauvegarde
3. Sélectionnez le **répertoire source**
4. Sélectionnez le **répertoire de destination**
5. Choisissez le **type de sauvegarde** (Complète/Différentielle)
6. Cochez ou non **Chiffrer la sauvegarde**
7. Cliquez sur **"Créer"**

## Interface utilisateur

### Fenêtre principale
L'interface principale EasySave se compose de :

- **Titre de l'application** : "EasySave" centré en haut de la fenêtre
- **Boutons Settings** : Changement de la langue et/ou du format, logiciel métier
- **Boutons de chiffrement** : Choix du type de chiffrement et de l'extension 
- **Barre d'outils principale** :
  - **Select All** : Sélectionner tous les travaux
  - **Delete** : Supprimer le ou les travaux sélectionnés
  - **Execute** : Exécuter le ou les travaux
  - **New** : Créer un nouveau travail de sauvegarde

### Liste des travaux de sauvegarde
Chaque travail est affiché dans une carte qui contient :
- **Nom du travail** (ex: "Test")
- **Répertoire source** (ex: "Source: D:\")
- **Répertoire cible** (ex: "Target: D:\CD")
- **Type de sauvegarde** ("Full" ou "Differential")
- **État du cryptage** ("None" ou nom de l'algorithme)
- **Boutons de contrôle** (Play/Pause/Stop/Edit) 
- **Icône de suppression** (🗑️)

## Création d'un travail de sauvegarde

### Boîte de dialogue "New Backup"
Pour créer un nouveau travail, cliquez sur **"New"** puis remplissez :

- **Name** : Nom unique pour identifier votre travail de sauvegarde
- **Source Path** : Répertoire à sauvegarder (avec bouton 📁 pour naviguer)
- **Target Path** : Répertoire de destination (avec bouton 📁 pour naviguer)
- **Type** : Menu déroulant avec les options :
  - **Full** : Sauvegarde complète
  - **Differential** : Sauvegarde différentielle
- **☑️ Encrypt Backup** : Case à cocher pour activer le cryptage

### Boutons d'action
- **Create** : Valider et créer le travail
- **Cancel** : Annuler la création

### Types de répertoires supportés
- **Disques locaux** : C:\, D:\, etc.
- **Disques externes** : Clés USB, disques durs externes
- **Lecteurs réseau** : Partages réseau (UNC : \\serveur\partage)

### Exemple de configuration
```
Name: Backup_Documents
Source Path: C:\Users\MonNom\Documents
Target Path: \\NAS\Backup\Documents
Type: Differential
☑️ Encrypt Backup: Activé
```

## Types de sauvegarde

### Sauvegarde complète
- **Principe** : Copie tous les fichiers à chaque exécution
- **Avantages** : Restauration simple et rapide
- **Inconvénients** : Plus lente, consomme plus d'espace
- **Usage recommandé** : Première sauvegarde, sauvegardes critiques

### Sauvegarde différentielle
- **Principe** : Copie uniquement les fichiers modifiés depuis la dernière sauvegarde complète
- **Avantages** : Plus rapide, économise l'espace
- **Inconvénients** : Restauration plus complexe
- **Usage recommandé** : Sauvegardes régulières, gros volumes de données

## Exécution des sauvegardes

### Exécution en parallèle
Les travaux de sauvegarde s'exécutent **en parallèle**.

#### Contrôles individuels
Pour chaque travail, vous disposez de quatre boutons :
- **▶️ Play** : Démarrer ou reprendre le travail
- **⏸️ Pause** : Mettre en pause (pause effective après le transfert du fichier en cours)
- **⏹️ Stop** : Arrêt immédiat du travail et de la tâche en cours
- **✏️Edit** : Modification du travail (il ne doit pas être en cours)

### Gestion de la bande passante
- **Limitation automatique** : Interdiction de transférer simultanément plusieurs fichiers volumineux
- **Seuil configurable** : Taille limite paramétrable (n Ko)
- **Règle** : Pendant le transfert d'un gros fichier, les autres travaux peuvent transférer des petits fichiers

### Surveillance temps réel
- **Barre de progression** : Pourcentage d'avancement pour chaque travail
- **Mise à jour automatique** : Rafraîchissement en temps réel de l'état
- **Informations détaillées** : Fichier en cours, vitesse, temps restant

## Configuration avancée

### Paramètres généraux (Settings)
Accédez aux paramètres via le bouton **"S"** en haut à droite de l'interface principale.

#### Fenêtre Settings
- **Language** : Menu déroulant pour choisir la langue (English/Français)
- **Format** : Format des logs (JSON/XML)
- **Business Software** : Pause temporaire si détection du fonctionnement d'un logiciel métier
- **Save** : Sauvegarder les modifications
- **Reset All** : Restaurer les paramètres par défaut

#### Configuration CryptoSoft
Fenêtre séparée **"CryptoSoft Settings"** :
- **Type** : Menu déroulant pour sélectionner l'algorithme de cryptage
- **Extensions** : Liste des extensions de fichiers à crypter (ex: .pdf, .docx, .txt)
- **Save** : Sauvegarder la configuration
- **Reset All** : Restaurer les paramètres par défaut

## Monitoring et logs

### Fichier d'état temps réel 
Le fichier `state.json` contient des informations étendues :
```json
{
  "Name": "Backup_Documents",
  "Timestamp": "2025-05-31T14:30:00",
  "State": "Active",
  "TotalFiles": 1250,
  "FilesToTransfer": 45,
  "SizeToTransfer": 1048576,  
  "Progression": 96.4,
  "CurrentSourceFile": "C:\\Documents\\rapport.pdf",
  "CurrentTargetFile": "D:\\Backup\\rapport.pdf",
  "TransferSpeed": 2048576,
  "Priority": "High",
  "ParallelTransfers": 3,
  "EncryptionQueue": 2
}
```

### Fichier de log journalier 
Le fichier `2025-05-31.json` enregistre chaque action avec informations étendues :
```json
{
  "Timestamp": "2025-05-31T14:30:15",
  "Name": "Backup_Documents",
  "SourceFilePath": "C:\\Documents\\rapport.pdf",
  "TargetFilePath": "D:\\Backup\\rapport.pdf",
  "FileSize": 2048576,
  "TransferTime": 1250,
  "EncryptionTime": 300,
  "Priority": "High",
  "ParallelJob": 2,
  "NetworkLoad": 45.2,
  "BusinessSoftwareDetected": false
}
```

### Surveillance en temps réel 
- **Progression parallèle** : Barres de progression pour chaque travail actif simultanément
- **Vitesse de transfert** : Affichage en temps réel par travail
- **Fichiers en cours** : Liste des fichiers actuellement traités en parallèle
- **Gestion des priorités** : Indicateur visuel des fichiers prioritaires
- **Charge réseau** : Monitoring de la bande passante utilisée
- **État CryptoSoft** : File d'attente des fichiers à crypter
- **Détection logiciel métier** : Alerte visuelle et pause automatique

## Dépannage

### Problèmes courants

#### La sauvegarde ne démarre pas
- ✅ Vérifiez que le répertoire source existe et est accessible
- ✅ Vérifiez les droits d'accès aux répertoires source et destination  
- ✅ Assurez-vous qu'aucun logiciel métier n'est en cours d'exécution
- ✅ Contrôlez que CryptoSoft n'est pas déjà utilisé par un autre processus

#### Sauvegardes en pause automatique 
- ✅ Vérifiez si un logiciel métier est détecté (ex: calculatrice.exe)
- ✅ Fermez le logiciel métier pour reprendre automatiquement les sauvegardes
- ✅ Vérifiez la configuration des processus surveillés dans les paramètres

#### Problèmes de performance 
- ✅ Vérifiez la charge réseau si les transferts sont lents
- ✅ Ajustez le seuil de taille des gros fichiers dans les paramètres
- ✅ Réduisez le nombre de travaux parallèles si nécessaire
- ✅ Vérifiez que les fichiers prioritaires ne bloquent pas les autres transferts

#### Erreurs de cryptage
- ✅ Vérifiez que CryptoSoft est correctement installé
- ✅ Assurez-vous qu'une seule instance de CryptoSoft fonctionne (mono-instance)
- ✅ Contrôlez les permissions sur les fichiers à crypter
- ✅ Vérifiez l'espace disque disponible

#### Erreur d'accès réseau
- ✅ Vérifiez la connexion réseau
- ✅ Contrôlez les identifiants d'accès aux partages
- ✅ Testez l'accès manuel au répertoire réseau
- ✅ Vérifiez que les chemins UNC sont correctement formatés

### Messages d'erreur

#### Codes de temps de transfert
- **Temps positif** : Transfert réussi (durée en ms)
- **Temps négatif** : Erreur lors de la copie (code d'erreur)

#### Codes de temps de cryptage  
- **0** : Pas de cryptage demandé
- **Temps positif** : Cryptage réussi (durée en ms)
- **Temps négatif** : Erreur lors du cryptage (code d'erreur)

#### États spéciaux (Version 3.0)
- **"Paused by Business Software"** : Pause automatique due à la détection d'un logiciel métier
- **"Waiting for Priority Files"** : En attente de traitement des fichiers prioritaires
- **"Network Throttling"** : Limitation due à la charge réseau élevée
- **"CryptoSoft Queue"** : En attente de disponibilité de CryptoSoft

---

*EasySave - Version 3.0 - Documentation utilisateur*  
*Dernière mise à jour : Juin 2025*
