import pandas as pd
import pymysql
import matplotlib.pyplot as plt
 
#省略数据库连接初始化操作
db  = pymysql.connect(host='localhost',user='root',passwd='123456',port=3306,db='itoe_mjl')

#开启一个游标cursor
cursor=db.cursor()

#获取phone数据表里的所有数据
sql="select tagname,DATE_FORMAT(timestamp,'%Y-%m-%d %H:%i:%s'),value from itoe_target_20220805 where tagdata=14502 and tagname in ('HS_077.QZ_Pressure','HS_077.Valve04.Status') and timestamp    BETWEEN '2022-08-04 05:10:00' and  '2022-08-04 05:40:00' order by timestamp asc"

#执行sql中的语句
cursor.execute(sql)

#将获取到的sql数据全部显示出来
result=cursor.fetchall()
print("result.length:"+str(len(result)))
#关闭游标和数据库
cursor.close()
db.close()

#定义需要用上的空数据数组，然后通过遍历数据库的数据将数据附上去
xname=[(['']*1) for i in range(4)]
ynum=[([0]*1) for i in range(4)]
linename=[]

#遍历表里的数据
for x in result:
    lineindex=0
    if x[0] in linename:
       lineindex=linename.index(x[0])
    if x[0] not in linename:
        linename.append(x[0])
        if lineindex>0:
           xname.append([''])
           ynum.append([0])
        else:
           if xname[0][0]=='':
              del xname[0][0]
              del ynum[0][0]
    #print("linename.length:"+str(len(linename)))
    #print("xname.length:"+str(len(xname)))
    #print("ynum.length:"+str(len(ynum)))
    #print("lineindex:"+str(lineindex))
    #print(xname[lineindex])
    #print(ynum[lineindex])
    print(x[1])
    print(x[2])
    xname[lineindex].append(x[1])
    ynum[lineindex].append(x[2])
    #print("linename.length:"+str(len(linename)))
    #print("xname.length:"+str(len(xname)))
    #print("ynum.length:"+str(len(ynum)))

print("gc:result")
del result

print(linename)

color=['r','b','y']
lines=[]
labels=[]
#创建一个figure（一个窗口）来显示折线图
plt.title('查看数据波动')  # 折线图标题
plt.rcParams['font.sans-serif'] = ['SimHei']  # 显示汉字
plt.xlabel('时间')  # x轴标题
plt.ylabel('值')  # y轴标题
for line in linename:
    index=0
    if line in linename:
       index=linename.index(line)
    line1=plt.scatter(xname[index], ynum[index], marker='o')  # 绘制折线图，添加数据点，设置点的大小, markersize=3
    lines.append(line1)
    #for a, b in zip(xname[index], ynum[index]):
        #line1=plt.text(a, b, b, ha='center', va='bottom', fontsize=10, label=line)  # 设置数据标签位置及大小
        #lines.append(line1)

plt.legend(handles=lines, labels=linename, loc='best')  # 设置折线名称
print(lines)

#显示图表
plt.show()