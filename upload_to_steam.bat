@echo off
setlocal

:: ================================
:: CONFIGURATION À MODIFIER ICI
:: ================================
set STEAM_SDK_PATH="C:\Users\thoma\CoffeeShop\Steam\steamworks_sdk_162\sdk\tools\ContentBuilder"
set PROJECT_BUILD_PATH="C:\Users\thoma\CoffeeShop\steam-version"
set APP_BUILD_SCRIPT="C:\Users\thoma\CoffeeShop\Steam\steamworks_sdk_162\sdk\tools\ContentBuilder\scripts\app_build_4086250.vdf"
set STEAM_LOGIN="aziteck28"

echo =============================
echo UPLOAD DU BUILD SUR STEAM
echo =============================

:: 1️⃣ Nettoyer le dossier content/
echo Nettoyage du dossier content...
rmdir /s /q "%STEAM_SDK_PATH%\content"
mkdir "%STEAM_SDK_PATH%\content"

:: 2️⃣ Copier ton build Unity
echo Copie des fichiers du build Unity...
xcopy "%PROJECT_BUILD_PATH%\*" "%STEAM_SDK_PATH%\content" /E /Y
if %errorlevel% neq 0 (
    echo Erreur lors de la copie des fichiers.
    exit /b %errorlevel%
)

:: 3️⃣ Upload via SteamCMD
echo 🔹 Lancement de SteamCMD...
cd /d "%STEAM_SDK_PATH%"
builder\steamcmd.exe +login %STEAM_LOGIN% +run_app_build %APP_BUILD_SCRIPT% +quit

if %errorlevel% neq 0 (
    echo Erreur lors de l'upload vers Steam.
    exit /b %errorlevel%
)

echo Upload terminé avec succès !
endlocal
pause
