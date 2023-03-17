using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataModel;
using MapGenerator.MapData;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public static class BindingExtensions
    {
        public static void BindToEnableStatus(this IUniTaskAsyncEnumerable<bool> source, MonoBehaviour monoBehaviour,
            CancellationToken cancellationToken, bool rebindOnError = true)
        {
            void Setter(bool value)
            {
                monoBehaviour.enabled = value;
            }

            BindToCore(source, Setter, cancellationToken, rebindOnError).Forget();
        }

        public static void BindToEnableStatus(this IUniTaskAsyncEnumerable<bool> source, Renderer renderer,
            CancellationToken cancellationToken, bool rebindOnError = true)
        {
            void Setter(bool value)
            {
                renderer.enabled = value;
            }

            BindToCore(source, Setter, cancellationToken, rebindOnError).Forget();
        }

        public static void BindToSize(this IUniTaskAsyncEnumerable<float> source, Scrollbar scrollbar,
            CancellationToken cancellationToken, bool rebindOnError = true)
        {
            void Setter(float value)
            {
                scrollbar.size = value;
            }

            BindToCore(source, Setter, cancellationToken, rebindOnError).Forget();
        }

        public static void BindToActivity(this IUniTaskAsyncEnumerable<bool> source, GameObject gameObject,
            CancellationToken cancellationToken, bool rebindOnError = true)
        {
            void Setter(bool value)
            {
                gameObject.SetActive(value);
            }

            BindToCore(source, Setter, cancellationToken, rebindOnError).Forget();
        }

        public static void BindMapDataToMainTex(this IUniTaskAsyncEnumerable<IGameState> source, Renderer target,
            CancellationToken cancellationToken, bool rebindOnError = true)
        {
            void Setter(IGameState value)
            {
                if (value is GameStateMapReady data) target.material.mainTexture = data.RawMapData.GetTexture();
            }

            BindToCore(source, Setter, cancellationToken, rebindOnError).Forget();
        }

        public static void BindToScale(this IUniTaskAsyncEnumerable<Resolution> source, Transform transform,
            CancellationToken cancellationToken, bool rebindOnError = true)
        {
            void Setter(Resolution value)
            {
                if (value.width != 0 && value.height != 0)
                    transform.localScale = new Vector3(value.width / (float)value.height, 1.0f, 1.0f);
            }

            BindToCore(source, Setter, cancellationToken, rebindOnError).Forget();
        }


        public static async UniTaskVoid BindToCore<TValue>(IUniTaskAsyncEnumerable<TValue> source,
            Action<TValue> setter, CancellationToken cancellationToken, bool rebindOnError)
        {
            var repeat = false;
            BIND_AGAIN:
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (true)
                {
                    bool moveNext;
                    try
                    {
                        moveNext = await e.MoveNextAsync();
                        repeat = false;
                    }
                    catch (Exception ex)
                    {
                        if (ex is OperationCanceledException) return;

                        if (rebindOnError && !repeat)
                        {
                            repeat = true;
                            goto BIND_AGAIN;
                        }
                        else
                        {
                            throw;
                        }
                    }

                    if (!moveNext) return;

                    setter(e.Current);
                }
            }
            finally
            {
                if (e != null) await e.DisposeAsync();
            }
        }
    }
}