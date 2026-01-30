# Chroma 知识库使用指南

## 安装完成

Chroma向量数据库已成功安装，版本：**1.4.1**

## 快速开始

### 1. 基本示例

运行简单示例：
```bash
python chroma_example.py
```

### 2. 知识库管理器

运行完整的管理工具：
```bash
python chroma_kb_manager.py
```

## 使用方法

### 创建知识库

```python
from chroma_kb_manager import ChromaKnowledgeBase

kb = ChromaKnowledgeBase(persist_directory="./chroma_db")
kb.get_or_create_collection("my_collection")
```

### 添加文档

```python
documents = ["文档内容1", "文档内容2"]
metadatas = [{"topic": "主题1"}, {"topic": "主题2"}]
kb.add_documents(documents, metadatas)
```

### 搜索知识

```python
results = kb.search("搜索查询", n_results=5)
```

### 获取集合信息

```python
kb.get_collection_info()
```

### 列出所有集合

```python
kb.list_all_collections()
```

## 主要功能

1. **创建集合**：创建或加载知识库集合
2. **添加文档**：批量添加文档和元数据
3. **语义搜索**：基于向量相似度的智能搜索
4. **元数据过滤**：支持按元数据过滤搜索结果
5. **持久化存储**：数据自动保存到本地
6. **集合管理**：查看和管理多个知识库

## 数据存储

知识库数据存储在：`./chroma_db/` 目录

## 特性

- 完全免费开源
- 本地部署，无需云服务
- 支持中文搜索
- 自动向量嵌入
- 高效相似性搜索
- 元数据支持

## 下一步

1. 添加您自己的文档到知识库
2. 使用不同的元数据进行分类
3. 集成到您的应用程序中
4. 探索更高级的搜索功能

## 常见问题

### 如何添加PDF文档？

需要先提取PDF文本内容，然后作为文档添加到知识库。

### 如何提高搜索准确性？

- 添加更多相关文档
- 使用更精确的元数据
- 调整搜索结果数量

### 如何备份数据？

直接复制 `chroma_db` 目录即可备份整个知识库。
