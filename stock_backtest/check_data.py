import zipfile
import pandas as pd
import io

z = zipfile.ZipFile(r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总\2015_60min.zip')
data = z.read('sh600000_2015.csv')
df = pd.read_csv(io.BytesIO(data))
print(df.head(10))
print('\n列名:', list(df.columns))
print('\n数据形状:', df.shape)
