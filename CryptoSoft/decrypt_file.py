# from cryptography.hazmat.primitives import hashes
# from cryptography.hazmat.primitives.asymmetric import padding as asym_padding
# from cryptography.hazmat.primitives import serialization

# def load_private_key(file_path):
#     with open(file_path, "rb") as key_file:
#         return serialization.load_pem_private_key(key_file.read(), password=None)

# def decrypt_file(file_path, private_key_path, output_path=None):
#     private_key = load_private_key(private_key_path)

#     # Lire les données chiffrées
#     with open(file_path, "rb") as encrypted_file:
#         encrypted_data = encrypted_file.read()

#     # Déchiffrer les données
#     decrypted_data = private_key.decrypt(
#         encrypted_data,
#         asym_padding.OAEP(
#             mgf=asym_padding.MGF1(algorithm=hashes.SHA256()),
#             algorithm=hashes.SHA256(),
#             label=None
#         )
#     )

#     # Écrire les données déchiffrées
#     output_file = output_path if output_path else file_path.replace(".enc", "")
#     with open(output_file, "wb") as file:
#         file.write(decrypted_data)

#     print(decrypted_data)

# decrypt_file("test.txt.enc", "private_key.pem")
# decrypt_file("config.json.enc", "private_key.pem")

import os
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.hazmat.primitives.asymmetric import padding as asym_padding
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import padding as sym_padding
from cryptography.hazmat.backends import default_backend

def load_private_key(path):
    with open(path, "rb") as file:
        return serialization.load_pem_private_key(file.read(), password=None)

def decrypt_file(file_path, private_key_path, output_path=None):
    with open(file_path, "rb") as f:
        content = f.read()

    # Décomposer : [RSA_AES_KEY (256 bytes)] + [IV (16 bytes)] + [ciphertext]
    encrypted_key = content[:256]
    iv = content[256:272]
    ciphertext = content[272:]

    # Déchiffrer la clé AES
    private_key = load_private_key(private_key_path)
    aes_key = private_key.decrypt(
        encrypted_key,
        asym_padding.OAEP(
            mgf=asym_padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )

    # Déchiffrer le contenu
    cipher = Cipher(algorithms.AES(aes_key), modes.CBC(iv), backend=default_backend())
    decryptor = cipher.decryptor()
    padded_plaintext = decryptor.update(ciphertext) + decryptor.finalize()

    # Supprimer le padding
    unpadder = sym_padding.PKCS7(128).unpadder()
    plaintext = unpadder.update(padded_plaintext) + unpadder.finalize()

    # Générer un nom de sortie par défaut
    if not output_path:
        base, ext = os.path.splitext(file_path)
        if ext == ".enc":
            original_base, original_ext = os.path.splitext(base)
            output_path = f"{original_base}_decrypted{original_ext}"
        else:
            output_path = f"{file_path}_decrypted"

    with open(output_path, "wb") as out_file:
        out_file.write(plaintext)

    return output_path

decrypt_file("test.txt.enc", "private_key.pem")
decrypt_file("config.json.enc", "private_key.pem")
decrypt_file("7-Boucle PBL.pdf.enc", "private_key.pem")
decrypt_file("Querrec Thomas - CER prosit 7.pdf.enc", "private_key.pem")
decrypt_file("Querrec Thomas - CER prosit 7.docx.enc", "private_key.pem")