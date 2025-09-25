import numpy as np
import matplotlib.pyplot as plt
from scipy.ndimage import gaussian_filter
from PIL import Image

def generate_tile_heightmap(tile_count=40,
                             tile_size=16,
                             smoothness=2.0,
                             height_range=(0.1, 1.0),
                             final_size=(513, 513),
                             output_png="resized_tiles.png",
                             output_raw="resized_tiles.raw"):
    # 原始尺寸
    H = W = tile_count * tile_size

    # 随机 tile 高度
    tile_heights = np.random.uniform(height_range[0], height_range[1], size=(tile_count, tile_count))

    # 构建大图
    heightmap = np.zeros((H, W), dtype=np.float32)
    for y in range(tile_count):
        for x in range(tile_count):
            val = tile_heights[y, x]
            y0 = y * tile_size
            x0 = x * tile_size
            heightmap[y0:y0+tile_size, x0:x0+tile_size] = val

    # 平滑
    if smoothness > 0:
        heightmap = gaussian_filter(heightmap, sigma=smoothness)

    # 归一化
    heightmap_norm = (heightmap - heightmap.min()) / (heightmap.max() - heightmap.min())

    # Resize 到最终尺寸
    resized_img = Image.fromarray((heightmap_norm * 255).astype(np.uint8)).resize(final_size, resample=Image.BICUBIC)
    resized_array = np.array(resized_img, dtype=np.uint8)

    # 保存 PNG
    Image.fromarray(resized_array).save(output_png)

    # 转换为 16-bit 并保存为 RAW
    raw_array = (resized_array.astype(np.float32) / 255.0 * 65535).astype(np.uint16)
    raw_array.tofile(output_raw)

    # 可视化
    plt.imshow(resized_array, cmap="gray")
    plt.title("Resized Tile-based Heightmap")
    plt.colorbar()
    plt.show()

    print(f"[✓] Resized heightmap saved as '{output_png}' and '{output_raw}' (size: {final_size[0]}x{final_size[1]})")

# 示例调用
if __name__ == "__main__":
    generate_tile_heightmap(
        tile_count=40,
        tile_size=16,
        smoothness=1,
        height_range=(0.0, 1.0),
        final_size=(513, 513),
        output_png="tilesmap_2.png",
        output_raw="tilesmap_2.raw"
    )
