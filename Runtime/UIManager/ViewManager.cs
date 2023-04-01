// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
// using FuryLion.AssetManagement;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
//
// namespace WindowManager
// {
//     public abstract class ViewManager<TView> : Manager
//         where TView : View
//     {
//         [SerializeField] protected internal GameObject _blocker;
//         [SerializeField] protected internal UICamera _uiCamera;
//
//         [SerializeField] protected internal List<TView> _views = new();
//         [SerializeField] protected internal List<ComponentReference<TView>> _viewsAddressable = new();
//
//         public static IContext TopViewContext => _instance.TopContext;
//         
//         public static bool IsProcessing => _instance.ProcessingSemaphore.CurrentCount > 0;
//
//         protected static Dictionary<Type, View> Views = new();
//         protected static int LastViewsCount;
//         protected static IViewBlocker Blocker;
//
//         private Transform _transform;
//
//         protected void BasicInitialization()
//         {
//             CheckManagerValid();
//
//             _transform = new GameObject(name).transform;
//
//             DontDestroyOnLoad(_transform);
//
//             Instantiate(_uiCamera, _transform);
//             Blocker = Instantiate(_blocker, _transform).GetComponent<IViewBlocker>();
//         }
//
//         protected void Initialization()
//         {
//             BasicInitialization();
//
//             var list = new List<View>();
//
//             foreach (var view in _views)
//             {
//                 list.Add(CreateView(view));
//             }
//
//             Init(list);
//         }
//
//         private async Task<IContext> GetView<T1View>() where T1View : TView
//         {
//             await LoadView<T1View>();
//
//             return OpenView<T1View>();
//         }
//
//         private async Task<IContext> GetView<T1View, TData>(TData data) where T1View : TView, IDataReceiver<TData>
//         {
//             await LoadView<T1View>();
//
//             return OpenView<T1View, TData>(data);
//         }
//
//         private async Task<IContext<TResult>> GetView<T1View, TResult>() where T1View : TView, IDataSender<TResult>
//         {
//             await LoadView<T1View>();
//
//             return OpenViewWithResult<T1View, TResult>();
//         }
//
//         private async Task<IContext<TResult>> GetView<T1View, TData, TResult>(TData data)
//             where T1View : TView, IDataReceiver<TData>, IDataSender<TResult>
//         {
//             await LoadView<T1View>();
//
//             return OpenViewWithResult<T1View, TData, TResult>(data);
//         }
//
//         public static async UniTask LoadView<T1View>() where T1View : TView
//         {
//             if (!AddressableManager.ContainsView(typeof(T1View)))
//             {
//                 foreach (var reference in _instance._viewsAddressable)
//                 {
//                     var path = AssetDatabase.GUIDToAssetPath(reference.AssetGUID);
//
//                     if (path.Contains($"{typeof(T1View)}.prefab"))
//                     {
//                         var view = await _instance.CreateAddressableView(reference);
//
//                         AddressableManager.AddView(view);
//
//                         break;
//                     }
//                 }
//             }
//         }
//
//         public static IContext Open<T1View>()
//             where T1View : TView
//         {
//             var context = _instance.GetView<T1View>();
//             context.Wait();
//             return context.Result;
//         }
//
//         public static IContext Open<T1View, TData>(TData data)
//             where T1View : TView, IDataReceiver<TData>
//         {
//             var context = _instance.GetView<T1View, TData>(data);
//             context.Wait();
//             return context.Result;
//         }
//
//         public static IContext<TResult> OpenWithResult<T1View, TResult>()
//             where T1View : TView, IDataSender<TResult>
//         {
//             var context = _instance.GetView<T1View, TResult>();
//             context.Wait();
//             return context.Result;
//         }
//
//         public static IContext<TResult> OpenWithResult<T1View, TData, TResult>(TData data)
//             where T1View : TView, IDataReceiver<TData>, IDataSender<TResult>
//         {
//             var context = _instance.GetView<T1View, TData, TResult>(data);
//             context.Wait();
//             return context.Result;
//         }
//
//         public static async UniTask CloseAll()
//         {
//             await _instance.CloseAllViews();
//         }
//
//         public static IContext CloseLast()
//         {
//             return _instance.CloseLastView();
//         }
//
//         public static void Remove<T1View>(View view) where T1View : TView
//         {
//             _instance.RemoveView<T1View>(view.Context);
//         }
//         
//         private async UniTask<TView> CreateAddressableView<TView>(ComponentReference<TView> view)
//             where TView : View
//         {
//             var viewObj = view.InstantiateAsync(_transform).WaitForCompletion();
//
//             var viewTransform = viewObj.transform;
//             viewTransform.localPosition = PoolPosition;
//             viewTransform.localScale = Vector3.one;
//
//             viewObj.SetActive(false);
//
//             viewObj.gameObject.AddComponent<ReleaseOnDestroy>();
//
//             return viewObj;
//         }
//
//         private View CreateView(View view)
//         {
//             var viewObj = Instantiate(view, null, false);
//
//             var viewTransform = viewObj.transform;
//             viewTransform.localPosition = PoolPosition;
//             viewTransform.localScale = Vector3.one;
//
//             viewTransform.SetParent(_transform);
//
//             viewObj.SetActive(false);
//             return viewObj;
//         }
//
//
//         protected override IDisposable ExecuteShowProcess(View view)
//         {
//             var tView = view as TView;
//
//             if (tView == null)
//                 return null;
//
//
//             if (Counter == 1)
//                 Blocker.FadeIn();
//             else
//                 Blocker.Transform.SetAsLastSibling();
//
//             var blocker = Recycler.Get<Blocker>();
//
//             tView.Scale = Vector3.zero;
//             tView.Position = Vector3.zero;
//             tView.SetActive(true);
//
//             var lifecycles = view.GetComponentsInChildren<IViewLifecycleObserver>();
//             var disposable = new DisposableSource(OnDispose);
//
//             foreach (var lifecycle in lifecycles)
//                 lifecycle.OpenStart();
//
//             return disposable;
//
//             void OnDispose()
//             {
//                 foreach (var lifecycle in lifecycles)
//                     lifecycle.OpenComplete();
//
//                 Recycler.Release(blocker);
//             }
//         }
//
//         protected override IDisposable ExecuteHideProcess(View view)
//         {
//             var tView = view as TView;
//
//             if (tView == null)
//                 return null;
//
//             var blocker = Recycler.Get<Blocker>();
//
//
//             if (Counter == 0)
//                 Blocker.FadeOut();
//
//             var lifecycles = view.GetComponentsInChildren<IViewLifecycleObserver>();
//             var disposable = new DisposableSource(OnDispose);
//
//             foreach (var lifecycle in lifecycles)
//                 lifecycle.CloseStart();
//
//             return disposable;
//
//             void OnDispose()
//             {
//                 foreach (var lifecycle in lifecycles)
//                     lifecycle.CloseComplete();
//
//                 Recycler.Release(blocker);
//                 tView.SetActive(false);
//                 tView.Position = PoolPosition;
//
//                 if (_transform.childCount > 0)
//                     Blocker.Transform.SetAsLastSibling();
//             }
//         }
//
//         private void CheckManagerValid()
//         {
//             if (_viewsAddressable.Count == 0 && _views.Count == 0)
//             {
//                 NotificationGUI.ShowNotification($"Views not found in {name}", NotificationType.Error);
//
//                 EditorApplication.ExitPlaymode();
//             }
//
//             if (!_uiCamera || !_blocker)
//             {
//                 NotificationGUI.ShowNotification($"Camera or blocker not found in {name}", NotificationType.Error);
//
//                 EditorApplication.ExitPlaymode();
//             }
//         }
//
//         protected virtual void OnAwake()
//         {
//         }
//
//         private static ViewManager<TView> _instance;
//
//         // "Manager";
//         private const string ResourcesPath = "ScriptableObjects/UIManagers";
//
//
//         protected static ViewManager<TView> GetInstance<T>(string fileName) where T : ViewManager<TView>
//         {
//             if (_instance != null)
//                 return _instance;
//
//             _instance = Resources.Load($"{ResourcesPath}/{fileName}") as T;
//             if (_instance != null)
//                 return _instance;
//
//             _instance = CreateInstance<T>();
//
// #if UNITY_EDITOR
//
//             var assetDirectory = $"Assets/Resources/{ResourcesPath}";
//             if (!System.IO.Directory.Exists(assetDirectory))
//                 System.IO.Directory.CreateDirectory(assetDirectory);
//
//             UnityEditor.AssetDatabase.CreateAsset(_instance, $"{assetDirectory}/{fileName}.asset");
//             UnityEditor.AssetDatabase.SaveAssets();
//             UnityEditor.AssetDatabase.Refresh();
//             
// #else
//             Debug.LogError("Created new Manager");
// #endif
//
//             return _instance;
//         }
//     }
//     
//     internal class ReleaseOnDestroy : MonoBehaviour
//     {
//         private void OnDestroy()
//         {
//             Addressables.ReleaseInstance(gameObject);
//         }
//     }
// }