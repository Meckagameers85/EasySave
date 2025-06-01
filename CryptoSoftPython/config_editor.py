# file : config_editor.py

import json, os
import key_utils

SETTINGS_FILE = "settings.json"

# Ensure the settings file exists with default values if it doesn't
def load_settings():
    if not os.path.exists(SETTINGS_FILE):
        return {"watch_directory": ".", "allowed_extensions": [], "public_key_path": "public_key.pem", "private_key_path": "private_key.pem"}
    with open(SETTINGS_FILE, "r") as f:
        settings = json.load(f)

        # Ensure default values for keys
        if "public_key_path" not in settings:
            settings["public_key_path"] = "public_key.pem"
        if "private_key_path" not in settings:
            settings["private_key_path"] = "private_key.pem"

        return settings

# Saves the given settings dictionary to the JSON file
def save_settings(settings):
    with open(SETTINGS_FILE, "w") as f:
        json.dump(settings, f, indent=4)

# Get and set functions for settings
def get_settings(path=SETTINGS_FILE):
    return load_settings()

# Set new settings and save them to the file
def set_settings(new_settings, path=SETTINGS_FILE):
    save_settings(new_settings)

# Display the menu
def display_menu(settings):
    print("\nCONFIGURATION")
    print(f"Dossier surveillé : {settings['watch_directory']}")
    print(f"Extensions autorisées : {', '.join(settings['allowed_extensions'])}")
    print(f"Chemin clé publique : {settings['public_key_path']}")
    print(f"Chemin clé privée : {settings['private_key_path']}")
    print("\nOptions :")
    print("1. Modifier le dossier")
    print("2. Ajouter une extension")
    print("3. Supprimer une extension")
    print("4. Modifier chemin clé publique")
    print("5. Modifier chemin clé privée")
    print("6. Sauvegarder et quitter")
    print("7. Quitter sans sauvegarder")

# Function to display the menu and handle user input
def menu():
    settings = load_settings()
    key_utils.ensure_keys_exist()
    while True:
        display_menu(settings)
        choice = input("Choix : ")

        if choice == "1":
            folder = input("Nouveau dossier à surveiller : ").strip()
            if os.path.isdir(folder):
                settings["watch_directory"] = folder
            else:
                print("Ce dossier n'existe pas.")
        elif choice == "2":
            ext = input("Nouvelle extension (ex: .pdf) : ").strip().lower()
            if ext.startswith(".") and ext not in settings["allowed_extensions"]:
                settings["allowed_extensions"].append(ext)
        elif choice == "3":
            ext = input("Extension à retirer : ").strip().lower()
            if ext in settings["allowed_extensions"]:
                settings["allowed_extensions"].remove(ext)
        elif choice == "4":
            new_pub = input("Nouveau chemin pour la clé publique : ").strip()
            if new_pub:
                settings["public_key_path"] = new_pub
        elif choice == "5":
            new_priv = input("Nouveau chemin pour la clé privée : ").strip()
            if new_priv:
                settings["private_key_path"] = new_priv
        elif choice == "6":
            save_settings(settings)
            print("Config sauvegardée.")
            break
        elif choice == "7":
            print("Changements ignorés.")
            break
        else:
            print("Choix invalide.")

if __name__ == "__main__":
    menu()
