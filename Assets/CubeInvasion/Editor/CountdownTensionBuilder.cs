using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Cinemachine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;
public static class CountdownTensionBuilder
{
    [MenuItem("Cube Invasion/Combat/Install Countdown Tension")]
    public static void Install()
    {
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        var follow=Object.FindAnyObjectByType<ArcadeFollowCamera>();var data=new SerializedObject(follow);
        var vcam=(CinemachineVirtualCamera)data.FindProperty("virtualCamera").objectReferenceValue;
        var shake=vcam.GetComponent<CountdownCameraShake>();if(!shake)shake=vcam.gameObject.AddComponent<CountdownCameraShake>();
        follow.ConfigureCountdown(shake);EditorUtility.SetDirty(follow);
        EditorSceneManager.MarkSceneDirty(follow.gameObject.scene);EditorSceneManager.SaveScene(follow.gameObject.scene);
        Debug.Log("CUBE_COUNTDOWN_INSTALL_OK: serialized Cinemachine noise-stage extension, camera transforms and framing preserved.");
    }
}
