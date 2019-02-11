using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
	[SerializeField]
	private Image portrait = null;
	[SerializeField]
    private Slider healthBar = null;
	[SerializeField]
	private Slider morphBar = null;
    [SerializeField]
    private Slider damageBar = null;

    private bool isAnimCoroutineStart = false;
    private bool isBlinkCoroutineStart = false;
    private bool isDamageBarAnimated = false;

    [SerializeField]
    private float animationSpeed = 0.5f;
    [SerializeField]
    private float damageBarTimer = 1.1f;
    [SerializeField]
    private float blinkTimer = 0.1f;

    [SerializeField]
    private int blinkPerHit = 3;

    Coroutine animCoroutine = null;

    private Player player;
    public Player Player
    {
		get { return player; }
        set
        {
			if (player)
			{
				player.OnTakeDamage -= UpdateHealthBar;
                player.OnTakeDamage -= Blink;
                player.OnHealth -= UpdateHealthBar;
                player.OnMorphUpdate -= UpdateMorphBar;
                player.OnDie -= UpdateDeathPlayer;
            }

            player = value;
            player.OnTakeDamage += UpdateHealthBar;
            player.OnTakeDamage += Blink;
            player.OnHealth += UpdateHealthBar;
			player.OnMorphUpdate += UpdateMorphBar;
            player.OnDie += UpdateDeathPlayer;
        }
    }

    public void FixedUpdate()
    {
        if (isDamageBarAnimated)
        {
            damageBar.value -= animationSpeed * Time.deltaTime;

            if (damageBar.value <= healthBar.value)
            {
                damageBar.value = healthBar.value;
                isDamageBarAnimated = false;
            }
        }
    }

    public void UpdateHealthBar()
    {
        if (player && player.MaxLife != 0)
            healthBar.value = player.CurrLife / (float)player.MaxLife;

        if (isAnimCoroutineStart)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(UpdateDamageBar());

        isAnimCoroutineStart = true;
        isDamageBarAnimated = false;
    }

	public void UpdateMorphBar()
	{
		if (player)
			morphBar.value = player.MorphTime / player.MorphDuration;
	}

    public void UpdateDeathPlayer()
    {
        healthBar.value = 0f;
        damageBar.value = 0f;
        morphBar.value = 0f;
    }

	public void ChangePortrait(Sprite sprite)
	{
		portrait.sprite = sprite;
	}

    private IEnumerator UpdateDamageBar()
    {
        yield return new WaitForSecondsPausable(damageBarTimer);
        isDamageBarAnimated = true;
        isAnimCoroutineStart = false;
    }

    private void Blink()
    {
        if (!isBlinkCoroutineStart)
        {
            StartCoroutine(BlinkCoroutine());
            isBlinkCoroutineStart = true;
        }
    }

    private IEnumerator BlinkCoroutine()
    {
        for (int idx = 0; idx < blinkPerHit; ++idx)
        {
            portrait.color = Color.red;
            yield return new WaitForSecondsPausable(blinkTimer);
            portrait.color = Color.white;
            yield return new WaitForSecondsPausable(blinkTimer);
        }
        isBlinkCoroutineStart = false;
    }
}
