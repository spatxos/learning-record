import os
import re
import time
from urllib import request
from bs4 import BeautifulSoup
 
 
def get_last_page(text):
    return int(re.findall('[^/$]\d*', re.split('/', text)[-1])[0])
 
 
def html_parse(url, headers):
    time.sleep(3)
    resp = request.Request(url=url, headers=headers)
    res = request.urlopen(resp)
    html = res.read().decode("utf-8")
    soup = BeautifulSoup(html, "html.parser", from_encoding="utf-8", verify=False)
    return soup
 
 
headers = {
    'user-agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.59'
}

path = "D:/Project/github/learning-record/py/mkjqryfyy/"
url = "http://www.chinamine-safety.gov.cn/xw/zt/2018zt/mkjqryfyy/gzdt_01/202307/t20230704_455286.shtml"
soup = html_parse(url, headers)
p_node = soup.findAll('p', attrs={"class": "MsoNormal"})
for a in p_node:
    span_node = soup.findAll('span')
    for a in span_node:
        href = a.find('a', attrs={'target': '_blank'}).get('href')
        title = a.find('a', attrs={'target': '_blank'}).text
        fileName = path + title
        if not os.path.exists(path):
            os.makedirs(path)
        if os.path.exists(fileName):
            continue
        request.urlretrieve(href, filename=fileName)
        request.urlcleanup()
    print(title, "下载完成了")