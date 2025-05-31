# file : encrypt_file.py

from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.primitives.asymmetric import padding as asym_padding
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import padding as sym_padding
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives import keywrap
from cryptography.hazmat.primitives.kdf.hkdf import HKDF
import secrets, json

SETTINGS_FILE = "settings.json"

# Function to get the public key path from settings
def get_public_key_path():
    with open(SETTINGS_FILE, "r") as f:
        return json.load(f)["public_key_path"]

# Function to load the public key from a PEM file
def load_public_key(path):
    with open(path, "rb") as file:
        return serialization.load_pem_public_key(file.read())

# Function to encrypt a file using AES and RSA
def encrypt_file(file_path, public_key_path = None):
    if public_key_path is None:
        public_key_path = get_public_key_path()

    # Read the file to encrypt
    with open(file_path, "rb") as f:
        plaintext = f.read()

    # Generate a random AES key (32 bytes for AES-256)
    aes_key = secrets.token_bytes(32)
    iv = secrets.token_bytes(16)

    # Encrypt the data with AES
    padder = sym_padding.PKCS7(128).padder()
    padded_data = padder.update(plaintext) + padder.finalize()

    cipher = Cipher(algorithms.AES(aes_key), modes.CBC(iv), backend=default_backend())
    encryptor = cipher.encryptor()
    ciphertext = encryptor.update(padded_data) + encryptor.finalize()

    # Encrypt the AES key with RSA
    public_key = load_public_key(public_key_path)
    encrypted_key = public_key.encrypt(
        aes_key,
        asym_padding.OAEP(
            mgf=asym_padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )

    # Write the final result: [RSA_AES_KEY][IV][CIPHERTEXT]
    with open(file_path + ".enc", "wb") as out_file:
        out_file.write(encrypted_key + iv + ciphertext)
