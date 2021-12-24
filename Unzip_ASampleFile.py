import zipfile
import pyzipper
import patoolib
from pyunpack import Archive

source_file = r'D:/Project/00be6858156b0be404b4fa4852ffc550c25565236beaa4cb13ffe288bcb48d8e.zip'
destination = r'D:/Project/ExtractedZip/'
password = 'infected'
open_file_mode = 'r'


def extract_zipfile():
    try:
        with zipfile.ZipFile(source_file, open_file_mode, password) as zip_file:
            zip_file.extractall(destination)
    except Exception as e:
        print(e)

def extract_pyzipper():
    try:
        with pyzipper.AESZipFile(source_file) as extracted_zip:
            extracted_zip.extractall(pwd=str.encode(password))
    except Exception as e:
        print(e)


def extract_patoo():
    try:
        patoolib.extract_archive(source_file, outdir=destination)
    except Exception as e:
        print(e)

def extract_pyunpack():
    try:
        Archive(source_file).extractall(destination)
    except Exception as e:
        print(e)

extract_zipfile()
extract_pyzipper()
extract_patoo()
extract_pyunpack()
