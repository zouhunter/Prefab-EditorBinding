using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Rotorz.ReorderableList;
using System;

public class PrefabBindingWindow : EditorWindow
{
    public class ObjHolder : IComparable<ObjHolder>
    {
        public string name;
        public int index;
        public GameObject obj;
        public static int id;
        private ObjHolder() { }
        public static ObjHolder SpanObj()
        {
            var objh = new ObjHolder();
            objh.index = id++;
            return objh;
        }

        public int CompareTo(ObjHolder obj)
        {
            if (obj.index > index)
            {
                return -1;
            }
            else if (obj.index < index)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    PrefabBindingObj obj;
    string[] switchOption = new string[] { "信息记录", "信息加载" };
    int selected = 0;
    Vector2 viewScrop;
    public List<ObjHolder> objectList = new List<ObjHolder>();
    SerializedObject serializedObj;
    SerializedProperty scriptProp;
    private void OnEnable()
    {
        serializedObj = new SerializedObject(this);
        scriptProp = serializedObj.FindProperty("m_Script");
        ObjHolder.id = 0;
        if (obj) objectList = LoadObjectList(obj);
    }



    public void OnGUI()
    {
        EditorGUILayout.PropertyField(scriptProp);

        serializedObj.Update();
        selected = GUILayout.Toolbar(selected, switchOption);
        var newObj = EditorGUILayout.ObjectField(obj, typeof(PrefabBindingObj), false) as PrefabBindingObj;

        if (newObj != obj && newObj != null)
        {
            obj = newObj;
            objectList = LoadObjectList(obj);
        }

        if (obj != null)
        {
            using (var ver = new EditorGUILayout.ScrollViewScope(viewScrop, GUILayout.Height(300)))
            {
                viewScrop = ver.scrollPosition;
                DrawListView();
            }
            DrawOptionButtons();
        }
        else
        {
            EditorGUI.HelpBox(GUILayoutUtility.GetRect(0, 40), "请先放置信息保存对象", MessageType.Error);
            if (GUILayout.Button("创建"))
            {
                obj = PrefabBindingUtility.CreatePreabBindingObj();
            }
        }
        serializedObj.ApplyModifiedProperties();
    }
    private void DrawListView()
    {
        if (objectList.Count == 0)
        {
            ObjHolder.id = 0;
        }
        ReorderableListGUI.Title("相关对象列表");
        ReorderableListGUI.ListField<ObjHolder>(objectList, DrawItem, EditorGUIUtility.singleLineHeight);
    }

    private ObjHolder DrawItem(Rect position, ObjHolder item)
    {
        if (item == null)
        {
            item = ObjHolder.SpanObj();
        }
        using (var hor = new EditorGUILayout.HorizontalScope())
        {
            var rect = new Rect(position.x, position.y, position.width * 0.3f, EditorGUIUtility.singleLineHeight);
            item.index = EditorGUI.IntField(rect, item.index);
            rect.x += 0.32f * position.width;
            item.name = EditorGUI.TextField(rect, item.name);
            rect.x += 0.32f * position.width;
            item.obj = EditorGUI.ObjectField(rect, item.obj, typeof(GameObject), true) as GameObject;

            if (string.IsNullOrEmpty(item.name) && item.obj != null)
            {
                item.name = item.obj.name;
            }
        }
        return item;
    }

    private void DrawOptionButtons()
    {
        var rect = new Rect(0, position.height - EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
        if (selected == 0)
        {
            if (GUI.Button(rect, "解析对象信息"))
            {
                bool ok = EditorUtility.DisplayDialog("保存到数据源", "点击确定完成预制体信息记录", "确定");
                if (ok)
                {
                    obj.bindingItems.Clear();
                    PrefabBindingUtility.ExargeRentItems(obj.bindingItems, objectList);
                    EditorUtility.SetDirty(obj);
                }
            }
        }
        else
        {
            if (GUI.Button(rect, "加载对象信息"))
            {
                bool ok = EditorUtility.DisplayDialog("从数据中加载", "点击确定完成预制体信息加载", "确定");
                if(ok) PrefabBindingUtility.InstallInfomation(objectList, obj.bindingItems);
            }
        }
    }
    private static List<ObjHolder> LoadObjectList(PrefabBindingObj obj)
    {
        var list = new List<ObjHolder>();
        foreach (var item in obj.bindingItems)
        {
            if (list.Find(x => x.index == item.index) == null)
            {
                var objholder = ObjHolder.SpanObj();
                objholder.name = item.name;
                objholder.index = item.index;
                list.Add(objholder);
            }
        }
        list.Sort();
        return list;
    }
}
