alias butler="D:/Program\ Files/butler-windows-amd64/butler.exe"
alias 7z="C:/Program\ Files/7-Zip/7z.exe"

function zip() {
    for i in win-exe win32-exe osx webgl gnu-linux; do
        7z a ../demos/zips/vaportrails-$i.zip ../demos/vaportrails-$i 
    done
}

function itchrelease() {
    for i in win-exe win32-exe osx webgl gnu-linux; do
        butler push ../demos/vaportrails-$i sevencrane/vaportrails-physics:$i
    done
}

set -x

itchrelease
zip

# python busybox.py --build $BUILD_VERSION --release

set +x
