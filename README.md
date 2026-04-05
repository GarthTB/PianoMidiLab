# PianoMidiLab 钢琴独奏MIDI编辑器

批量编辑格式 0/1 钢琴 MIDI 的音符与踏板。
提供清理、力度映射、力度均衡三个顺序执行的功能。
结果以唯一文件名输出至原目录，无覆写风险。

![Tech Stack](https://skillicons.dev/icons?i=dotnet,cs,windows)

[![License MIT](https://img.shields.io/badge/License-MIT-750014)](https://mit-license.org)
[![Latest 1.0.0](https://img.shields.io/badge/Latest-1.0.0-0FBF3E?logo=github)](https://github.com/GarthTB/PianoMidiLab/releases/latest)

## ✨ 特点

- 🏭 一站式：多种功能，批量操作
- 📦 免安装：<1 MB 便捷包，解压即用
- 💪 轻量级：不依赖外部 MIDI 库，高效轻巧

## ⚙ 功能

1. **清除音符**：删除长于/短于/高于/低于/强于/弱于特定阈值的所有音符
2. **力度映射**：利用两个固定锚点（输入力度 1 和 127）与 0–125 个附加锚点，线性插值，对 `NoteOn` 执行 `[输入力度, 输出力度]`
   映射。
3. **力度均衡**：利用两个固定锚点（音高 0 和 127）与 0–126 个附加锚点，线性插值，对 `NoteOn` 施加 `[音高, 力度增益]`。
    - 增益定义为 gamma 的倒数：`out = Pow((in - 1) / 126, 1 / gain) * 126 + 1`

## 📥 用法

### 系统要求：Windows

### 运行依赖：[.NET 10.0 运行时](https://dotnet.microsoft.com/download/dotnet/10.0)

### 使用步骤

1. 下载 [最新版本发布包](https://github.com/GarthTB/PianoMidiLab/releases/latest) 并解压
2. 运行 `PianoMidiLab.exe`
3. 添加文件，调整参数，启动处理

## ℹ 关于

- 地址：https://github.com/GarthTB/PianoMidiLab
- 技术：.NET 10.0/C# 14.0/WPF/CommunityToolkit.Mvvm
- 协议：[MIT 许可证](https://mit-license.org/)
- 作者：Garth TB | 天卜 <g-art-h@outlook.com>
- 版权：Copyright (c) 2026 Garth TB | 天卜
- 声明：本项目基于作者自用需求，追求极简高效而不承诺完备

## 📝 版本

### v1.0.0 (20260405)

首发，部分继承并废弃 [TrimMIDI](https://github.com/GarthTB/TrimMIDI) 项目
