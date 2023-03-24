import json
from urllib import parse
import requests
import os
import time
from bs4 import BeautifulSoup as BS
import urllib.request

webroot="http://www.btbtt.me/"
url = 'http://www.btbtt.me/thread-index-fid-950-tid-4435716.htm'
requests.packages.urllib3.disable_warnings()  # 忽视网页安全性问题
r = requests.get(url, verify=False)  # 不验证证书
r.encoding = 'utf8'
soup = BS(r.text, 'html.parser')
headers = {'User-Agent':'Mozilla/5.0 (Windows NT 6.1; WOW64; rv:23.0) Gecko/20100101 Firefox/23.0'}

attatchpage=soup.find_all('div',class_='attachlist')
for i in attatchpage:
    #print(i)
    for j in i.find_all('a'):
        downloadurl=j.get('href')
        
       # print(j.get('href'))
        r2=requests.get(webroot+j.get('href'), verify=False) 
        downpage=BS(r2.text,'html.parser')
        
        want5=downpage.find('div',id='body')
        filename=want5.select('dd')
        imgpath='E:/python/code/down/'+filename[0].text.strip()
        for k in want5.find_all('a'):
            print(imgpath)
            if not os.path.exists(imgpath):
                response = requests.get(webroot+k.get('href'),headers=headers)
                with open(imgpath, 'wb') as f:
                    f.write(response.content)
                    f.flush()