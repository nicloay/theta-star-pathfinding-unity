using Controllers;
using Controllers.UI;
using Cysharp.Threading.Tasks;
using MapGenerator.MapData;
using MessagePipe;
using Messages;
using Services;
using UnityEngine;
using Utils;
using VContainer;
using VContainer.Unity;

public class SceneScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        RegisterReactiveProperties(builder);
        RegisterEntryPoints(builder);
        InjectToComponentsOnScene(builder);
        RegisterMessagePipe(builder);
        
        builder.RegisterInstance(new LevelGenerationProgress());
        
        base.Configure(builder);
    }

    private void RegisterMessagePipe(IContainerBuilder builder)
    {
        var options = builder.RegisterMessagePipe();
        builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
        builder.RegisterMessageBroker<MapError>(options);
    }

    // components which are only receive injections, You can not inject these types to another consumers
    private void InjectToComponentsOnScene(IContainerBuilder builder)
    {
        InjectToComponents<ResolutionToScaleCtrl>(builder);
        InjectToComponents<MaterialTextureUpdateCtrl>(builder);
        InjectToComponents<PathRenderController>(builder);
        InjectToComponents<MapGenerationUICtrl>(builder);
        InjectToComponents<PathfindingUICtrl>(builder);
        InjectToComponents<ErrorHandlerUICtrl>(builder);
    }

    
    /// <summary>
    /// Services with the same lifetime as Scope
    /// </summary>
    private void RegisterEntryPoints(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ScreenSizeMonitor>().AsSelf();
        builder.RegisterEntryPoint<GameManager>().AsSelf();
    }

    /// <summary>
    /// Collection of reactive properties
    /// </summary>
    private void RegisterReactiveProperties(IContainerBuilder builder)
    {
        RegisterReactiveProperty<IMapData>(builder, new EmptyMapData());
        RegisterReactiveProperty(builder, GameState.Unknown);
        RegisterReactiveProperty(builder, new Resolution(){height = Screen.height, width = Screen.width});
    }

    private static void RegisterReactiveProperty<T>(IContainerBuilder builder, T value)
    {
        builder.RegisterInstance<AsyncReactiveProperty<T>>(new AsyncReactiveProperty<T>(value))
            .As<IAsyncReactiveProperty<T>>()
            .As<IReadOnlyAsyncReactiveProperty<T>>();
    }

    private void InjectToComponents<T>(IContainerBuilder builder) where T : Component
    {
        builder.RegisterBuildCallback(resolver =>
        {
            foreach (var component in gameObject.scene.FindComponents<T>()) resolver.Inject(component);
        });
    }
}