using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    //public DataClassWrapper(Vector2 nodePos, string name, string id, bool isParent, Node data)
    //{
    //    nodePosition = nodePos;
    //    nodeName = name;
    //    isParentNode = isParent;
    //    this.id = id;

    //    DataClass dataclass = new DataClass((DataClass.InputType)data.inputDropdown.Value, data.stdDamageSlider.Value, data.stgDamageSlider.Value, data.aglDamageSlider.Value, data.lifeTimeSlider.Value, data.recoveryTimeSlider.Value);
    //    dataClass = dataclass;

    //    if (data.lightVaraible.outPoint.parentConnection != null)
    //        lightComboId = data.lightVaraible.outPoint.parentConnection.inPoint.node.id;

    //    if (data.strongVaraible.outPoint.parentConnection != null)
    //        strongComboId = data.strongVaraible.outPoint.parentConnection.inPoint.node.id;

    //    if (data.specialVaraible.outPoint.parentConnection != null)
    //        specialComboId = data.specialVaraible.outPoint.parentConnection.inPoint.node.id;

    //    if (data.jumpVaraible.outPoint.parentConnection != null)
    //        jumpComboId = data.jumpVaraible.outPoint.parentConnection.inPoint.node.id;
    //}

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