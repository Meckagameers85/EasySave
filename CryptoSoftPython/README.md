# 🔐 CryptoSoft

CryptoSoft est un logiciel de chiffrement/déchiffrement de fichiers basé sur un système hybride RSA + AES. Il permet de protéger les fichiers sensibles dans un répertoire surveillé selon des extensions autorisées.

<br>

## 🧩 Objectif du projet

CryptoSoft automatise le chiffrement des fichiers dans un dossier spécifié. Il utilise :
- AES-256 pour chiffrer les fichiers
- RSA (2048 bits) pour chiffrer la clé AES

Les fichiers chiffrés ont l’extension ```.enc```. Les originaux sont supprimés après traitement (sauf en cas d'erreur).

<br>

## 📁 Structure des fichiers

Fichier                | Rôle
-----------------------|-----------------------------------------------------------------------
```cryptosoft.py```          | Point d'entrée principal. Lance le programme en mode CLI ou via une fonction ```main()``` pour une intégration GUI/programmée. Gère l'encodage et le décodage.
```encrypt_file.py```        | Chiffre un fichier avec AES, puis chiffre la clé AES avec RSA.
```decrypt_file.py```        | Déchiffre un fichier ```.enc``` en récupérant la clé AES via RSA, puis déchiffre le contenu.
```generate_rsa_keys.py```   | Génère une paire de clés RSA (publique/privée) et les sauvegarde dans des fichiers ```.pem```.
```key_utils.py```           | Gère le chargement, la sauvegarde des chemins de clés RSA et leur création si elles sont absentes.
```config_editor.py```       | Interface CLI simple pour modifier la configuration (```settings.json```) : extensions, dossier surveillé, chemins des clés.

<br>

## ⚙️ Fichiers générés

### ```settings.json```

Fichier de configuration du projet. Exemple :
```json
{
  "watch_directory": "./my_folder",
  "allowed_extensions": [".txt", ".pdf", ".*"],
  "public_key_path": "public_key.pem",
  "private_key_path": "private_key.pem"
}
```

- ```watch_directory``` : dossier à surveiller
- ```allowed_extensions``` : extensions autorisées à être chiffrées, "```.*```" pour travailler sur toutes les extensions
- ```public_key_path / private_key_path``` : chemins vers les clés RSA

<br>

### ```crypto.lock```

Fichier temporaire utilisé pour empêcher le lancement multiple de l'application (```mono-instance```). Supprimé automatiquement à la fermeture.

<br>

### ```public_key.pem / private_key.pem```

Fichiers de clés RSA au format PEM.

- ```public_key.pem``` : utilisée pour chiffrer la clé AES
- ```private_key.pem``` : utilisée pour la déchiffrer

<br>

## 📦 Génération d’un exécutable

Pour packager le projet en ```.exe```, nous utiliserons ***PyInstaller***.

Pour installer la bibliothèque, ouvrir le terminal de commandes, puis, exécuter la ligne suivante :

```pip install pyinstaller```

Une fois que la bibliothèque est présente, nous pouvons construire l'exécutable avec la commande suivante, dans le répertoire cible (dans notre cas, il nous faudra être dans le répertoire contenant le fichier ```cryptosoft.py```) :

```pyinstaller --onefile --console --noconfirm --name CryptoSoftCLI cryptosoft.py```

- ```--onefile``` : Compresser en un seul fichier
- ```--console``` : Activer le mode console et intéragir avec l'exécutable
- ```--noconfirm``` : Eviter les confirmation durant le build
- ```--name CryptoSoftCLI``` : Renommage de l'exécutable en CryptoSoftCLI dans notre cas
- ```cryptosoft.py``` : Fichier principal lancé avec l'exécutable

Assurez-vous que tous les modules sont présents et que les fichiers ```.py``` sont accessibles au moment de la compilation.

L'exécutable généré pourra alors fonctionner sur n'importe quelle machine, y compris celles n'ayant pas Python d'intaller.

<br>

## ▶️ Lancement

Il y a plusieurs possibilités pour lancer cet exécutable :

1. Dans le terminal de commandes, dans le bon répertoire, saisissez : ```cryptosoft.exe```
2. En double-cliquant sur le fichier comme un exécutable classique
3. En le lançant via un fichier batch automatiquement si souhaité
4. Autre selon l'imagination...

Une fois lancé, il nous est proposé d’ouvrir le configurateur (automatique si le fichier n'existe pas au même niveau).  
Puis, nous avons le choix entre :

- ```(E)```ncoder les fichiers du dossier surveillé
- ```(D)```écoder les fichiers ```.enc``` présents dans ce même dossier surveillé

Les fichiers nécessaires comme ```settings.json```, ```public_key.pem```, ```private_key.pem``` ou encore ```crypto.lock``` sont générés aautomatiquement lors de l'exécution s'il ne sont pas déjà présents.

A noter que les chemins pour les clés RSA peuvent être changés et déplacés dans un dossier autre (chacun ou non) pour plus de sécurité.