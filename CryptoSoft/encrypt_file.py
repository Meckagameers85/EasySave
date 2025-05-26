import os
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.primitives.asymmetric import padding as asym_padding
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import padding as sym_padding
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives import keywrap
from cryptography.hazmat.primitives.kdf.hkdf import HKDF
import secrets

def load_public_key(path):
    with open(path, "rb") as file:
        return serialization.load_pem_public_key(file.read())

def encrypt_file(file_path, public_key_path):
    # Lire le fichier d'entrée
    with open(file_path, "rb") as f:
        plaintext = f.read()

    # Générer une clé AES aléatoire (32 octets pour AES-256)
    aes_key = secrets.token_bytes(32)
    iv = secrets.token_bytes(16)

    # Chiffrer les données avec AES
    padder = sym_padding.PKCS7(128).padder()
    padded_data = padder.update(plaintext) + padder.finalize()

    cipher = Cipher(algorithms.AES(aes_key), modes.CBC(iv), backend=default_backend())
    encryptor = cipher.encryptor()
    ciphertext = encryptor.update(padded_data) + encryptor.finalize()

    # Chiffrer la clé AES avec RSA
    public_key = load_public_key(public_key_path)
    encrypted_key = public_key.encrypt(
        aes_key,
        asym_padding.OAEP(
            mgf=asym_padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )

    # Écrire le résultat final : [RSA_AES_KEY][IV][CIPHERTEXT]
    with open(file_path + ".enc", "wb") as out_file:
        out_file.write(encrypted_key + iv + ciphertext)

encrypt_file("test.txt", "public_key.pem")
encrypt_file("config.json", "public_key.pem")
encrypt_file("7-Boucle PBL.pdf", "public_key.pem")
encrypt_file("Querrec Thomas - CER prosit 7.pdf", "public_key.pem")
encrypt_file("Querrec Thomas - CER prosit 7.docx", "public_key.pem")