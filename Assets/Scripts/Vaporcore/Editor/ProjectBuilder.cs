using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;

public class ProjectBuilder {

    static EditorBuildSettingsScene[] enabledScenes;

    public static void BuildSteam() {
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, "STEAM");
        BuildWindows();
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, "");
    }

    public static void BuildAll() {
        enabledScenes = GetEnabledScenes();
        Build(BuildTarget.WebGL, "webgl");
        Build(BuildTarget.StandaloneWindows, "win32-exe", extension: ".exe");
        Build(BuildTarget.StandaloneOSX, "osx");
        Build(BuildTarget.StandaloneLinux64, "gnu-linux", extension: ".x86");
        Build(BuildTarget.StandaloneWindows64, "win-exe", extension: ".exe");
    }

    public static void BuildWindows() {
        enabledScenes = GetEnabledScenes();
        Build(BuildTarget.StandaloneWindows64, "win-exe", extension: ".exe");
    }

    static void Build(BuildTarget target, string folderSuffix, string extension="") {
        Console.WriteLine($"Starting build for {folderSuffix}");
        BuildReport report = BuildPipeline.BuildPlayer(enabledScenes, BuildFolder(folderSuffix.ToString(), extension), target, BuildOptions.None);
        if (report.summary.result.Equals(BuildResult.Succeeded)) {
            Console.WriteLine($"Finished! Build for {folderSuffix} succeeded with size {report.summary.totalSize}");
        } else {
            if (report.summary.result == BuildResult.Succeeded) {
                Console.WriteLine("Build Success for "+folderSuffix);
            }
            Console.WriteLine($"Build for {folderSuffix} finished with result: {report.summary.result}");
            Console.WriteLine($"Total errors: {report.summary.totalErrors}");
        }
    }

    static string BuildFolder(string platform, string extension) {
        return $"../demos/vaportrails-{platform}/Vapor Trails{extension}";
    }

    static EditorBuildSettingsScene[] GetEnabledScenes() {
        return EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();
    }
}
