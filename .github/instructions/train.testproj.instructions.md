---
name: train.testproj.instructions
description: "Project-level instructions for train.testproj（测试项目）。"
applyTo: "**/train.testproj/**"
---
 
简要说明：此项目包含单元测试，使用 MSTest 框架。

常用命令：
- 运行测试: `dotnet test train.testproj`

关键文件/目录：
- `FetchXmlBuilderTest.cs` — 示例测试
- `appsettings.json` — 会被复制到输出目录

示例提示：
- "运行并报告 `train.testproj` 的失败测试。"
- "检查 `FetchXmlBuilderTest` 并建议改进或修复。"

注意：此 instruction 仅在 `train.testproj` 目录下加载。
