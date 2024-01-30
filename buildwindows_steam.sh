echo "removing old demo folders"
for i in win-exe; do
    rm -r ../demos/vaportrails-$i
done

code "C:\Users\Adrian\AppData\Local\Unity\Editor\Editor.log"
echo "use revert file to pick up new changes"

echo "building project"
"C:\Program Files\Unity\Hub\Editor\2021.3.15f1\Editor\Unity.exe" \
    -quit \
    -batchmode \
    -executeMethod ProjectBuilder.BuildWindowsSteam
echo "done"

echo "copying outputs to steam build directory"

set +x
for i in win-exe; do
    cp -r ../demos/vaportrails-$i "X:\steamworks_sdk\tools\ContentBuilder\content\vaportrails-$i"
done
set -x

echo "rendering achievements..."
sh render_achievements.sh
echo "done"
