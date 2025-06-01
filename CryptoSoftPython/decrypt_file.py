# file : decrypt_file.py

import os, json
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.primitives.asymmetric import padding as asym_padding
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import padding as sym_padding
from cryptography.hazmat.backends import default_backend

SETTINGS_FILE = "settings.json"

# Function to get the private key path from settings
def get_private_key_path():
    with open(SETTINGS_FILE, "r") as f:
        return json.load(f)["private_key_path"]

# Function to load the private key from a PEM file
def load_private_key(path):
    with open(path, "rb") as file:
        return serialization.load_pem_private_key(file.read(), password=None)

# Function to decrypt a file using AES and RSA
def decrypt_file(file_path, private_key_path = None, output_path=None):
    if private_key_path is None:
        private_key_path = get_private_key_path()

    with open(file_path, "rb") as f:
        content = f.read()

    # Decompose : [RSA_AES_KEY (256 bytes)] + [IV (16 bytes)] + [ciphertext]
    encrypted_key = content[:256]
    iv = content[256:272]
    ciphertext = content[272:]

    # Decrypt the AES key
    private_key = load_private_key(private_key_path)
    aes_key = private_key.decrypt(
        encrypted_key,
        asym_padding.OAEP(
            mgf=asym_padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )

    # Decrypt the content
    cipher = Cipher(algorithms.AES(aes_key), modes.CBC(iv), backend=default_backend())
    decryptor = cipher.decryptor()
    padded_plaintext = decryptor.update(ciphertext) + decryptor.finalize()

    # Remove the padding
    unpadder = sym_padding.PKCS7(128).unpadder()
    plaintext = unpadder.update(padded_plaintext) + unpadder.finalize()

    # Generate a default output name
    if not output_path:
        base, ext = os.path.splitext(file_path)
        if ext == ".enc":
            original_base, original_ext = os.path.splitext(base)
            output_path = f"{original_base}{original_ext}"
        else:
            output_path = f"{file_path}"

    with open(output_path, "wb") as out_file:
        out_file.write(plaintext)

    return output_path
