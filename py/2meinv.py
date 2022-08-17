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
    soup = BeautifulSoup(html, "html.parser", from_encoding="utf-8")
    return soup
 
 
headers = {
    'user-agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.59'
}
 
url = "https://www.2meinv.com/"
for p in range(1, 10 + 1):
    next_url = url + "index-" + str(p) + ".html"
    soup = html_parse(next_url, headers)
    link_node = soup.findAll('div', attrs={"class": "dl-name"})
    for a in link_node:
        path = "E:/spider/image/2meinv/"
        href = a.find('a', attrs={'target': '_blank'}).get('href')
        no = re.findall('[^-$][\d]', href)[1] + re.findall('[^-$][\d]', href)[2]
        first_url = url + "/article-" + no + ".html"
        title = a.find('a', attrs={'target': '_blank'}).text
        path = path + title + "/"
        soup = html_parse(href, headers)
        count = soup.find('div', attrs={'class': 'des'}).find('h1').text
        last_page = get_last_page(count)
        for i in range(1, last_page + 1):
            next_url = url + "/article-" + no + "-" + str(i) + ".html"
            soup = html_parse(next_url, headers)
            image_url = soup.find('img')['src']
            image_name = image_url.split("/")[-1]
            fileName = path + image_name
            if not os.path.exists(path):
                os.makedirs(path)
            if os.path.exists(fileName):
                continue
            request.urlretrieve(image_url, filename=fileName)
            request.urlcleanup()
        print(title, "下载完成了")