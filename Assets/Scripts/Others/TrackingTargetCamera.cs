using UnityEngine;

public class FollowXOnly : MonoBehaviour
{
    public Transform target;          // 需要跟随的物体
    public bool keepInitialYZ = true; // 是否锁定相机初始的 Y/Z
    public bool keepInitialOffset = true; // 是否保持与目标在 X 轴的初始相对距离
    public bool smooth = true;        // 是否平滑跟随
    public float smoothTime = 0.15f;  // 平滑时间

    float initY, initZ;
    float xVel;       // SmoothDamp 的速度缓存
    float initOffset; // 初始的 X 轴偏移

    void Start()
    {
        var p = transform.position;
        initY = p.y;
        initZ = p.z;

        if (target != null)
            initOffset = transform.position.x - target.position.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 期望的 X（可选择保留初始相对偏移）
        float desiredX = keepInitialOffset ? target.position.x + initOffset : target.position.x;

        // 平滑或直接跟随
        float newX = smooth ? Mathf.SmoothDamp(transform.position.x, desiredX, ref xVel, smoothTime)
                            : desiredX;

        // 锁定 Y/Z（或保持当前值）
        float y = keepInitialYZ ? initY : transform.position.y;
        float z = keepInitialYZ ? initZ : transform.position.z;

        transform.position = new Vector3(newX, y, z);
    }
}
