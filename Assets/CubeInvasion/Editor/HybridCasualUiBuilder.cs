using UnityEditor;
public static class HybridCasualUiBuilder
{
    [MenuItem("Cube Invasion/UI/Apply Hybrid Casual Style")]
    public static void Install()=>ReferenceUiBuilder.Install();
    public static void InstallGameplayScene()=>ReferenceUiBuilder.InstallGameplayScene();
}
