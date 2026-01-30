import chromadb
from chromadb.config import Settings

client = chromadb.Client(Settings(
    persist_directory="./chroma_db"
))

collection = client.create_collection(name="my_knowledge_base")

documents = [
    "Python是一种高级编程语言，由Guido van Rossum于1991年创建。",
    "JavaScript是一种脚本语言，主要用于Web开发。",
    "Chroma是一个开源的向量数据库，用于构建AI应用。",
    "向量数据库存储数据的向量表示，支持高效的相似性搜索。",
    "机器学习是人工智能的一个子领域，专注于算法和统计模型。"
]

metadatas = [
    {"topic": "programming", "language": "Python"},
    {"topic": "programming", "language": "JavaScript"},
    {"topic": "database", "type": "vector"},
    {"topic": "database", "type": "vector"},
    {"topic": "AI", "field": "ML"}
]

ids = ["doc1", "doc2", "doc3", "doc4", "doc5"]

collection.add(
    documents=documents,
    metadatas=metadatas,
    ids=ids
)

print("知识库创建成功！")
print(f"集合名称: {collection.name}")
print(f"文档数量: {collection.count()}")

results = collection.query(
    query_texts=["什么是向量数据库？"],
    n_results=2
)

print("\n搜索结果:")
for i, (doc, distance) in enumerate(zip(results['documents'][0], results['distances'][0])):
    print(f"\n结果 {i+1}:")
    print(f"内容: {doc}")
    print(f"相似度: {1-distance:.2f}")
