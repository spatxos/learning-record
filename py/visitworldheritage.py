import csv
import requests
from bs4 import BeautifulSoup

# 获取世界遗产名单
url = 'https://visitworldheritage.com/'
response = requests.get(url)
soup = BeautifulSoup(response.content, 'html.parser')

# 获取每个遗产的名称、国家和图片链接
heritages = soup.find_all('div', class_='heritage')
for heritage in heritages:
  name = heritage.find('h2').text
  country = heritage.find('p', class_='country').text
  image_url = heritage.find('img')['src']

# 将遗产信息保存到 CSV 文件
with open('heritages.csv', 'w') as f:
  writer = csv.writer(f)
  writer.writerow(['名称', '国家', '图片链接'])
  for heritage in heritages:
    writer.writerow([name, country, image_url])