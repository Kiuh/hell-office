using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<SceneManager>(Lifetime.Singleton);
    }
}
