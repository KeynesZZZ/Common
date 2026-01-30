import chromadb
from chromadb.config import Settings
import os

class ChromaKnowledgeBase:
    def __init__(self, persist_directory="./chroma_db"):
        self.persist_directory = persist_directory
        os.makedirs(persist_directory, exist_ok=True)
        
        self.client = chromadb.PersistentClient(path=persist_directory)
        self.collection = None
    
    def create_collection(self, name):
        self.collection = self.client.create_collection(name=name)
        print(f"集合 '{name}' 创建成功！")
        return self.collection
    
    def get_or_create_collection(self, name):
        self.collection = self.client.get_or_create_collection(name=name)
        print(f"已加载集合 '{name}'")
        return self.collection
    
    def add_documents(self, documents, metadatas=None, ids=None):
        if not self.collection:
            raise Exception("请先创建或加载集合")
        
        if ids is None:
            ids = [f"doc_{i}" for i in range(len(documents))]
        
        self.collection.add(
            documents=documents,
            metadatas=metadatas,
            ids=ids
        )
        print(f"成功添加 {len(documents)} 个文档")
    
    def search(self, query, n_results=5, where=None):
        if not self.collection:
            raise Exception("请先创建或加载集合")
        
        results = self.collection.query(
            query_texts=[query],
            n_results=n_results,
            where=where
        )
        
        print(f"\n搜索查询: '{query}'")
        print(f"找到 {len(results['documents'][0])} 个结果:\n")
        
        for i, (doc, metadata, distance) in enumerate(zip(
            results['documents'][0],
            results['metadatas'][0],
            results['distances'][0]
        )):
            print(f"结果 {i+1}:")
            print(f"内容: {doc}")
            print(f"元数据: {metadata}")
            print(f"相似度: {1-distance:.4f}")
            print("-" * 50)
        
        return results
    
    def delete_document(self, doc_id):
        if not self.collection:
            raise Exception("请先创建或加载集合")
        
        self.collection.delete(ids=[doc_id])
        print(f"文档 '{doc_id}' 已删除")
    
    def get_collection_info(self):
        if not self.collection:
            raise Exception("请先创建或加载集合")
        
        count = self.collection.count()
        print(f"\n集合信息:")
        print(f"名称: {self.collection.name}")
        print(f"文档数量: {count}")
        return count
    
    def list_all_collections(self):
        collections = self.client.list_collections()
        print(f"\n所有集合:")
        for coll in collections:
            print(f"- {coll.name}")
        return collections


if __name__ == "__main__":
    kb = ChromaKnowledgeBase()
    
    kb.get_or_create_collection("my_knowledge_base")
    
    documents = [
        "Python是一种高级编程语言，由Guido van Rossum于1991年创建。",
        "JavaScript是一种脚本语言，主要用于Web开发。",
        "Chroma是一个开源的向量数据库，用于构建AI应用。",
        "向量数据库存储数据的向量表示，支持高效的相似性搜索。",
        "机器学习是人工智能的一个子领域，专注于算法和统计模型。",
        "深度学习是机器学习的一个分支，使用神经网络进行学习。",
        "自然语言处理（NLP）是AI的一个领域，专注于计算机与人类语言的交互。",
        "计算机视觉是AI的一个领域，使计算机能够理解和分析图像。"
    ]
    
    metadatas = [
        {"topic": "programming", "language": "Python"},
        {"topic": "programming", "language": "JavaScript"},
        {"topic": "database", "type": "vector"},
        {"topic": "database", "type": "vector"},
        {"topic": "AI", "field": "ML"},
        {"topic": "AI", "field": "DL"},
        {"topic": "AI", "field": "NLP"},
        {"topic": "AI", "field": "CV"}
    ]
    
    kb.add_documents(documents, metadatas)
    
    kb.get_collection_info()
    
    kb.search("什么是深度学习？", n_results=3)
    
    kb.search("编程语言", n_results=2)
    
    kb.list_all_collections()
