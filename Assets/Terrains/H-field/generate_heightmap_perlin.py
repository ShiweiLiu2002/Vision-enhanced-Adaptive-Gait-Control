import numpy as np
import cv2
import noise

def generate_perlin_hfield(
    output_prefix="perlin_hfield",
    image_width=512,
    image_height=512,
    smooth=100.0,
    perlin_octaves=6,
    perlin_persistence=0.5,
    perlin_lacunarity=2.0,
):
    # 生成Perlin噪声高度图
    terrain = np.zeros((image_height, image_width), dtype=np.float32)

    for y in range(image_height):
        for x in range(image_width):
            nx = x / smooth
            ny = y / smooth
            noise_value = noise.pnoise2(
                nx,
                ny,
                octaves=perlin_octaves,
                persistence=perlin_persistence,
                lacunarity=perlin_lacunarity,
                repeatx=1024,
                repeaty=1024,
                base=42
            )
            terrain[y][x] = (noise_value + 1) / 2.0  # Normalize to [0, 1]

    # 保存 PNG 图像用于可视化
    terrain_img = (terrain * 255).astype(np.uint8)
    cv2.imwrite(f"{output_prefix}.png", terrain_img)

    # 保存 .raw 文件用于 Unity Terrain 导入（16位灰度）
    terrain_raw = (terrain * 65535).astype(np.uint16)
    terrain_raw.tofile(f"{output_prefix}.raw")

    print(f"Saved: {output_prefix}.png and {output_prefix}.raw")

# 示例调用
generate_perlin_hfield(
    output_prefix="unity_perlin_heightmap",
    image_width=513,
    image_height=513,
    smooth=100,
    perlin_octaves=6,
    perlin_persistence=0.5,
    perlin_lacunarity=2.0
)
