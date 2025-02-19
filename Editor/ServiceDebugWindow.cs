using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Services.Editor
{
    public class ServiceDebugWindow : EditorWindow
    {
        string[] _cachedServicesName;
        List<IService> _foundServices;
        int _currentServiceHash;
        IService _currentService;

        Color _color_blueColor; 
        
        Texture2D _texture2D_blueColor;
        

        [MenuItem("PService/Service Explorer")]
        static void ShowWindow()
        {
            GetWindow<ServiceDebugWindow>("Service debugger");
        }

        void OnEnable()
        {
            ColorUtility.TryParseHtmlString("#3FC6FF", out _color_blueColor);
            
            TryCacheServiceNames();

            _texture2D_blueColor = new Texture2D(1, 1);
            _texture2D_blueColor.SetPixel(0, 0, _color_blueColor);
            _texture2D_blueColor.Apply();
        }

        void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Service inspector only works during play mode", MessageType.Warning);
                return;
            }

            DrawSidePanel();
            DrawMainPanel();
        }

        GUIStyle GetStyle_Selected()
        {
            GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
            guiStyle.normal.background = _texture2D_blueColor;
            return guiStyle;
        }

        GUIStyle GetStyle_h1()
        {
            GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
            guiStyle.fontSize = 38;
            return guiStyle;
        }

        GUIStyle GUIStyle_SetPadding(GUIStyle baseStyle, int padding)
        {
            GUIStyle guiStyle = new GUIStyle(baseStyle);
            guiStyle.padding = new RectOffset(left: padding, top: padding, right: padding, bottom: padding);
            return guiStyle;
        }

        void DrawSidePanel()
        {
            GUILayout.BeginArea(new Rect(new Vector2(5, 5), new Vector2(190, position.size.y - 10)), GUI.skin.box);

            ServiceContainer.EDITOR_DrawServiceSelector(serviceHash =>
            {
                bool isSelected = serviceHash == _currentServiceHash;
                IService service = ServiceContainer.EDITOR_GetServiceByHash(serviceHash);
                bool serviceSelected = GUILayout.Button(service.Name, isSelected ? GetStyle_Selected() : GUI.skin.label);
                if (serviceSelected)
                {
                    _currentServiceHash = serviceHash;
                    _currentService = service;
                }
            });
            
            GUILayout.EndArea();
        }
        
        void DrawMainPanel()
        {
            GUILayout.BeginArea(new Rect(new Vector2(205, 5), new Vector2(position.size.x - 210, position.size.y - 10)), GUIStyle_SetPadding(GUI.skin.box, 10));
            if (null == _currentService)
            {
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label(_currentService.Name, GetStyle_h1());
            GUILayout.Space(10f);
            _currentService.DebugService();
            
            GUILayout.EndArea();
        }

        string[] GetServices()
        {
            var type = typeof(IService);
            var types = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => p.GetInterfaces().Contains(type))
                .Select(t => t.FullName)
                .Where(name => name.Contains("Implementation"));
                
            return types.ToArray();
        }

        void TryCacheServiceNames()
        {
            if (!Application.isPlaying)
                return;
            
            if (null == _cachedServicesName)
            {
                _cachedServicesName = GetServices();
                _foundServices = new List<IService>();

                int i = 0;
                foreach (string serviceFullName in _cachedServicesName)
                {
                    Type serviceType = Type.GetType(serviceFullName);
                    Type foundInterface = serviceType.GetInterfaces()
                        .FirstOrDefault(interfaceType => interfaceType.GetInterface("IService") != null);
                    if (null == foundInterface)
                        continue;

                    ServiceContainer.UseService(foundInterface, (implementation) => _foundServices.Add(implementation));
                }   
            }
        }
    }
}