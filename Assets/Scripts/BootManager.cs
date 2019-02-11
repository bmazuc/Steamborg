using UnityEngine;


public class BootManager : MonoBehaviour
{
    [SerializeField]
    private GameObject gameMgrPrefab = null;
	[SerializeField]
	private GameObject transitionScreenPrefab = null;
    [SerializeField]
    private GameObject controllerRefocusPrefab = null;

    static bool isLoaded = false;

    [SerializeField]
    private UnityEngine.UI.GraphicRaycaster graphicRaycaster = null;

#if !UNITY_EDITOR
    private void Awake()
	{
		Cursor.visible = false;
        if (graphicRaycaster)
            graphicRaycaster.enabled = false;
    }
#endif

	private void Start()
    {
        if (!isLoaded)
        {
			isLoaded = true;

			GameObject gameMgr = Instantiate(gameMgrPrefab);
            DontDestroyOnLoad(gameMgr);

			GameObject transitionScreen = Instantiate(transitionScreenPrefab);
			DontDestroyOnLoad(transitionScreen);

            GameObject controllerRefocus = Instantiate(controllerRefocusPrefab);
            DontDestroyOnLoad(controllerRefocus);
        }
    }
}
