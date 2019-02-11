using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class MarkSystem : MonoBehaviour, IPausable
{
	[SerializeField]
	private Mark[] marksLevels = new Mark[5];
	[SerializeField]
	private float markDuration = 1f;
	[SerializeField]
	private float aoeDuration = 1f;
	[SerializeField]
	private ParticleSystem explosionFX = null;

	private int markLevel = -1;
	private float timer;

	private BoxCollider collid = null;
	private Enemy enemy = null;

	private PlayerRanged player;
	private Player.ARCHETYPE detonation = Player.ARCHETYPE.Standard;

	private bool isDetonate = false;
    public bool IsDetonate { get { return isDetonate; } }

    // For debug text
    private bool showDebugText = false;
	private GUIStyle guiStyle = new GUIStyle();

    [System.Serializable]
	struct Mark
	{
		public int damage;
		public CrowdControl.CONTROL_TYPE crowdControl;
		public float effectModifier;
		public ParticleSystem markFX;
	}

	public System.Action<MarkSystem> OnDestroyed;

    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    private void Start()
	{
	    guiStyle.fontSize = 24;
	    guiStyle.normal.textColor = Color.white;

        markLevel = 0;
		timer = markDuration;

        collid = GetComponent<BoxCollider>();
		enemy = GetComponentInParent<Enemy>();

        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.AddPausable(this);

		marksLevels[0].markFX.gameObject.SetActive(true);

        particleSystems.AddRange(GetComponents<ParticleSystem>());
        particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>(true));
    }

	private void Update()
	{
		if (detonation == Player.ARCHETYPE.Standard)
		{
			timer -= Time.deltaTime;
			if (timer <= 0f)
			{
				MarkExpire();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Enemy" && other.gameObject != enemy.gameObject)
		{
			Enemy otherEnemy = other.GetComponent<Enemy>();
			if (detonation == Player.ARCHETYPE.Strong)
				ApplyMarkEffect();
			else
				player.ApplyMark(otherEnemy);
		}
	}

    void OnDestroy()
    {
        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.RemovePausable(this);
    }

	public void DetonateMarks(Player.ARCHETYPE detonationType, PlayerRanged _player)
	{
	    isDetonate = true;

		if (explosionFX != null)
			explosionFX.gameObject.SetActive(true);

		detonation = detonationType;

		if (detonationType == Player.ARCHETYPE.Strong || detonationType == Player.ARCHETYPE.Agile)
			collid.enabled = true;

		player = _player;

        ApplyMarkEffect();
		MarkExpire();
	}

	public void NextLevel()
	{
		if (detonation == Player.ARCHETYPE.Standard)
		{
			markLevel = Mathf.Min(++markLevel, marksLevels.Length - 1);
			timer = markDuration;

			marksLevels[markLevel].markFX.gameObject.SetActive(true);
			marksLevels[markLevel - 1].markFX.gameObject.SetActive(false);
		}
	}

	public void MarkExpire()
	{
	    HideAllMarks();

        if (isDetonate)
		{
			if (detonation == Player.ARCHETYPE.Standard)
				Invoke("Delete", explosionFX.main.duration);
			else
				Invoke("Delete", Mathf.Max(explosionFX.main.duration, aoeDuration));
		}
		else
			Delete();

		if (OnDestroyed != null)
			OnDestroyed(this);
	}

	private void ApplyMarkEffect()
	{
		switch (marksLevels[markLevel].crowdControl)
		{
			case CrowdControl.CONTROL_TYPE.KnockBack:
				enemy.CCMgr.KnockBack((enemy.transform.position - player.transform.position).normalized, marksLevels[markLevel].effectModifier);
				break;
			case CrowdControl.CONTROL_TYPE.KnockDown:
				enemy.CCMgr.KnockDown(marksLevels[markLevel].effectModifier);
				break;
			default:
				break;
		}

		enemy.TakeDamage(marksLevels[markLevel].damage);
	}

	private void Delete()
	{
		Destroy(gameObject);
	}

    private void HideAllMarks()
    {
        for (int i = 0; i < 5; i++)
        {
            marksLevels[i].markFX.gameObject.SetActive(false);
        }
    }

    private void OnGUI()
    {
        if (showDebugText)
        {
            Vector3 screenpos = Camera.main.WorldToScreenPoint(transform.position);
            GUI.Label(new Rect(screenpos.x - 160f, Screen.height - (screenpos.y + 135f), 250f, 250f), "Mark level : " + (markLevel + 1).ToString() + " | Timer : " + timer, guiStyle);

            if (isDetonate)
                GUI.Label(new Rect(screenpos.x - 160f, Screen.height - (screenpos.y + 155f), 250f, 250f), "Mark detonate", guiStyle);
        }
    }

    public void OnPause()
    {
        enabled = false;
        foreach (ParticleSystem sys in particleSystems)
            sys.Pause();
    }

    public void OnResume()
    {
        enabled = true;
        foreach (ParticleSystem sys in particleSystems)
            sys.Play();
    }
}
