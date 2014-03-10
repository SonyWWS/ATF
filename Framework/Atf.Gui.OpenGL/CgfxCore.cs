//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Tao.Cg;

namespace Sce.Atf.Rendering.OpenGL
{
    /// <summary>
    /// Cg (C for Graphics) shader, a wrapper for Tao.Cg that handles error checking API calls</summary>
    public static class CgfxCore
    {
        /// <summary>
        /// Initializes CgfxCore</summary>
        public static void Init()
        {
            Cg.cgSetErrorCallback(ErrorCallback);

            if (CgGl.cgGLIsProfileSupported(Cg.CG_PROFILE_ARBVP1))
                m_cg_profiles["arbvp1"] = Cg.CG_PROFILE_ARBVP1;

            if (CgGl.cgGLIsProfileSupported(Cg.CG_PROFILE_ARBFP1))
                m_cg_profiles["arbfp1"] = Cg.CG_PROFILE_ARBFP1;

            if (CgGl.cgGLIsProfileSupported(Cg.CG_PROFILE_VP20))
                m_cg_profiles["vp20"] = Cg.CG_PROFILE_VP20;

            if (CgGl.cgGLIsProfileSupported(Cg.CG_PROFILE_FP20))
                m_cg_profiles["fp20"] = Cg.CG_PROFILE_FP20;

            if (CgGl.cgGLIsProfileSupported(Cg.CG_PROFILE_VP30))
                m_cg_profiles["vp30"] = Cg.CG_PROFILE_VP30;

            if (CgGl.cgGLIsProfileSupported(Cg.CG_PROFILE_FP30))
                m_cg_profiles["fp30"] = Cg.CG_PROFILE_FP30;

            if (CgGl.cgGLIsProfileSupported(Cg.CG_PROFILE_VP30))
                m_cg_profiles["vp40"] = Cg.CG_PROFILE_VP40;

            const int FP40 = 6151;
            if (CgGl.cgGLIsProfileSupported(Cg.CG_PROFILE_FP30))
                m_cg_profiles["fp40"] = FP40;

            if (Cg.cgIsContext(m_cg_context) == 0)
            {
                m_cg_context = Cg.cgCreateContext();
            }
        }

