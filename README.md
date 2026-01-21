# WPF.OnClap - Wallpaper Focus Tool

![License](https://img.shields.io/badge/license-MIT-green) ![Platform](https://img.shields.io/badge/platform-Windows-blue) ![Framework](https://img.shields.io/badge/.NET-8.0-purple)

**Wallpaper Focus Tool** 是一个基于 WPF (.NET 8) 开发的桌面小工具，通过本工具，你可以快速给喜欢的图片添加**高斯模糊**、**蒙版滤镜**等效果，生成一张既保留原图氛围，又适合作为生产力桌面的“专注型”壁纸。

## ✨ 功能特性 (Features)

* **现代卡片式 UI**：采用当下流行的 Web 风格设计，清爽的卡片布局与阴影效果。
* **实时预览**：利用 WPF 强大的图形渲染引擎，调整参数时实时查看模糊效果。
* **多种模糊策略**：
    * 🟥 **方框模糊 (Box Blur)**：高性能，适合营造独特的块状虚化感。
    * 💧 **高斯模糊 (Gaussian Blur)**：最经典的柔和虚化，画质细腻。
    * 📚 **堆叠模拟 (Stack Emulation)**：模拟多层模糊叠加产生的强虚化效果。
* **所见即所得的高清导出**：无论预览窗口多小，保存时始终基于**原始图片分辨率**（支持 4K/8K）进行渲染，绝不压缩画质。
* **智能交互**：自定义的单选按钮样式、拖拽滑块时的数值反馈。

## 🛠️ 技术栈 (Tech Stack)

* **开发框架**: .NET 8
* **UI 框架**: WPF
* **开发语言**: C#
* **IDE**: Visual Studio 2022

## 🧩 技术亮点 (Technical Highlights)

本项目在实现过程中解决了几个 WPF 开发中的常见痛点，适合作为 WPF 学习案例参考：

### 1. 解决 RenderTargetBitmap 的特效丢失问题
在早期的实现中，直接使用 `DrawingVisual` 绘制带有 `BlurEffect` 的图片时，保存结果往往是原图（特效失效）。
本项目采用了一种**“内存 UI 树重建”**的方案：在内存中构建一个与预览区结构一致但尺寸为原图物理像素的 `Grid`，强制执行 `Measure` 和 `Arrange` 布局流程，最后再进行 `Render`。这确保了无论屏幕 DPI 如何，导出的图片像素都能与原图 1:1 对齐且特效完整。

### 2. 自定义控件模板 (ControlTemplate)
为了摒弃 WPF 原生老旧的 RadioButton 样式，本项目完全重写了 `ControlTemplate`，通过 `Trigger` 实现了“选中后边框变色 + 背景高亮”的现代按钮交互效果，而非简单的圆圈选择。

### 3. MVVM 友好的转换器设计
实现了通用的 `BooleanToVisibilityConverter`，支持 `Inverse` 参数和空值判断，极大地简化了 XAML 中的逻辑绑定。

## 🚀 快速开始 (Getting Started)

### 环境要求
* Windows 10 / 11
* .NET 8 Runtime (如果只运行发布包)
* Visual Studio 2022 (如果需要编译源码)

## 📅 路线图 (Roadmap)

* [ ] **径向模糊 (Radial Blur)**: 目前 UI 已预留入口，计划接入 HLSL Shader (PixelShader) 来实现高性能的径向模糊效果。
* [ ] **MVVM 重构**: 引入 CommunityToolkit.Mvvm 或 Prism 框架，将目前的 Code-Behind 逻辑解耦。
* [ ] **批量处理**: 支持一次性拖入多张图片批量生成壁纸。

## 📄 许可证 (License)

本项目采用 [MIT License](LICENSE) 开源。