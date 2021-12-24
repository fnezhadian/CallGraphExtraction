import os
import subprocess
import zipfile

samples_path = "D:\\Sample"
ida_path = "C:\\Program Files\\IDA Pro 7.5 SP3"
script_file_path = "C:\\Project\\CreateGDL.py"
OutputPath = "D:\\Sample\\Output"
searchPattern = "*.zip"
password = "infected"
runtime_files_extensions: list[str] = ["", "id0", "id1", "id2", "nam", "til"]


def get_subdirectories(main_dir):
    sub_dir_list = []
    for sub_dir in os.listdir(main_dir):
        sub_dir_path = os.path.join(main_dir, sub_dir)
        if os.path.isdir(sub_dir_path):
            sub_dir_new_path = sub_dir_path.replace(" ", "")
            os.rename(sub_dir_path, sub_dir_new_path)
            sub_dir_list.append(sub_dir_new_path)
    return sub_dir_list


#def extract_zipfile(source_file, destination):
#    try:
#        with zipfile.ZipFile(source_file, 'r', password) as zip_file:
#            zip_file.extractall(destination)
#    except Exception as e:
#        print(e)


def get_files(directory):
    target_files = []
    for filename in os.listdir(directory):
        full_path = os.path.join(directory, filename)
        if not os.path.isfile(full_path):
            continue
        target_files.append(full_path)
    return target_files


def run_ida_process(file_path):

    try:
        command = 'cmd /c idat64 -a -A -S{0} {1}'.format(script_file_path, file_path)
        process = subprocess.Popen(command, cwd=ida_path)
        process.wait()
        delete_runtime_files(file_path)
    except:
        pass


def delete_runtime_files(file_path):

    try:
        file_name = os.path.basename(file_path)
        pure_file_name = os.path.splitext(file_name)[0]
        file_location = os.path.dirname(os.path.abspath(file_path))
        for file_extension in runtime_files_extensions:
            os.remove(os.path.join(file_location, "{}.{}".format(pure_file_name, file_extension)))
    except:
        pass


def main():

    sub_dir_list = get_subdirectories(samples_path)

    for sub_dir in sub_dir_list:
        target_files = get_files(sub_dir)
        for file_path in target_files:
            #TODO: extract_zipfile(file_path, OutputPath)
            run_ida_process(file_path)


if __name__ == "__main__":
    main()

