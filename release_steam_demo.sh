export $(cat .env | xargs)
./steamcmd.exe +login $STEAM_USER $STEAM_PASSWORD +run_app_build ../scripts/full_demo_build.vdf +quit
