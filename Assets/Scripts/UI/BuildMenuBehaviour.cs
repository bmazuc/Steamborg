using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuBehaviour : MonoBehaviour
{
    [SerializeField]
    private Button LevelOneButton;
    [SerializeField]
    private Button LevelTwoButton;
    [SerializeField]
    private Button LevelThreeButton;
    [SerializeField]
    private Button LaunchGameButton;

    [SerializeField]
    private Toggle MeleePlayerToogle;
    [SerializeField]
    private Toggle DistancePlayerToogle;

    [SerializeField]
    private Button ReturnButton;

    void Start ()
    {
//#if UNITY_EDITOR
//        LaunchGameButton.gameObject.SetActive(false);
//#else
//        LevelOneButton.gameObject.SetActive(false);
//        LevelTwoButton.gameObject.SetActive(false);
//        LevelThreeButton.gameObject.SetActive(false);

//        Navigation navigationMelee = MeleePlayerToogle.navigation;
//        navigationMelee.selectOnDown = LaunchGameButton;
//        MeleePlayerToogle.navigation = navigationMelee;

//        Navigation navigationDistance = DistancePlayerToogle.navigation;
//        navigationDistance.selectOnDown = LaunchGameButton;
//        DistancePlayerToogle.navigation = navigationDistance;


//        Navigation navigationReturn = ReturnButton.navigation;
//        navigationReturn.selectOnUp = LaunchGameButton;
//        ReturnButton.navigation = navigationReturn;
//#endif
    }
}