        /// <summary>
        /// Creates a Cg program</summary>
        /// <param name="id">ID to identify this Cg program</param>
        /// <param name="type">Tells whether this Cg program is in CG_SOURCE or CG_OBJECT</param>
        /// <param name="file">Either the string to a path of a Cg program, or the string that stores the whole Cg program</param>
        /// <param name="profile">String for the profile used by the Cg program</param>
        /// <param name="entry">String for the entry function name of the Cg program</param>
        /// <param name="fromfile">True if argument "file" is a string to a path of a file, 
        /// false if argument "file" is a string that store the whole Cg program</param>
        /// <returns>True if the CgfxCore creates program successfully</returns>
        public static bool CreateProgram(string id, int type, string file, string profile, string entry, bool fromfile)
        {
           try
            {
                if (m_cg_programs.ContainsKey(id))
                    return false;
                if (fromfile)
                    m_cg_programs[id] = Cg.cgCreateProgramFromFile(m_cg_context, type, file, m_cg_profiles[profile], entry, null);
                else
                    m_cg_programs[id] = Cg.cgCreateProgram(m_cg_context, type, file, m_cg_profiles[profile], entry, null);
                m_cg_program_counter++;
//                System.Console.WriteLine("  Cg CreateProgram Counter  ," + m_cg_context + "," + m_cg_program_counter + " id=" + id);
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Compiles and loads a Cg program</summary>
        /// <param name="id">ID string for the Cg program we want to compile and load</param>
        public static void LoadProgram(string id)
        {
            if (!Cg.cgIsProgramCompiled(m_cg_programs[id]))
                Cg.cgCompileProgram(m_cg_programs[id]);
            if (!CgGl.cgGLIsProgramLoaded(m_cg_programs[id]))
                CgGl.cgGLLoadProgram(m_cg_programs[id]);
        }

        /// <summary>
        /// Binds a Cg program</summary>
        /// <param name="id">ID for the Cg program we want to bind</param>
        public static void BindProgram(string id)
        {
            CgGl.cgGLBindProgram(m_cg_programs[id]);
//            System.Console.WriteLine("  Cg BindProgram  ," + m_cg_context + "," + m_cg_program_counter + " id=" + id);
        }

        /// <summary>
        /// Enables a Cg profile</summary>
        /// <param name="profile">ID for profile we want to enable</param>
        public static void EnableProfile(string profile)
        {
            CgGl.cgGLEnableProfile(m_cg_profiles[profile]);
        }

        /// <summary>
        /// Disables a Cg profile</summary>
        /// <param name="profile">ID for profile we want to disable</param>
        public static void DisableProfile(string profile)
        {
            CgGl.cgGLDisableProfile(m_cg_profiles[profile]);
        }

        /// <summary>
        /// Unbinds a program from a Cg profile</summary>
        /// <param name="profile">ID for profile we want to unbind the Cg program with</param>
        public static void UnBindProgram(string profile)
        {
            CgGl.cgGLUnbindProgram(m_cg_profiles[profile]);
//            System.Console.WriteLine("  Cg UnBindProgram  ," + m_cg_context + "," + m_cg_program_counter + " profile=" + profile);
        }

        /// <summary>
        /// Gets the parameter used in a Cg program</summary>
        /// <param name="id">ID for Cg program that we want to get param from</param>
        /// <param name="parameter">Name for parameter used in a Cg program</param>
        /// <param name="semantic">The semantic in this parameter, so that we can use the proper Cg API to get parameter</param>
        public static void GetParameter(string id, string parameter, string semantic)
        {
            if (semantic == "SAMPLER2DSTATE")
                m_cg_parameters[parameter] = Cg.cgGetNamedSamplerState(m_cg_context, parameter);
            else 
                m_cg_parameters[parameter] = Cg.cgGetNamedParameter(m_cg_programs[id], parameter);
//            System.Console.WriteLine("Cg GetParameter " + gl_context + "," + m_cg_context + "," + m_cg_program_counter);
        }

        /// <summary>
        /// Enables or binds a texture with a parameter and a texture ID</summary>
        /// <param name="parameter">Name of parameter used in the Cg program, usually a Simpler2D</param>
        /// <param name="texture_id">Texture ID from glTexGen()</param>
        /// <returns>True if texture enabled successfully</returns>
        public static bool EnableTexture(string parameter, int texture_id)
        {
            IntPtr param;
            try {
                m_cg_parameters.TryGetValue(parameter, out param);
                if (param == (IntPtr)0) return false;
                CgGl.cgGLSetTextureParameter(param, texture_id);
                CgGl.cgGLEnableTextureParameter(param);
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Disables or unbinds a specific parameter</summary>
        /// <param name="parameter">Name of parameter used in the Cg program, usually a Simpler2D</param>
        /// <returns>True if texture disabled successfully</returns>
        public static bool DisableTexture(string parameter)
        {
            IntPtr param;
            try {
                m_cg_parameters.TryGetValue(parameter, out param);
                if (param == (IntPtr)0) return false;
                CgGl.cgGLDisableTextureParameter(param);
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Sets parameter used in a Cg program</summary>
        /// <param name="parameter">Name of parameter that we want to set</param>
        /// <param name="array">Float array as values that we want to set parameter with</param>
        /// <returns>True if parameter set successfully</returns>
        public static bool SetParameter(string parameter, float[] array)
        {
            if (array == null) return false;
            IntPtr param;
            try {
                m_cg_parameters.TryGetValue(parameter, out param);
                if (param == (IntPtr)0) return false;
                int length = array.Length;
                switch (length)
                {
                    case 1:
                        Cg.cgSetParameter1f(param, array[0]);
                        break;
                    case 2:
                        Cg.cgSetParameter2f(param, array[0], array[1]);
                        break;
                    case 3:
                        Cg.cgSetParameter3f(param, array[0], array[1], array[2]);
                        break;
                    case 4:
                        Cg.cgSetParameter4f(param, array[0], array[1], array[2], array[3]);
                        break;
                    case 16:
                        CgGl.cgGLSetMatrixParameterfc(param, array);
                        break;
                    default:
                        break;
                }
            }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Sets a matrix parameter on a special semantic in a Cg program</summary>
        /// <param name="parameter">Name of parameter that we want to set</param>
        /// <param name="semantic">The semantic that is needed to set a parameter in a special way</param>
        /// <returns>True if parameter set successfully</returns>
        public static bool SetMatrixParameter(string parameter, string semantic)
        {
            IntPtr param;
            int matrix = 0;
            int tranform = 0;
            try
            {
                m_cg_parameters.TryGetValue(parameter, out param);
                if (semantic == "WORLDVIEWPROJECTION")
                {
                    matrix = CgGl.CG_GL_MODELVIEW_PROJECTION_MATRIX;
                    tranform = CgGl.CG_GL_MATRIX_IDENTITY;
                }

                CgGl.cgGLSetStateMatrixParameter(param, matrix, tranform);
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Destroys a Cg program created from CreateProgram</summary>
        /// <param name="id">ID for the Cg program to be destroyed</param>
        public static void DestroyProgram(string id)
        {
            if (Cg.cgIsProgram(m_cg_programs[id]))
            {
                System.Console.WriteLine("Cg DestroyProgram Counter = " + m_cg_program_counter);
                m_cg_program_counter--;
                Cg.cgDestroyProgram(m_cg_programs[id]);
            }
            else
            {
                System.Console.WriteLine("Cg Can't DestroyProgram Counter = " + m_cg_program_counter);
            }
        }

        /// <summary>
        /// Destroys all Cg programs and shuts down CgfxCore</summary>
        public static void Shutdown()
        {
            foreach (KeyValuePair<string, IntPtr> pair in m_cg_programs)
                DestroyProgram(pair.Key);

            if (Cg.cgIsContext(m_cg_context) != 0)
                Cg.cgDestroyContext(m_cg_context);        
        }

        /// <summary>
        /// Error callback from Tao.Cg layer</summary>
        public static void ErrorCallback()
        {
            string error_string = Cg.cgGetErrorString(Cg.cgGetError());
            System.Console.WriteLine("Cg error " + error_string);
        }

        private static readonly Dictionary<string, int> m_cg_profiles = new Dictionary<string,int>();
        private static IntPtr m_cg_context;
        private static readonly Dictionary<string, IntPtr> m_cg_programs = new Dictionary<string,IntPtr>();
        private static int m_cg_program_counter = 0;
        private static readonly Dictionary<string, IntPtr> m_cg_parameters = new Dictionary<string, IntPtr>();

        /// <summary>
        /// Gets Cg program is in source code format</summary>
        public static int CG_SOURCE
        {
            get { return Cg.CG_SOURCE; }
        }
        /// <summary>
        /// Gets Cg program is in object code format</summary>
        public static int CG_OBJECT
        {
            get { return Cg.CG_OBJECT; }
        }
    }
}
