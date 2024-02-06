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
    -executeMethod ProjectBuilder.BuildWindows
echo "done"
echo $(cat "C:\Users\Adrian\AppData\Local\Unity\Editor\Editor.log" | grep "Build for")

echo "copying outputs to steam build directory"
