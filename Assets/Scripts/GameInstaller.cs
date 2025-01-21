public class GameInstaller : MonoInstaller
{
    public override void InstallBindings(DIContainer container)
    {
        container.Bind<PlayerController>(nonLazy: true, DIContainer.LifeCycle.Singleton);
    }
}
