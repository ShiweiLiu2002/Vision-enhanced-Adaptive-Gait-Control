import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
from PIL import Image

# --- 1. 加载高度图 ---
# 将 'your_heightmap.png' 替换为你的文件名
try:
    img = Image.open('./grassland_H/terrain_000.png').convert('L')  # 'L'模式表示灰度图
except FileNotFoundError:
    print("错误：请确保 'your_heightmap.png' 文件在正确的路径下！")
    exit()

# 将图像数据转换为numpy数组
# 数组中的值将代表地形的高度
z_data = np.array(img)

# 为了让地形起伏更明显，可以根据需要乘以一个缩放因子
# 如果您的地形已经很明显，可以跳过这一步或减小因子
height_scale = 0.001
z_data = z_data * height_scale

# --- 2. 创建坐标网格 ---
# 获取图像的尺寸
height, width = z_data.shape
x_data = np.arange(width)
y_data = np.arange(height)

# 创建网格
X, Y = np.meshgrid(x_data, y_data)

# --- 3. 创建3D图形 ---
fig = plt.figure(figsize=(16, 12))  # 创建一个更大尺寸的图形，以便获得更高清的输出
ax = fig.add_subplot(111, projection='3d')

# --- 4. 绘制表面并进行美化 ---
# 使用 'cmap' 参数选择一个漂亮的颜色映射
# 推荐: 'viridis', 'plasma', 'inferno', 'magma', 'cividis', 'terrain'
# stride参数可以降低渲染密度，让曲面更平滑，尤其是在高分辨率图像上
surf = ax.plot_surface(X, Y, z_data, cmap='terrain', rstride=5, cstride=5,
                       linewidth=0, antialiased=True, shade=True)

# --- 5. 精细化美学调整 ---
# a) 设置视角 (elevation, azimuth)
# elevation: 上下视角. azimuth: 水平旋转角度
ax.view_init(elev=85, azim=0)
ax.axis('off')

# # b) 移除坐标轴刻度和背景网格，让画面更纯粹
# ax.set_xticks([])
# ax.set_yticks([])
# ax.set_zticks([])

# # c) 设置坐标轴的背景为透明
# ax.xaxis.pane.fill = False
# ax.yaxis.pane.fill = False
# ax.zaxis.pane.fill = False
# ax.xaxis.pane.set_edgecolor('w')
# ax.yaxis.pane.set_edgecolor('w')
# ax.zaxis.pane.set_edgecolor('w')

# # d) 移除坐标轴的灰色网格线
# ax.grid(False)

# e) 添加颜色条 (可选，但通常很好看)
# shrink和aspect控制颜色条的大小
# cbar = fig.colorbar(surf, shrink=0.5, aspect=10, pad=0.1)
# cbar.set_label('高度')

# f) 调整图形与边界的间距
plt.subplots_adjust(left=0, right=1, top=1, bottom=0)

# --- 6. 保存为高清图片 ---
# dpi (dots per inch) 参数决定了输出图片的分辨率，300是印刷级别的高清标准
# transparent=True 使背景透明，可以更好地融入PPT
plt.savefig('3d_visualization.png', dpi=300, transparent=True, bbox_inches='tight', pad_inches=0)

print("3D可视化图片 '3d_visualization.png' 已成功保存！")

# 显示图形 (可选)
plt.show()