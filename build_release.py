import argparse
import json
import os
import shutil
import subprocess
import tempfile
from pathlib import Path
from zipfile import ZipFile


def dotnet_clean():
    result = subprocess.run(["dotnet", "clean"])
    return result.returncode == 0


def dotnet_build(configuration):
    result = subprocess.run(["dotnet", "build", "-c", configuration])
    return result.returncode == 0


def create_synthmod(mod_name, version_str, input_dir, input_subpaths, output_dir):
    mod_name = str(mod_name)
    version_str = str(version_str)
    print(f"{mod_name} version {version_str}")

    tmpdir = tempfile.mkdtemp()
    print(f"tmp directory is {tmpdir}")

    print("Creating LocalItem.json")
    local_item_contents = {
        "hash": mod_name
    }
    local_item_json = json.dumps(local_item_contents, indent=2)
    local_item_path = Path(tmpdir, "LocalItem.json")
    with open(local_item_path, "w") as local_item_file:
        local_item_file.write(local_item_json)
    
    print("Setting up output dir")
    # root_output_dir = Path("..", "Downloads")
    tmp_outputdir = Path(tmpdir, mod_name)
    tmp_outputdir.mkdir(parents=True, exist_ok=True)

    output_name = f"{mod_name}_{version_str}"
    output_name_synthmod = f"{output_name}.synthmod"
    output_path = Path(tmp_outputdir, output_name_synthmod)
    print(f"Creating zipped synthmod file {output_path}")
    with ZipFile(output_path, "w") as zip_file:
        # print(f"Adding {local_item_path} to LocalItem.json")
        zip_file.write(local_item_path, "LocalItem.json")

        for input_file in input_subpaths:
            real_path = Path(input_dir, input_file)
            zip_path = Path("Mods", input_file)
            # print(f"Adding {real_path} to {zip_path}")
            zip_file.write(real_path, zip_path)

    if not output_path.exists():
        print(f"Failed to create zip file {output_path}")
        return False
    
    final_path = Path(output_dir, output_name, output_name_synthmod)
    final_path.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(output_path, final_path)
    print(f"synthmod file created at {final_path}")
    return True


def create_zip(mod_name, version_str, input_dir, input_subpaths, output_dir):
    mod_name = str(mod_name)
    version_str = str(version_str)

    tmpdir = tempfile.mkdtemp()
    print(f"tmp directory is {tmpdir}")

    print("Setting up output dir")
    # root_output_dir = Path("..", "Downloads")
    tmp_outputdir = Path(tmpdir, mod_name)
    tmp_outputdir.mkdir(parents=True, exist_ok=True)

    output_name = f"{mod_name}_{version_str}"
    output_name_zip = f"{output_name}.zip"
    output_path = Path(tmp_outputdir, output_name_zip)
    print(f"Creating zip file {output_path}")
    with ZipFile(output_path, "w") as zip_file:
        for input_file in input_subpaths:
            real_path = Path(input_dir, input_file)
            zip_path = Path("Mods", input_file)
            # print(f"Adding {real_path} to {zip_path}")
            zip_file.write(real_path, zip_path)

    if not output_path.exists():
        print(f"Failed to create zip file {output_path}")
        return False
    
    final_zip_path = Path(output_dir, output_name, output_name_zip)
    final_zip_path.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(output_path, final_zip_path)
    print(f"zip file created at {final_zip_path}")
    return True


def add_git_tag(mod_name, version):
    print(f"Adding git tag v{version}")
    result = subprocess.run([
        "git", "tag",
        "-a",
        "-m", f"{mod_name} v{version}",
        f"v{version}"
    ])

    print("Remember to push up with `git push --tags`")
    return result.returncode == 0


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        prog='synthmod_util',
        description='Helper functions for building synthmod files')
    parser.add_argument(
        "version",
        help="Mod version being built. Ex. 1.3.0"
    )
    parser.add_argument(
        "-n",
        "--name",
        help="Mod name. Defaults to current directory name."
    )
    parser.add_argument(
        "-o",
        "--output-dir",
        type=Path,
        default=Path("build"),
        help="Output directory. Defaults to build/"
    )
    parser.add_argument(
        "-i",
        "--input-dir",
        type=Path,
        help="Input directory (build directory). Defaults to <ModName>/bin/<Configuration>"
    )
    parser.add_argument(
        "--tag",
        action="store_true",
        help="If specified, tags the version and pushes tag to repo"
    )
    
    args = parser.parse_args()

    configuration = "Release"

    version = args.version
    mod_name = args.name
    if not mod_name:
        mod_name = Path(".").resolve().stem
        print(f"Mod name not specified; using '{mod_name}'")
    input_dir = args.input_dir
    if not input_dir:
        input_dir = Path(".", mod_name, "bin", configuration).resolve()
        print(f"Input dir not specified; using '{input_dir}'")
    output_dir = args.output_dir
    if not output_dir:
        output_dir = Path("build")
        print(f"Output dir not specified; using {output_dir}")

    main_dlls = [
        f"{mod_name}.dll",
    ]
    dep_dlls = [
        "SRModCore.dll",
    ]

    commands = [
        lambda: dotnet_clean(),
        lambda: dotnet_build(configuration),
        lambda: create_zip(mod_name, version, input_dir, main_dlls + dep_dlls, output_dir),
        lambda: create_synthmod(mod_name, version, input_dir, main_dlls, output_dir),
    ]
    if args.tag:
        commands.append(lambda: add_git_tag(mod_name, version))
    for cmd in commands:
        if not cmd():
            print("Error, stopping")
            break
        print()

