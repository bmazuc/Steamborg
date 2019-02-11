using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField]
    private GameObject playerUIPrefab;

    [SerializeField]
    private Vector3 playerUIOffset;

	[SerializeField]
	private Sprite meleePortrait;
	[SerializeField]
	private Sprite rangedPortrait;

    private PlayerUI firstPlayerUI;
    private PlayerUI secondPlayerUI;
    private GameObject display;


	private void Start()
	{
		StartCoroutine(TryInit());
    }
	
	private void InstantiateSecondPlayerUI(bool bStart = false)
    {
        if (secondPlayerUI)
            return;

        secondPlayerUI = Instantiate(playerUIPrefab, transform).GetComponent<PlayerUI>();
        secondPlayerUI.transform.localPosition += playerUIOffset;
        secondPlayerUI.Player = GameManager.Instance.Player2;
		secondPlayerUI.ChangePortrait((secondPlayerUI.Player is PlayerMelee) ? meleePortrait : rangedPortrait);

		if (!bStart)
		{
			secondPlayerUI.UpdateHealthBar();
			secondPlayerUI.UpdateMorphBar();
		}

        display.SetActive(false);
    }

    public void HidePlayerUI()
    {
        if (firstPlayerUI)
            firstPlayerUI.gameObject.SetActive(false);
        if (secondPlayerUI)
            secondPlayerUI.gameObject.SetActive(false);
    }

    public void ShowPlayerUI()
    {
        if (firstPlayerUI)
            firstPlayerUI.gameObject.SetActive(true);
        if (secondPlayerUI)
            secondPlayerUI.gameObject.SetActive(true);
    }

    private void DestroySecondPlayerUI()
    {
        if (secondPlayerUI)
        {
            Destroy(secondPlayerUI.gameObject);
            display.SetActive(false);
        }
    }

	private IEnumerator TryInit()
	{
		GameManager gameManager = GameManager.Instance;

		while (!gameManager.isInit)
			yield return null;

		firstPlayerUI = Instantiate(playerUIPrefab, transform).GetComponent<PlayerUI>();
        firstPlayerUI.Player = gameManager.Player1;
        firstPlayerUI.ChangePortrait((firstPlayerUI.Player is PlayerMelee) ? meleePortrait : rangedPortrait);

		display = GameObject.Find("ConnectP2Display");

		if (gameManager.HasTwoPlayer)
			InstantiateSecondPlayerUI(true);
		else if (display)
			display.transform.localPosition += playerUIOffset;

		gameManager.OnPlayer2Connect += () => { InstantiateSecondPlayerUI(); };
		gameManager.OnPlayer2Disconnect += DestroySecondPlayerUI;
	}
}
