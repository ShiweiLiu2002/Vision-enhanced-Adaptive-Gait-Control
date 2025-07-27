import imageio.v2 as imageio  # v2 保持兼容 PIL 接口
import numpy as np
import matplotlib.pyplot as plt

# 用 imageio 加载原始 16-bit 灰度 PNG
img = imageio.imread("Substance_graph_height.png")  # shape: (H, W), dtype=uint16

# 检查像素范围
print("原始像素范围：", img.min(), img.max())

# 归一化至 [0, 65535]
normalized = ((img - img.min()) / (img.max() - img.min()) * 65535).astype(np.uint16)

# 保存为 RAW
normalized.tofile("Substance_graph_height.raw")

# 可视化预览
plt.imshow(normalized, cmap="gray")
plt.colorbar()
plt.title("Normalized Height Map")
plt.show()
