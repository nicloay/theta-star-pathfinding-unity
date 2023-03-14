using Controllers;
using Cysharp.Threading.Tasks;
using Modules.MapGenerator;
using Services;
using UnityEngine;
using Utils;
using VContainer;
using VContainer.Unity;

public class SceneScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<MapGenerator>().AsSelf();
        builder.RegisterEntryPoint<ScreenSizeMonitor>().AsSelf();

        InjectToComponents<ResolutionToScaleCtrl>(builder);
        InjectToComponents<MaterialTextureUpdateCtrl>(builder);
        InjectToComponents<PathRenderController>(builder);
        base.Configure(builder);
    }
    
    private void InjectToComponents<T>(IContainerBuilder builder) where T : Component
    {
        builder.RegisterBuildCallback(resolver =>
        {
            foreach (var component in gameObject.scene.FindComponents<T>())
            {
                resolver.Inject(component);
            }
        });
    }
}
