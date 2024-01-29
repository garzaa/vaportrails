export $(cat .env | xargs)
steamcmd +login $STEAM_USER $STEAM_PASSWORD +run_app_build ../scripts/full_build.vdf +quit
