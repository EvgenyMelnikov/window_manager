using UnityEngine;

namespace WindowManager
{
    public class BackManager : MonoBehaviour
    {
        public static bool ActiveInput = true;

        public static BackManager Instance { get; private set; }
        
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            var gameObject = new GameObject("BackManager", typeof(BackManager));
            Instance = gameObject.GetComponent<BackManager>();
            
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            if (!ActiveInput)
                return;

            if (!Input.GetKeyUp(KeyCode.Escape))
                return;
            
            // if (MessageBoxManager.TopViewContext != null)
            //     MessageBoxManager.CloseLast();
            //
            // else if (PopupManager.TopViewContext != null)
            //     PopupManager.CloseLast();
            //
            // else if (PageManager.TopViewContext != null)
            //     PageManager.CloseLast();
        }
    }
}