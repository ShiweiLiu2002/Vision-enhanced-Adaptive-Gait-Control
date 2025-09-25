using UnityEngine;

public class FollowXZ : MonoBehaviour
{
    public Transform target;          // 需要跟随的物体
    public bool keepInitialY = true;  // 是否锁定相机初始的 Y
    public bool keepInitialOffset = true; // 是否保持与目标的初始相对偏移
    public bool smooth = true;        // 是否平滑跟随
    public float smoothTime = 0.15f;  // 平滑时间

    float initY;
    Vector3 initOffset;   // 初始的偏移（包含x和z）
    Vector3 velocity;     // SmoothDamp 的速度缓存

    void Start()
    {
        initY = transform.position.y;

        if (target != null)
            initOffset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 期望位置（是否保持初始偏移）
        Vector3 desiredPos = keepInitialOffset ? target.position + initOffset
                                               : new Vector3(target.position.x, transform.position.y, target.position.z);

        // Y轴处理
        float y = keepInitialY ? initY : transform.position.y;
        desiredPos.y = y;

        // 平滑或直接跟随
        Vector3 newPos = smooth ? Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime)
                                : desiredPos;

        transform.position = newPos;
    }
}
