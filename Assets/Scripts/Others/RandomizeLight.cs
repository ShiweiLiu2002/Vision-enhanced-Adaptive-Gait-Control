using UnityEngine;

public class RandomLightRotation : MonoBehaviour
{
    [Header("角度范围限制")]
    public Vector2 xRange = new Vector2(-30f, 30f);
    public Vector2 yRange = new Vector2(-60f, 60f);
    public Vector2 zRange = new Vector2(-20f, 20f);

    [Header("旋转参数")]
    public float changeInterval = 2f;   // 每隔多少秒随机一个新方向
    public float rotationSpeed = 2f;    // 平滑旋转速度

    private Quaternion targetRotation;  // 目标旋转
    private float timer;

    void Start()
    {
        SetRandomRotation();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 每隔 changeInterval 秒重新生成目标旋转
        if (timer >= changeInterval)
        {
            SetRandomRotation();
            timer = 0f;
        }

        // 平滑旋转到目标角度
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }

    void SetRandomRotation()
    {
        float rx = Random.Range(xRange.x, xRange.y);
        float ry = Random.Range(yRange.x, yRange.y);
        float rz = Random.Range(zRange.x, zRange.y);

        targetRotation = Quaternion.Euler(rx, ry, rz);
    }
}
