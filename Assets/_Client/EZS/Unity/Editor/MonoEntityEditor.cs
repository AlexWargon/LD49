using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = System.Object;

namespace Wargon.ezs.Unity {

    [CustomEditor(typeof(MonoEntity), true), CanEditMultipleObjects]
    public class MonoEntityEditor : Editor 
    {
        private MonoEntity monoEntity;
        private MonoEntity[] manyEntities = new MonoEntity[16];
        private bool flowed = true;
        private GUIContent addComponentText;
        private GUIStyle addComponentButtonStyle;
        private Rect addButtonRect;
        private bool editMany;
        private int entitiesCount;
        private void Awake() 
        {
            EntityGUI.Init();
            ComponentTypesList.Init();
        }

        // private MonoEntity[] FindEntitiesWithSameComponents(Object[] targets)
        // {
        //     var entities = targets.Select(s1 =>(s1 as MonoEntity).Components).ToList().Intersect()
        //     var mono = new MonoEntity[entitiesCount];
        //     var index = 0;
        //     var entityFromTargets1 = targets[0];
        //     for (var i = 1; i < entitiesCount; i++)
        //     {
        //         var entityFromTargets1 = targets[i];
        //         for (var j = 0; j < entitiesCount; j++)
        //         {
        //             var entityFromTargets2 = targets[j];
        //             
        //             
        //         }
        //     }
        //
        // }
        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();
            editMany = targets.Length > 1;
            entitiesCount = targets.Length;
            for (var i = 0; i < entitiesCount; i++)
            {
                manyEntities[i] = (MonoEntity) targets[i];
            }
            monoEntity = (MonoEntity)target;
            
            addComponentText = new GUIContent("Add Component");
            addComponentButtonStyle = GUI.skin.button;
            EditorGUI.BeginChangeCheck();

            if (monoEntity.runTime)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("Kill Entity"),GUILayout.Width(154), GUILayout.Height(24)))
                    monoEntity.Entity.Destroy();
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                monoEntity.destroyComponent = EditorGUILayout.Toggle("Destroy MonoBeh", monoEntity.destroyComponent);
                monoEntity.destroyObject = EditorGUILayout.Toggle("Destroy GO", monoEntity.destroyObject);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Run Time", monoEntity.runTime ? "✔" : "✘", EditorStyles.largeLabel);
            EditorGUILayout.LabelField(monoEntity.runTime ? $"ID:{monoEntity.Entity.id.ToString()}" : "ID:X");
            EditorGUILayout.EndHorizontal();
            
            
            EntityGUI.Vertical(GUI.skin.box, () =>
            {
                if (monoEntity.runTime)
                    if (monoEntity.Entity.IsDead())
                    {
                        EditorGUILayout.LabelField("ENTITY DEAD", EditorStyles.whiteLargeLabel);
                        return;
                    }
                EntityGUI.Horizontal(() =>
                {
                    flowed = EditorGUILayout.Foldout(flowed, $"ECS Components [{monoEntity.ComponentsCount.ToString()}]");
                    
                    if (GUILayout.Button(new GUIContent("Clear", "Remove All Components")))
                        RemoveAll();
                });
                if (flowed)
                {
                    addButtonRect = GUILayoutUtility.GetRect(addComponentText, addComponentButtonStyle);
                    if (GUI.Button(addButtonRect, addComponentText, addComponentButtonStyle))
                    {
                        addButtonRect.y -= 20f;
                        PopupWindow.Show(addButtonRect, new ComponentSearchPopup(AddComponent, manyEntities, entitiesCount));
                    }
                    
                    DrawComponents();
                }

            });
            EditorGUILayout.LabelField($"EZS", EditorStyles.whiteMiniLabel);

            EditorUtility.SetDirty(monoEntity);
                
        }

        
        private object NewObject(Type type) {
            return Activator.CreateInstance(type);
        }

        private Type GetComponentType(string typeName) {
            //return Type.GetType(type + ",Assembly-CSharp", true);
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => Assembly.Load(x.FullName))
                .Select(x => x.GetType(typeName))
                .FirstOrDefault(x => x != null);
            
        }

        private void RemoveAll() {
            monoEntity.Components.Clear();
        }

        private void AddComponent(string componentName, MonoEntity entity) {
            if (entity.runTime)
                AddComponentRuntime(componentName,entity);
            else
                AddComponentEditor(componentName,entity);
        }
        
        private void AddComponentEditor(string componentName, MonoEntity entity)
        {
            var type = GetComponentType(componentName);
            if (entity.Components.HasType(type)) {
                return;
            }

            var resolver = NewObject(type);
            entity.Components.Add(resolver);
        }
        
        private void AddComponentRuntime(string componentName, MonoEntity entity)
        {
            var type = GetComponentType(componentName);
            if (entity.Entity.GetEntityData().componentTypes.Contains(ComponentTypeMap.GetID(type))) {
                Debug.LogError($"ENTITY ALREADY HAS '{type}' COMPONENT");
                return;
            }
            
            var component= NewObject(type);
            entity.Entity.AddBoxed(component);
            entity.Components.Add(component);
        }

        private void DrawComponents() {
            for (var index = 0; index < monoEntity.ComponentsCount; index++)
                ComponentInspector.DrawComponentBox(monoEntity, index, manyEntities, entitiesCount);
        }
    }
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
    public static class ListExtension
    {
        public static bool HasType(this System.Collections.IList list, Type whatHas)
        {
            var i = 0;
            var count = list.Count;
            for (i = 0; i < count; i++)
                if (list[i].GetType() == whatHas)
                    return true;
            return false;
        }
    }
}

