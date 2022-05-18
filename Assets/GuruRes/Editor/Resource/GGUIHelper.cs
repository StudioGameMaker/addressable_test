using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GuruFramework
{
    public static class GGUI
    {

        public static Color ColorGreen = new Color(0.54f, 1, 0);
        public static Color ColorOrange = new Color(1, 0.73f, 0);

        public static void GButton<T>(this T window, string btnName, Action onClick, float width = 0,
            float height = 0) where T : EditorWindow
        {
            GButton(window, btnName, onClick, Color.white, width, height);
        }

        public static void GButton<T>(this T window, string btnName, Action onClick, Color color, float width = 0, float height = 0 ) where T : EditorWindow
        {
            List<GUILayoutOption> opts = new List<GUILayoutOption>();
            if (width > 0) opts.Add(GUILayout.Width(width));
            if (height > 0) opts.Add(GUILayout.Height(height));
            
            GColorUI(window, color, ()=>{
                if (GUILayout.Button(btnName, opts.ToArray()))
                {
                    onClick?.Invoke();
                }
            });
        }

        
        public static void GButtonGreen<T>(this T window, string btnName, Action onClick, float width = 0, float height = 0) where T : EditorWindow
        {
            GButton(window, btnName, onClick, ColorGreen, width, height);
        }


        public static void GColorUI<T>(this T window, Color color, Action handler) where T : EditorWindow
        {
            Color c = GUI.color;
            GUI.color = color;
            handler?.Invoke();
            GUI.color = c;
        }
        
        public static void Horizontal(this EditorWindow window, Action handler)
        {
            GUILayout.BeginHorizontal(new GUIStyle("Block"));
            handler?.Invoke();
            GUILayout.EndHorizontal();
        }
        
        
        
        
    }
}