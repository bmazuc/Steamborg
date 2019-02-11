using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BTMCombo
{

[Serializable]
public class ComboAction
{
    private String id;
    public String Id { get { return id; } set { id = value; } }

    public ComboBrain.InputType inputType;
    public int standardDamage;
    public int strongDamage;
    public int agileDamage;

    public float lifeTime;
    public float recoveryTime;
    private readonly float createdTime;
    public float CreatedTime
    {
        get { return createdTime; }
    }

    public ComboAction lightAction;
    public ComboAction strongAction;
    public ComboAction SpecialAction;
    public ComboAction JumpAction;
        
    public ComboAction()
    {
    }
        
    public ComboAction(ComboBrain.InputType input, float lTime, float reTime, float oTime)
    {
        inputType = input;
        lifeTime = lTime;
        recoveryTime = reTime;
        createdTime = oTime;
    }

    public ComboAction(ComboAction action, float cTime)
    {
        inputType = action.inputType;
        standardDamage = action.standardDamage;
        agileDamage = action.agileDamage;
        strongDamage = action.strongDamage;
        lifeTime = action.lifeTime;
        recoveryTime = action.recoveryTime;

        lightAction = action.lightAction;
        strongAction = action.strongAction;
        SpecialAction = action.SpecialAction;
        JumpAction = action.JumpAction;

        createdTime = cTime;
    }

    public bool CanChangeCombo()
    {
        return Time.time < CreatedTime + lifeTime;
    }

    public bool ComboTimeOut()
    {
        return Time.time > CreatedTime + lifeTime;
    }

    public bool RecoveryTime()
    {
        return (Time.time > createdTime + recoveryTime) && (Time.time < CreatedTime + lifeTime);
    }
}


public class ComboBrain : MonoBehaviour
{
    public enum InputType
    {
        None,
        Light,
        Strong,
        Special,
        Jump
    }

	public Action OnComboReset;

    private List<ComboAction> comboList;

    public TextAsset comboFile;

    private ComboAction currentAction;
    private PlayerMelee player;

    // For debug text
    [SerializeField]
    private bool showDebugText = false;
        private GUIStyle guiStyle = new GUIStyle();

        #region Mono

    void Start ()
    {
        comboList = new List<ComboAction>();
        ReadComboFile();

        ResetCombo();

	    guiStyle.fontSize = 24;
	    guiStyle.normal.textColor = Color.white;
        player = GetComponent<PlayerMelee>();
	}
        
    void Update()
    {
        if (currentAction.inputType != InputType.None && currentAction.ComboTimeOut())
        {
            ResetCombo();

			if (OnComboReset != null)
				OnComboReset();
        }
    }

    #endregion // Mono
        
    private ComboAction FindBaseCombo(InputType type)
    {
        if (comboList.Count > 0)
            return comboList.Find(x => x.inputType == type);
        else
            return null;
    }

    private ComboAction FindComboInList(List<ComboAction> list, InputType type)
    {
        return list.Find(x => x.inputType == type);
    }

    private ComboAction FindNextCombo(ComboAction combo, InputType type)
    {
        switch (type)
        {
            case InputType.Light:
                if (combo.lightAction != null)
                    return combo.lightAction;
                break;
            case InputType.Strong:
                if (combo.strongAction != null)
                    return combo.strongAction;
                break;
            case InputType.Special:
                if (combo.SpecialAction != null)
                    return combo.SpecialAction;
                break;
            case InputType.Jump:
                if (combo.JumpAction != null)
                    return combo.JumpAction;
                break;
        }

        return null;
    }

    private int GetDamage(ComboAction currentCombo, Player.ARCHETYPE archetype)
    {
        int damage = 0;

        switch (archetype)
        {
            case Player.ARCHETYPE.Standard:
                damage = currentAction.standardDamage;
                break;
            case Player.ARCHETYPE.Strong:
                damage = currentAction.strongDamage;
                break;
            case Player.ARCHETYPE.Agile:
                damage = currentAction.agileDamage;
                break;
            default:
                break;
        }

        return damage;
    }

    public void SendAction(InputType inputType, Player.ARCHETYPE archetype)
    {
        if (currentAction.inputType == InputType.None)
        {
            currentAction = new ComboAction(FindBaseCombo(inputType), Time.time);


            LaunchPlayerAction(currentAction.inputType, GetDamage(currentAction, archetype));
        }
        else
        {
			if (inputType == InputType.Special)
				return;

            ComboAction nextCombo = FindNextCombo(currentAction, inputType);

            if (nextCombo != null)
            {
                if (currentAction.RecoveryTime())
                {
                    currentAction = new ComboAction(nextCombo, Time.time);
                    LaunchPlayerAction(currentAction.inputType, GetDamage(currentAction, archetype));
                }
            }
        }
    }
	
    public void LaunchPlayerAction(InputType type, int damage)
    {
        switch (type)
        {
            case InputType.None:
                break;
            case InputType.Light:
                player.LightAttack_Archetype(damage);
                break;
            case InputType.Strong:
                player.StrongAttack_Archetype(damage);
                break;
            case InputType.Special:
                player.SpecialAttack_Archetype(damage);
                break;
            case InputType.Jump:
                player.Jump();
                break;
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }
    }


    private void ResetCombo()
    {
        currentAction = new ComboAction(InputType.None, 0f, 0f, 0f);
    }

    private void ReadComboFile()
    {
        string jsonString = comboFile.text;
        
        ListDataClassWrapper loadedData = JsonUtility.FromJson<ListDataClassWrapper>(jsonString);

        List<ComboAction> tempComboList = new List<ComboAction>();

        foreach (DataClassWrapper dataWrapper in loadedData.dataList)
        {
            ComboAction combo = new ComboAction
            {
                Id = dataWrapper.id,
                standardDamage = dataWrapper.dataClass.standardDamage,
                strongDamage = dataWrapper.dataClass.strongDamage,
                agileDamage = dataWrapper.dataClass.agileDamage,
                lifeTime = dataWrapper.dataClass.lifeTime,
                recoveryTime = dataWrapper.dataClass.recoveryTime
            };

            combo.inputType = (InputType) dataWrapper.dataClass.input;

            tempComboList.Add(combo);
        }
        
        foreach (DataClassWrapper dataWrapper in loadedData.dataList)
        {
            ComboAction curNode = FindComboById(tempComboList, dataWrapper.id);

            if (dataWrapper.lightComboId != "")
            {
                ComboAction targetNode = FindComboById(tempComboList, dataWrapper.lightComboId);
                curNode.lightAction = targetNode;
            }
            if (dataWrapper.strongComboId != "")
            {
                ComboAction targetNode = FindComboById(tempComboList, dataWrapper.strongComboId);
                curNode.strongAction = targetNode;
            }
            if (dataWrapper.specialComboId != "")
            {
                ComboAction targetNode = FindComboById(tempComboList, dataWrapper.specialComboId);
                curNode.SpecialAction = targetNode;
            }
            if (dataWrapper.jumpComboId != "")
            {
                ComboAction targetNode = FindComboById(tempComboList, dataWrapper.jumpComboId);
                curNode.JumpAction = targetNode;
            }

            if (dataWrapper.isParentNode)
                comboList.Add(curNode);
        }
        
     }

    private ComboAction FindComboById(List<ComboAction> list, String id)
    {
        return list.Find(x => x.Id == id);
    }

    void OnGUI()
    {
        float yPos = 25;

        if (showDebugText)
            GUI.Label(new Rect(15, yPos, 300, 20), "ComboType : " + currentAction.inputType + " | Damage : " + currentAction.standardDamage + " | Time : " + currentAction.CreatedTime, guiStyle);
            
        //if (currentAction.actionCombo != null)
        //{
        //    yPos += 15f;

        //    foreach (ComboAction comboChild in currentAction.actionCombo)
        //    {
        //        GUI.Label(new Rect(20, yPos, 300, 20), "ComboType : " + comboChild.inputType + " | Damage : " + comboChild.damage + " | Time : " + comboChild.CreatedTime);

        //        yPos += 15f;
        //    }
        //}
    }


}

} // namespace BTMCombo