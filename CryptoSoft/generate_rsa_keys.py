# File : generate_rsa_keys.py

import os, json
from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.hazmat.primitives import serialization

SETTINGS_FILE = "settings.json"
DEFAULT_PUBLIC_KEY = "public_key.pem"
DEFAULT_PRIVATE_KEY = "private_key.pem"

# Loads settings from a JSON file
def load_settings():
    if not os.path.exists(SETTINGS_FILE):
        return {
            "public_key_path": DEFAULT_PUBLIC_KEY,
            "private_key_path": DEFAULT_PRIVATE_KEY
        }
    with open(SETTINGS_FILE, "r") as f:
        settings = json.load(f)
    
    # Default values if keys are missing
    settings.setdefault("public_key_path", DEFAULT_PUBLIC_KEY)
    settings.setdefault("private_key_path", DEFAULT_PRIVATE_KEY)
    return settings

# Saves the given settings dictionary to the JSON file
def save_settings(settings):
    with open(SETTINGS_FILE, "w") as f:
        json.dump(settings, f, indent=4)

# Generates an RSA key pair and saves them if they don't already exist
def generate_rsa_keypair(public_key_path, private_key_path):
    # Check if keys already exist
    if os.path.exists(public_key_path) and os.path.exists(private_key_path):
        print(f"Clés déjà présentes :\n- {public_key_path}\n- {private_key_path}")
        return

    # Generate RSA key pair
    private_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)

    # Save private key
    with open(private_key_path, "wb") as f:
        f.write(private_key.private_bytes(
            encoding=serialization.Encoding.PEM,
            format=serialization.PrivateFormat.TraditionalOpenSSL,
            encryption_algorithm=serialization.NoEncryption()
        ))

    # Save public key
    public_key = private_key.public_key()
    with open(public_key_path, "wb") as f:
        f.write(public_key.public_bytes(
            encoding=serialization.Encoding.PEM,
            format=serialization.PublicFormat.SubjectPublicKeyInfo
        ))

def main():
    settings = load_settings()
    generate_rsa_keypair(
        settings["public_key_path"],
        settings["private_key_path"]
    )

if __name__ == "__main__":
    main()
