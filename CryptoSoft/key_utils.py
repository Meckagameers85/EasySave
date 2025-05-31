# file : key_utils.py

import os, json
import generate_rsa_keys

SETTINGS_FILE = "settings.json"

DEFAULT_PUBLIC_KEY = "public_key.pem"
DEFAULT_PRIVATE_KEY = "private_key.pem"

# Loads settings from a JSON file
def load_settings():
    if not os.path.exists(SETTINGS_FILE):
        return {}
    with open(SETTINGS_FILE, "r") as f:
        return json.load(f)

# Saves the given settings dictionary to the JSON file
def save_settings(settings):
    with open(SETTINGS_FILE, "w") as f:
        json.dump(settings, f, indent=4)

# Ensures that the RSA keys exist and updates the settings file if necessary
def ensure_keys_exist():
    settings = load_settings()

    # Default paths for keys
    public_key = settings.get("public_key_path") or DEFAULT_PUBLIC_KEY
    private_key = settings.get("private_key_path") or DEFAULT_PRIVATE_KEY

    # Update JSON if fields are missing or empty
    if not settings.get("public_key_path"):
        settings["public_key_path"] = public_key
    if not settings.get("private_key_path"):
        settings["private_key_path"] = private_key
    save_settings(settings)

    # Generate if files are missing
    if not os.path.exists(public_key) or not os.path.exists(private_key):
        # Create directories if needed
        os.makedirs(os.path.dirname(public_key) or ".", exist_ok=True)
        os.makedirs(os.path.dirname(private_key) or ".", exist_ok=True)

        print("Clés RSA manquantes. Génération en cours...")
        generate_rsa_keys.generate_rsa_keypair(public_key, private_key)
        print("Clés RSA générées.")
