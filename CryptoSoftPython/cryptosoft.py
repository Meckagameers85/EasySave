# file : cryptosoft.py

import os, json, sys, msvcrt
import encrypt_file, decrypt_file, key_utils

LOCK_FILE = "crypto.lock"
SETTINGS_FILE = "settings.json"

# Function to get the public and private key paths from settings
def get_key_paths():
    with open(SETTINGS_FILE, "r") as f:
        settings = json.load(f)
        return settings["public_key_path"], settings["private_key_path"]

# Function to check if another instance is running
def check_mono_instance():
    try:
        lock_fd = open(LOCK_FILE, 'w')
        msvcrt.locking(lock_fd.fileno(), msvcrt.LK_NBLCK, 1)
        return lock_fd
    except OSError:
        print("Une autre instance est déjà en cours.")
        sys.exit(1)

# Function to load settings from the JSON file
def load_settings():
    with open(SETTINGS_FILE, "r") as f:
        return json.load(f)

# Function to get files to encrypt based on allowed extensions
def get_files_to_encrypt(folder, allowed_extensions):
    allow_all = ".*" in allowed_extensions
    files = []
    for root, _, filenames in os.walk(folder):
        for name in filenames:
            if name.endswith(".enc"):
                continue
            _, ext = os.path.splitext(name)
            if allow_all or ext.lower() in allowed_extensions:
                files.append(os.path.join(root, name))
    return files

# Main CLI function
def main_cli():
    # Mono-instance
    lock_fd = check_mono_instance()

    open_settings = "n"

    # Check if settings file exists
    if not os.path.exists(SETTINGS_FILE):
        print("Aucune configuration trouvée. Veuillez configurer d'abord.\nLancement du configurateur...")
        import config_editor
        config_editor.menu()

    # Ask to open settings if not configured
    else:
       open_settings = input("Voulez-vous ouvrir le configurateur ? (O/N) : ").strip().lower()
       if open_settings == "o":
           import config_editor
           config_editor.menu()

    # Load settings
    settings = load_settings()
    folder = settings.get("watch_directory")
    extensions = [ext.lower() for ext in settings.get("allowed_extensions", [])]
    allow_all = ".*" in extensions

    key_utils.ensure_keys_exist()

    # Check command line arguments
    if len(sys.argv) >= 2:
        mode = sys.argv[1].lower()
    else:
        mode = input("Souhaitez-vous (E)ncoder ou (D)ecoder ? ").strip().lower()
        if mode.startswith("e"):
            mode = "encode"
        elif mode.startswith("d"):
            mode = "decode"
        else:
            print("Option invalide.")
            return

    if mode == "encode":
        files = get_files_to_encrypt(folder, extensions)

        if not files:
            print("Aucun fichier à chiffrer.")
            return
        for file_path in files:
            try:
                public_key_path, _ = get_key_paths()
                encrypt_file.encrypt_file(file_path, public_key_path)
                os.remove(file_path)
                print(f"Chiffré : {file_path}")
            except Exception as e:
                print(f"Erreur sur {file_path} : {e}")             

    elif mode == "decode":
        encrypted_files = []
        for root, _, filenames in os.walk(folder):
            for name in filenames:
                if not name.endswith(".enc"):
                    continue
                original_ext = os.path.splitext(os.path.splitext(name)[0])[1].lower()
                if allow_all or original_ext in extensions:
                    encrypted_files.append(os.path.join(root, name))

        if not encrypted_files:
            print("Aucun fichier à déchiffrer.")
            return
        for file_name in encrypted_files:
            try:
                _, private_key_path = get_key_paths()
                file_path = os.path.join(folder, file_name)
                decrypt_file.decrypt_file(file_path, private_key_path)
                os.remove(file_path)
                print(f"Déchiffré : {file_path}")
            except Exception as e:
                print(f"Erreur sur {file_path} : {e}")

    else:
        print("Mode non reconnu. Utilisez 'encode' ou 'decode'.")

    lock_fd.close()
    input("Appuyez sur \"entrée\" pour sortir...")

# Main function 
def main(config_path=SETTINGS_FILE, mode=None, folder_override=None):
    # Function without print/input for program or GUI use
    import config_editor
    settings = config_editor.load_settings(config_path)
    folder = folder_override or settings["watch_directory"]
    extensions = [e.lower() for e in settings["allowed_extensions"]]
    allow_all = ".*" in extensions

    key_utils.ensure_keys_exist()

    files = get_files_to_encrypt(folder, extensions)

    results = []

    if mode == "encode":
        files = get_files_to_encrypt(folder, extensions)

        if not files:
            return "Aucun fichier à chiffrer."
        for file_path in files:
            try:
                public_key_path, _ = get_key_paths()
                encrypt_file.encrypt_file(file_path, public_key_path)
                os.remove(file_path)
                results.append((file_path, "OK"))
            except Exception as e:
                results.append((file_path, f"Erreur : {e}"))

    elif mode == "decode":
        encrypted_files = []
        for root, _, filenames in os.walk(folder):
            for name in filenames:
                if not name.endswith(".enc"):
                    continue
                original_ext = os.path.splitext(os.path.splitext(name)[0])[1].lower()
                if allow_all or original_ext in extensions:
                    encrypted_files.append(os.path.join(root, name))

        if not encrypted_files:
            return "Aucun fichier à déchiffrer."
        for file_name in encrypted_files:
            try:
                _, private_key_path = get_key_paths()
                file_path = os.path.join(folder, file_name)
                decrypt_file.decrypt_file(file_path, private_key_path)
                os.remove(file_path)
                results.append((file_path, "OK"))
            except Exception as e:
                results.append((file_path, f"Erreur : {e}"))

    else:
        return "Mode non reconnu. Utilisez 'encode' ou 'decode'."

    return results

if __name__ == "__main__":
    main_cli()
