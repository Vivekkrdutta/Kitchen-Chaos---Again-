using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform followTransform;
    public void SetFollowTransform(Transform followTransform)
    {
        this.followTransform = followTransform;
    }

    private void LateUpdate()
    {
        if(followTransform == null)
        {
            return;
        }

        transform.position = followTransform.position;
        transform.rotation = followTransform.rotation;
    }
}
