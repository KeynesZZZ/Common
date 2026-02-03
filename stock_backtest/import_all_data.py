import os
from data_importer import DataImporter

if __name__ == "__main__":
    print("开始导入所有A股历史数据...")
    print()
    
    importer = DataImporter()
    importer.import_all_data()
    
    print()
    print("导入完成！")
