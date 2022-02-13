using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using UnityEngine.SceneManagement;
//using UnityEditorInternal;
#if UNITY_EDITOR
//using UnityEditor;
#endif

public class ULinkConnector : MonoBehaviour
{
    [Serializable]
    public class ULinkConnectionVars
    {
        [StringInList(typeof(ULinkVarsManager), "AllULinkOutputVars")] public string From;
        [StringInList(typeof(ULinkVarsManager), "AllULinkInputVars")] public string To;
    }

    public class prevULinkConnectionVars
    {
        public string From;
        public string To;
    }

    [SerializeField]
    public prevULinkConnectionVars[] prevULinkConnections;
    [SerializeField]
    public ULinkConnectionVars[] ULinkConnections;

    void registerConnections()
    {
        if (prevULinkConnections != null)
        {
            foreach (var ulc in prevULinkConnections)
            {
                if (ulc.From != "---")
                {
                    ULinkConnect.Instance.disconnectULinkVars(ulc.From, ulc.To);
                }
            }
        }

        prevULinkConnections = new prevULinkConnectionVars[ULinkConnections.Length];
        int ind = 0;
        foreach (var ulc in ULinkConnections)
        {
            if (ulc.From != "---" && ulc.From != ulc.To) // basic avoidance of infinite loop & stack-overflow (BUG)
            {
                ULinkConnect.Instance.connectULinkVars(ulc.From, ulc.To);
            }
            prevULinkConnections[ind++] = new prevULinkConnectionVars { From = ulc.From.ToString(), To = ulc.To.ToString() };
        }
    }

    public void unregisterConnection()
    {
        if (prevULinkConnections != null)
        {
            foreach (var ulc in prevULinkConnections)
            {
                if (ulc.From != "---")
                {
                    ULinkConnect.Instance.disconnectULinkVars(ulc.From, ulc.To);
                }
            }
        }
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ULinkVarsManager.scanVars();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ULinkVarsManager.RescanVars();
    }

    // Start is called before the first frame update
    void Start()
    {
        registerConnections();
    }

    private void OnValidate()
    {
        //registerConnections();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
