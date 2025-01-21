using UnityEngine;

public class ProjectInstaller : MonoBehaviour
{
    public static DIContainer Container { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (FindObjectOfType<ProjectInstaller>() == null)
        {
            GameObject installerObject = new GameObject("ProjectInstaller");
            installerObject.AddComponent<ProjectInstaller>();
        }
        
        Container = new DIContainer();
    }

    private void Start()
    {
        ExecuteCustomInstallers();
    }

    private static void ExecuteCustomInstallers()
    {
        var installers = Resources.FindObjectsOfTypeAll<MonoInstaller>();
        foreach (var installer in installers)
        {
            installer.InstallBindings(Container);
        }
    }
}
