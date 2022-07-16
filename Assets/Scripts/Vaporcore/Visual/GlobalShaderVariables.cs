using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GlobalShaderVariables : MonoBehaviour {
    float t;

    void Start() {
        StartCoroutine(UpdateStepTime());
    }

    void Update() {
        Shader.SetGlobalFloat("_UnscaledTime", Time.unscaledTime);
    }

    IEnumerator UpdateStepTime() {
        t = Time.time;
        Shader.SetGlobalVector("_StepTime", new Vector4(
            t/20f,
            t,
            t*2f,
            t*3f
        ));
        yield return new WaitForSeconds(1f/16f);
        StartCoroutine(UpdateStepTime());
    }
}
