using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager
{
    private readonly NodeBasedEditor nodeEditor;

    public SaveManager(NodeBasedEditor editor)
    {
        nodeEditor = editor;
    }

    public void SaveAllNodes()
    {
        if (nodeEditor.Nodes == null || nodeEditor.Nodes.Count <= 0)
        {
            Debug.LogError("Nothing to save");
            return;
        }

        DataClassWrap.ListDataClassWrapper list = new DataClassWrap.ListDataClassWrapper();
        list.dataList = new List<DataClassWrap.DataClassWrapper>();

        string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save combo file ", "Combo", "json", "Please enter a file name to save the combo to");

        nodeEditor.ComboName = Path.GetFileNameWithoutExtension(path);
        Debug.Log(path);

        foreach (Node node in nodeEditor.Nodes)
        {
            bool isParent = node.nodeInPoint.parentConnection == null;

            DataClassWrap.DataClassWrapper dataclass = new DataClassWrap.DataClassWrapper(new Vector2(node.rect.position.x, node.rect.position.y), node.title, node.id, isParent, node);
            list.dataList.Add(dataclass);
        }
        
        File.WriteAllText(path, JsonUtility.ToJson(list));

        Debug.Log("Node saved at : " + Application.dataPath + "/" + nodeEditor.ComboName + ".json");
    }

    public void LoadNode()
    {
        string filePath = UnityEditor.EditorUtility.OpenFilePanel("Select combo file", "", "json");
        
        if (filePath.Length != 0 && File.Exists(filePath))
        {
            Debug.Log("Load node from : " + filePath);
            nodeEditor.ComboName = Path.GetFileNameWithoutExtension(filePath);

            string dataAsJson = File.ReadAllText(filePath);
            ListDataClassWrapper loadedData = JsonUtility.FromJson<ListDataClassWrapper>(dataAsJson);

            nodeEditor.ResetNodes();

            foreach (DataClassWrapper dataWrapper in loadedData.dataList)
                nodeEditor.AddNode(dataWrapper.nodePosition, dataWrapper.nodeName, dataWrapper.id, dataWrapper.dataClass);

            foreach (DataClassWrapper dataWrapper in loadedData.dataList)
            {
                Node curNode = nodeEditor.GetNodeByID(dataWrapper.id);

                if (dataWrapper.lightComboId != "")
                {
                    Node targetNode = nodeEditor.GetNodeByID(dataWrapper.lightComboId);

                    curNode.lightVaraible.isActive = true;
                    curNode.lightVaraible.ShowOutPoint();
                    curNode.AddConnexion(targetNode.nodeInPoint, curNode.lightOutPoint);
                }

                if (dataWrapper.strongComboId != "")
                {
                    Node targetNode = nodeEditor.GetNodeByID(dataWrapper.strongComboId);

                    curNode.strongVaraible.isActive = true;
                    curNode.strongVaraible.ShowOutPoint();
                    curNode.AddConnexion(targetNode.nodeInPoint, curNode.strongOutPoint);
                }

                if (dataWrapper.specialComboId != "")
                {
                    Node targetNode = nodeEditor.GetNodeByID(dataWrapper.specialComboId);

                    curNode.specialVaraible.isActive = true;
                    curNode.specialVaraible.ShowOutPoint();
                    curNode.AddConnexion(targetNode.nodeInPoint, curNode.specialOutPoint);
                }

                if (dataWrapper.jumpComboId != "")
                {
                    Node targetNode = nodeEditor.GetNodeByID(dataWrapper.jumpComboId);

                    curNode.jumpVaraible.isActive = true;
                    curNode.jumpVaraible.ShowOutPoint();
                    curNode.AddConnexion(targetNode.nodeInPoint, curNode.jumpOutPoint);
                }
            }
        }
        else
        {
            Debug.LogWarning("No node data to load");
        }

    }
}

namespace DataClassWrap
{

[Serializable]
public class ListDataClassWrapper
{
    public List<DataClassWrapper> dataList;

    public DataClassWrapper FindDataClassById(string id)
    {
        return dataList.Find(x => x.id == id);
    }
}

[Serializable]
public class DataClassWrapper
{
    public DataClassWrapper(Vector2 nodePos, string name, string id, bool isParent, Node data)
    {
        nodePosition = nodePos;
        nodeName = name;
        isParentNode = isParent;
        this.id = id;

        DataClass dataclass = new DataClass((DataClass.InputType)data.inputDropdown.Value, data.stdDamageSlider.Value, data.stgDamageSlider.Value, data.aglDamageSlider.Value, data.lifeTimeSlider.Value, data.recoveryTimeSlider.Value);
        dataClass = dataclass;

        if (data.lightVaraible.outPoint.parentConnection != null)
            lightComboId = data.lightVaraible.outPoint.parentConnection.inPoint.node.id;

        if (data.strongVaraible.outPoint.parentConnection != null)
            strongComboId = data.strongVaraible.outPoint.parentConnection.inPoint.node.id;

        if (data.specialVaraible.outPoint.parentConnection != null)
            specialComboId = data.specialVaraible.outPoint.parentConnection.inPoint.node.id;

        if (data.jumpVaraible.outPoint.parentConnection != null)
            jumpComboId = data.jumpVaraible.outPoint.parentConnection.inPoint.node.id;
    }

    public Vector2 nodePosition;
    public String nodeName;
    public String id;
    public bool isParentNode;
    public DataClass dataClass;

    public String lightComboId;
    public String strongComboId;
    public String specialComboId;
    public String jumpComboId;
}
    
[Serializable]
public class DataClass
{
    public DataClass()
    {
    }

    public DataClass(InputType input, int standardDamage, int strongDamage, int agileDamage, float lifeTime, float recoveryTime)
    {
        this.input = input;
        this.standardDamage = standardDamage;
        this.strongDamage = strongDamage;
        this.agileDamage = agileDamage;
        this.lifeTime = lifeTime;
        this.recoveryTime = recoveryTime;
    }

    public enum InputType
    {
        None,
        Light,
        Strong,
        Special,
        Jump
    }

    public InputType input;
    public int standardDamage;
    public int strongDamage;
    public int agileDamage;

    public float lifeTime;
    public float recoveryTime;
}

}