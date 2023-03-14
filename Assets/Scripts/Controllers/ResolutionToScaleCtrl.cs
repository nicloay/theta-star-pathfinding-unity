﻿using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Services;
using UnityEngine;
using Utils;
using VContainer;

namespace Controllers
{
    public class ResolutionToScaleCtrl : MonoBehaviour
    {
        [Inject]
        [UsedImplicitly]
        private void InjectResolution(ScreenSizeMonitor monitor)
        {
            monitor.Resolution.BindToScale(transform, this.GetCancellationTokenOnDestroy());
        }
    }
}