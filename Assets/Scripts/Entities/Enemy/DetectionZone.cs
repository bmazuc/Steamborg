using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DetectionZone : MonoBehaviour
{
	[SerializeField]
	private GameObject brain = null;

    private bool fadeOutOnAwake = false;

    private Animator enemyAnimator;

	private int numberOfPlayer = 0;


    private void Awake()
    {
        enemyAnimator = brain.transform.parent.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
	{
	    if (other.tag == "Player")
	    {
            if (numberOfPlayer == 0)
            {
                enemyAnimator.SetBool("isBrainActive", true);
                brain.SetActive(true);

                if (fadeOutOnAwake)
                    enemyAnimator.Play("FadeIn");
                
            }

			++numberOfPlayer;
        }
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
            Debug.Log("FadeOut");
			--numberOfPlayer;

			if (numberOfPlayer == 0)
			{
				GetComponentInParent<Enemy>().Move(Vector3.zero);

				enemyAnimator.SetBool("isBrainActive", false);
				//enemyAnimator.Play("FadeOut");

			    if (!fadeOutOnAwake)
			        fadeOutOnAwake = true;

			}
        }
	}
}
