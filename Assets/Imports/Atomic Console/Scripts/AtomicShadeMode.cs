using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtomicAssembly
{
    public class AtomicShadeMode : MonoBehaviour
    {
        public static bool EnableWireframe { get; private set; } = false;
        public static bool EnableCulling { get; private set; } = false;



        private bool oldCulling;
        void OnPreRender()
        {
            if (EnableWireframe)
            {
                GL.wireframe = true;
            }

            if(EnableCulling)
            {
                oldCulling = GL.invertCulling;
                GL.invertCulling = true;
            }
        }

        void OnPostRender()
        {
            if (EnableWireframe)
            {
                GL.wireframe = false;
            }

            if(EnableCulling)
            {
                GL.invertCulling = oldCulling;
            }
        }

        public static void ToggleWireframe()
        {
            EnableWireframe = !EnableWireframe;
        }


        public static void ToggleCulling()
        {
            EnableCulling = !EnableCulling;
        }
    }
}
