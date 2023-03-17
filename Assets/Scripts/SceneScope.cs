using Controllers;
using Controllers.UI;
using Cysharp.Threading.Tasks;
using DataModel;
using MessagePipe;
using Messages;
using Pathfinding;
using Services;
using UnityEngine;
using UnityEngine.EventSystems;
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

        builder.RegisterComponentInHierarchy<EventSystem>(); // this component can be injected anywhere
        builder.RegisterComponentInHierarchy<MapCameraCtrl>();

        base.Configure(builder);
    }

    private void RegisterMessagePipe(IContainerBuilder builder)
    {
        var options = builder.RegisterMessagePipe();
        builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
        builder.RegisterMessageBroker<MapError>(options);
        builder.RegisterMessageBroker<VisualMessage>(options);
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
        InjectToComponents<VisualLoggerUICtrl>(builder);
        InjectToComponents<RunTestUIController>(builder);
        InjectToComponents<PathInputController>(builder);
        InjectToComponents<StartEndPointCtrl>(builder);
    }


    /// <summary>
    ///     Services with the same lifetime as Scope
    /// </summary>
    private void RegisterEntryPoints(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ScreenSizeMonitor>().AsSelf();
        builder.RegisterEntryPoint<GameManager>().AsSelf();
        builder.RegisterEntryPoint<VisualMessageLogger>().AsSelf();
        builder.RegisterEntryPoint<MapErrorProxy>().AsSelf();
    }

    /// <summary>
    ///     Collection of reactive properties
    /// </summary>
    private void RegisterReactiveProperties(IContainerBuilder builder)
    {
        RegisterReactiveProperty<IGameState>(builder, new GameStateNan());
        RegisterReactiveProperty(builder, PathFindingType.Fast);
        RegisterReactiveProperty(builder, new Resolution { height = Screen.height, width = Screen.width });
        RegisterReactiveProperty<IInputState>(builder, new InputIdle());
    }

    private static void RegisterReactiveProperty<T>(IContainerBuilder builder, T value)
    {
        builder.RegisterInstance(new AsyncReactiveProperty<T>(value))
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