using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public class Shadow : MonoBehaviour
{
	private GameObject shadow = null;

	private BoxCollider collid = null;
	private Vector3 size = Vector3.zero;
	private Vector3 unscaledSize = Vector3.zero;

	private int groundMask;
	private int pushableObjectMask;


	private void Awake()
	{
		collid = GetComponent<BoxCollider>();

		unscaledSize = collid.size;

		size = collid.size;
		size.x *= collid.gameObject.transform.localScale.x;
		size.y *= collid.gameObject.transform.localScale.y;
		size.z *= collid.gameObject.transform.localScale.z;

		groundMask = 1 << LayerMask.NameToLayer("Ground");
		pushableObjectMask = 1 << LayerMask.NameToLayer("PushableObject");
	}
	private void Start()
	{
		GameObject go = Resources.Load<GameObject>("Shadow");
		shadow = Instantiate(go, transform);
		shadow.transform.localPosition = Vector3.zero;
	}
	private void LateUpdate()
	{
		RaycastHit hit;

		if (Physics.BoxCast(collid.bounds.center, new Vector3(size.x / 2f, size.y / 2f * 0.9f, size.z / 2f), Vector3.down, out hit, Quaternion.identity, float.PositiveInfinity, groundMask | pushableObjectMask))
		{
			Vector3 pos = collid.transform.position;
			pos.x += collid.center.x;
			pos.y = hit.point.y;
			shadow.transform.position = pos;

			float xScale = Mathf.Clamp(unscaledSize.x * (unscaledSize.y / 2f / hit.distance), 0.1f, unscaledSize.x);
			float yScale = Mathf.Clamp(unscaledSize.z * (unscaledSize.y / 2f / hit.distance), 0.1f, unscaledSize.z);
			Vector3 scale = new Vector3(xScale, yScale, 1f);
			shadow.transform.localScale = scale;
		}
	}

	public void Show()
	{
		shadow.SetActive(true);
	}
	public void Hide()
	{
		shadow.SetActive(false);
	}
}
