import numpy as np
import cv2
import os
import noise
from PIL import Image
import matplotlib.pyplot as plt
from scipy.ndimage import gaussian_filter

def generate_seamless_perlin_terrains(
    output_dir="terrains",
    num_terrains=5,
    image_width=513,
    image_height=513,
    smooth=100.0,
    perlin_octaves=6,
    perlin_persistence=0.5,
    perlin_lacunarity=2.0,
):
    os.makedirs(output_dir, exist_ok=True)
    last_right_column = None  # 用于存储前一张的右边界列

    for terrain_index in range(num_terrains):
        terrain = np.zeros((image_height, image_width), dtype=np.float32)

        for y in range(image_height):
            for x in range(image_width):
                nx = x / smooth
                ny = y / smooth
                value = noise.pnoise2(
                    nx + terrain_index * image_width / smooth,  # 横向偏移
                    ny,
                    octaves=perlin_octaves,
                    persistence=perlin_persistence,
                    lacunarity=perlin_lacunarity,
                    repeatx=1024,
                    repeaty=1024,
                    base=42
                )
                terrain[y, x] = (value + 1) / 2.0  # 归一化到 [0,1]

        # 将最左边一列设置为上一张最右边一列，实现无缝连接
        if last_right_column is not None:
            terrain[:, 0] = last_right_column

        last_right_column = terrain[:, -1].copy()

        # 保存图像
        terrain_img = (terrain * 255).astype(np.uint8)
        png_path = os.path.join(output_dir, f"terrain_{terrain_index:03}.png")
        cv2.imwrite(png_path, terrain_img)

        # 保存 .raw 文件 (16-bit)
        raw_path = os.path.join(output_dir, f"terrain_{terrain_index:03}.raw")
        terrain_raw = (terrain * 65535).astype(np.uint16)
        terrain_raw.tofile(raw_path)

        print(f"[✓] Saved: {png_path} and {raw_path}")

def generate_batch_tile_heightmaps(
    num_maps=5,
    output_dir="terrains/tiles",
    tile_count=30,
    tile_size=16,
    smoothness=2.0,
    height_range=(0.1, 1.0),
    final_size=(513, 513)
):
    os.makedirs(output_dir, exist_ok=True)

    for i in range(num_maps):
        H = W = tile_count * tile_size
        tile_heights = np.random.uniform(height_range[0], height_range[1], size=(tile_count, tile_count))
        heightmap = np.zeros((H, W), dtype=np.float32)

        for y in range(tile_count):
            for x in range(tile_count):
                val = tile_heights[y, x]
                y0 = y * tile_size
                x0 = x * tile_size
                heightmap[y0:y0 + tile_size, x0:x0 + tile_size] = val

        if smoothness > 0:
            heightmap = gaussian_filter(heightmap, sigma=smoothness)

        heightmap_norm = (heightmap - heightmap.min()) / (heightmap.max() - heightmap.min())
        resized_img = Image.fromarray((heightmap_norm * 255).astype(np.uint8)).resize(final_size, resample=Image.BICUBIC)
        resized_array = np.array(resized_img, dtype=np.uint8)
        raw_array = (resized_array.astype(np.float32) / 255.0 * 65535).astype(np.uint16)

        png_path = os.path.join(output_dir, f"tilemap_{i:03}.png")
        raw_path = os.path.join(output_dir, f"tilemap_{i:03}.raw")
        Image.fromarray(resized_array).save(png_path)
        raw_array.tofile(raw_path)

        print(f"[✓] Saved {png_path} and {raw_path}")


# generate_seamless_perlin_terrains(
#     output_dir="./grassland_H",
#     num_terrains=3,
#     image_width=513,
#     image_height=513,
#     smooth=80,
#     perlin_octaves=6,
#     perlin_persistence=0.4,
#     perlin_lacunarity=2.0
# )

generate_batch_tile_heightmaps(
        num_maps=3,
        output_dir="./tiles_H",
        tile_count=30,
        tile_size=16,
        smoothness=1,
        height_range=(0.0, 1.0),
        final_size=(513, 513)
    )