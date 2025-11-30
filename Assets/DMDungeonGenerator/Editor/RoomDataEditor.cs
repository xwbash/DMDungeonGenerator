using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using DMDungeonGenerator;

[RequireComponent(typeof(RoomData))]
[CustomEditor(typeof(RoomData))]
public class RoomDataEditor : Editor
{
    public static EditingMode mode = EditingMode.None;
    private bool invert = false;
    private bool doEntireRow = false;

    public override void OnInspectorGUI()
    {
        RoomData data = (RoomData)target;

        mode = (EditingMode)EditorGUILayout.EnumPopup("EditingMode: ", mode);
        EditorGUILayout.LabelField("Transparency: ");
        data.debugTransparency = EditorGUILayout.Slider(data.debugTransparency, 0f, 0.25f);

        base.OnInspectorGUI();
    }

    protected virtual void OnSceneGUI()
    {
        RoomData data = (RoomData)target;

        if (mode != EditingMode.None)
        {
            if (Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
        }

        if (!RoomData.DrawVolumes)
            return;

        List<Voxel> vox = data.LocalVoxels;
        float voxelScale = DungeonGenerator.voxelScale;

        for (int i = 0; i < vox.Count; i++)
        {
            Handles.color = new Color(1f, 1f, 1f, data.debugTransparency);
            Vector3 pos = data.transform.TransformPoint(vox[i].position * voxelScale);

            Handles.CubeHandleCap(
                0,
                pos,
                data.transform.rotation,
                voxelScale,
                EventType.Repaint
            );
        }

        for (int i = 0; i < data.Doors.Count; i++)
        {
            Handles.color = new Color(1f, 0f, 0f, 0.4f);
            Vector3 pos = (data.Doors[i].position +
                           data.Doors[i].direction * 0.5f +
                           Vector3.down * 0.25f) * voxelScale;

            Vector3 worldPos = data.transform.TransformPoint(pos);
            Handles.Label(worldPos, i.ToString());

            float s = HandleUtility.GetHandleSize(worldPos) * 0.4f;

            if (mode != EditingMode.Doors)
            {
                Handles.CubeHandleCap(0, worldPos, data.transform.rotation, s, EventType.Repaint);
            }
            else
            {
                if (Handles.Button(worldPos, data.transform.rotation, s, s, Handles.CubeHandleCap))
                {
                    ClickedDoorHandle(i);
                }
            }
        }

        switch (mode)
        {
            case EditingMode.Voxels:
            {
                var targetVoxel = GetClosestVoxel(data);
                if (targetVoxel != null)
                    DrawVoxelEditingHandlesForSingleVoxel(targetVoxel);
                break;
            }

            case EditingMode.Doors:
            {
                var targetVoxel = GetClosestVoxel(data); 
                if (targetVoxel != null)
                    DrawDoorEditingHandlesForSingleVoxel(targetVoxel);
                break;
            }

        }
    }
    
    private void DrawDoorEditingHandlesForSingleVoxel(Voxel v)
    {
        Vector3 pos = v.position;

        Handles.color = Color.cyan;
        DrawDoorButtonArrow(pos, Vector3.forward, 0.4f, 0f, 1f);
        DrawDoorButtonArrow(pos, Vector3.back, 0.4f, 0f, 1f);
        DrawDoorButtonArrow(pos, Vector3.left, 0.4f, 0f, 1f);
        DrawDoorButtonArrow(pos, Vector3.right, 0.4f, 0f, 1f);
    }

    
    private void DrawVoxelEditingHandlesForSingleVoxel(Voxel v)
    {
        Vector3 pos = v.position;

        invert = Event.current.shift;
        doEntireRow = Event.current.control;

        float iS = invert ? -1f : 1f;
        float iO = invert ? 0.5f : 0f;

        Handles.color = doEntireRow ? new Color(0.3f, 0.3f, 1f) : Color.blue;
        DrawArrowButton(pos, Vector3.forward, 0.4f, iO, iS);
        DrawArrowButton(pos, Vector3.back, 0.4f, iO, iS);

        Handles.color = doEntireRow ? new Color(0.3f, 1f, 0.3f) : Color.green;
        DrawArrowButton(pos, Vector3.up, 0.4f, iO, iS);
        DrawArrowButton(pos, Vector3.down, 0.4f, iO, iS);

        Handles.color = doEntireRow ? new Color(1f, 0.3f, 0.3f) : Color.red;
        DrawArrowButton(pos, Vector3.left, 0.4f, iO, iS);
        DrawArrowButton(pos, Vector3.right, 0.4f, iO, iS);
    }


    private void DrawVoxelEditingHandles(List<Voxel> vox)
    {
        RoomData data = (RoomData)target;

        var voxCopy = vox.ToList();

        foreach (var v in voxCopy)
        {
            Vector3 pos = v.position;

            invert = Event.current.shift;
            doEntireRow = Event.current.control;

            float iS = invert ? -1f : 1f;
            float iO = invert ? 0.5f : 0f;

            Handles.color = doEntireRow ? new Color(0.3f, 0.3f, 1f) : Color.blue;
            DrawArrowButton(pos, Vector3.forward, 0.4f, iO, iS);
            DrawArrowButton(pos, Vector3.back, 0.4f, iO, iS);

            Handles.color = doEntireRow ? new Color(0.3f, 1f, 0.3f) : Color.green;
            DrawArrowButton(pos, Vector3.up, 0.4f, iO, iS);
            DrawArrowButton(pos, Vector3.down, 0.4f, iO, iS);

            Handles.color = doEntireRow ? new Color(1f, 0.3f, 0.3f) : Color.red;
            DrawArrowButton(pos, Vector3.left, 0.4f, iO, iS);
            DrawArrowButton(pos, Vector3.right, 0.4f, iO, iS);
        }
    }


    private void DrawDoorEditingHandles(List<Voxel> vox)
    {
        RoomData data = (RoomData)target;

        foreach (var v in vox)
        {
            Vector3 pos = v.position;

            Handles.color = Color.cyan;
            DrawDoorButtonArrow(pos, Vector3.forward, 0.4f, 0f, 1f);
            DrawDoorButtonArrow(pos, Vector3.back, 0.4f, 0f, 1f);
            DrawDoorButtonArrow(pos, Vector3.left, 0.4f, 0f, 1f);
            DrawDoorButtonArrow(pos, Vector3.right, 0.4f, 0f, 1f);
        }
    }

    private void DrawArrowButton(Vector3 pos, Vector3 dir, float handleSize, float iO, float iS)
    {
        RoomData rd = (RoomData)target;
        float voxelScale = DungeonGenerator.voxelScale;

        Vector3 worldPos = rd.transform.TransformPoint(pos * voxelScale + dir * (iO + 0.5f) * voxelScale);
        float size = HandleUtility.GetHandleSize(worldPos) * handleSize;

        if (IsVoxelEmpty(pos + dir))
        {
            if (Handles.Button(worldPos,
                               rd.transform.rotation * Quaternion.LookRotation(dir * iS),
                               size,
                               size,
                               Handles.ArrowHandleCap))
            {
                ClickedArrowHandle(pos, dir);
            }
        }
    }

    private void DrawDoorButtonArrow(Vector3 pos, Vector3 dir, float handleSize, float iO, float iS)
    {
        RoomData rd = (RoomData)target;
        float voxelScale = DungeonGenerator.voxelScale;

        Vector3 worldPos = rd.transform.TransformPoint(pos * voxelScale + dir * (iO + 0.5f) * voxelScale);
        float size = HandleUtility.GetHandleSize(worldPos) * handleSize;

        if (IsVoxelEmpty(pos + dir) && IsDoorEmpty(pos, dir))
        {
            if (Handles.Button(worldPos,
                               rd.transform.rotation * Quaternion.LookRotation(dir * iS),
                               size,
                               size,
                               Handles.ArrowHandleCap))
            {
                ClickedArrowHandleDoor(pos, dir);
            }
        }
    }

    private void ClickedArrowHandle(Vector3 pos, Vector3 dir)
    {
        RoomData rd = (RoomData)target;

        if (invert)
        {
            if (doEntireRow)
            {
                var list = rd.LocalVoxels
                    .Where(v => Vector3.Scale(v.position, dir) == Vector3.Scale(pos, dir))
                    .Select(v => v.position)
                    .ToList();

                foreach (var p in list)
                    rd.RemoveVoxel(p);
            }
            else
            {
                rd.RemoveVoxel(pos);
            }
        }
        else
        {
            if (doEntireRow)
            {
                var list = rd.LocalVoxels
                    .Where(v => Vector3.Scale(v.position, dir) == Vector3.Scale(pos, dir))
                    .Select(v => v.position)
                    .ToList();

                foreach (var p in list)
                    if (IsVoxelEmpty(p + dir))
                        rd.AddVoxel(p, dir);
            }
            else
            {
                rd.AddVoxel(pos, dir);
            }
        }

        EditorUtility.SetDirty(target);
    }

    Voxel GetClosestVoxel(RoomData data)
    {
        float minDist = float.MaxValue;
        Voxel closest = null;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        foreach (var v in data.LocalVoxels)
        {
            float dist = HandleUtility.DistanceToCircle(
                data.transform.TransformPoint(v.position * DungeonGenerator.voxelScale), 
                0.1f
            );

            if (dist < minDist)
            {
                minDist = dist;
                closest = v;
            }
        }

        return closest;
    }

    private void ClickedArrowHandleDoor(Vector3 pos, Vector3 dir)
    {
        ((RoomData)target).AddDoor(pos, dir);
        EditorUtility.SetDirty(target);
    }

    public void ClickedDoorHandle(int index)
    {
        ((RoomData)target).RemoveDoor(index);
        EditorUtility.SetDirty(target);
    }

    private bool IsVoxelEmpty(Vector3 voxelPosition)
    {
        RoomData obj = (RoomData)target;
        return !obj.LocalVoxels.Any(v => v.position == voxelPosition);
    }

    private bool IsDoorEmpty(Vector3 pos, Vector3 dir)
    {
        RoomData obj = (RoomData)target;
        return !obj.Doors.Any(d => d.position == pos && d.direction == dir);
    }

    public enum EditingMode
    {
        None = 0,
        Voxels = 1,
        Doors = 2
    }
}
