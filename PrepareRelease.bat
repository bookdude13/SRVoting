
set BUILT_VERSION="1.1.1"
set MOD_NAME="SRVoting"

set RELEASE_BUILD_DIR=".\%MOD_NAME%\bin\Release"
set MAIN_DLL="%RELEASE_BUILD_DIR%\%MOD_NAME%.dll"
set LIB_DLL_DIR="%RELEASE_BUILD_DIR%\libs"

set OUTPUT_DIR=".\build\%MOD_NAME%_v%BUILT_VERSION%"
mkdir %OUTPUT_DIR%

copy %MAIN_DLL% %OUTPUT_DIR%
copy %LIB_DLL_DIR%\* %OUTPUT_DIR%
