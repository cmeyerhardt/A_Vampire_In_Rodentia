using UnityEngine;

public class ValueBarDisplay : MonoBehaviour
{
    [SerializeField] Transform resize = null;

    public virtual void UpdateValue(float percentage)
    {
        resize.localScale = new Vector3(Mathf.Clamp(percentage, 0f, 1f), resize.transform.localScale.y, 0f);
    }
}
