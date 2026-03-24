---
name: train.dataverse.comm.instructions
description: "Project-level instructions for train.dataverse.comm (库)。"
applyTo: "../../train.dataverse.comm/**"
---
 
简要说明：此项目包含公用库代码，用于构建与与 Dataverse 数据访问相关的帮助程序和仓库。

常用命令：
- 构建整个解决方案: `dotnet build train.dataverse.sln`

关键文件/目录：
- `FetchXmlBuilder.cs` — 辅助构建 FetchXML
- `EntityCollectionExtensions.cs` — 实用扩展
- `FetchRepository/` — 各实体仓库（例如 `AccountRepository.cs`）

示例提示：
- "查找仓库中所有使用 `FetchXmlBuilder` 的位置并说明用途。"
- "帮我为 `FetchRepository` 添加单元测试示例（MSTest）。"

注意：此 instruction 仅在 `train.dataverse.comm` 目录下加载。
