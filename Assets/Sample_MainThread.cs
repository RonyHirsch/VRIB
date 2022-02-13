using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using Newtonsoft.Json;
using System.Linq;

namespace Test120FPS
{
    public class Sample_MainThread : ULinkBase
    {
        private Sample_GetDataThread DataThread = null;
        private EyeData data = new EyeData();        // Use this for initialization        
        void Start()        
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            };
        }
        // You can get data from another thread and use MonoBehaviour's method here.       
        // But in Unity's Update function, you can only have 90 FPS.        
        void Update()       
        {            
            ViveSR.Error error = SRanipal_Eye_API.GetEyeData(ref data);
            string x = JsonConvert.SerializeObject(JsonConvert.SerializeObject(data));
            RecordLog(x);
        }   
    }
}

