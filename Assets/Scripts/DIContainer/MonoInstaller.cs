using UnityEngine;

public abstract class MonoInstaller : MonoBehaviour
{
    public abstract void InstallBindings(DIContainer container);
}