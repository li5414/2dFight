import urllib
import urllib.request
import os
import time

def mkdir(path):
    path=path.strip()
    path=path.rstrip("\\")
    isExists=os.path.exists(path)
 
    if not isExists:
        os.makedirs(path) 
        return True
    else:
        return False

def cbk(a,b,c):
    per=100.0*a*b/c
    if per>100:
        per=100

dir=os.path.abspath('.')

d = ["","a","b","c","d","e","n","l"]
# names = ["kyo","benimaru","daimon","terry","andy","joe","ryo","robert","yuri","leona",
#         "ralf","clark","athena","kensou","chin","chizuru","mai","king","kim","chang",
#         "choi","yashiro","shermie","chris","yamazaki","mary","billy","iori","orochi",
#         "shingo","iori2","orichi-yashiro","orichi-shermie","orichi-chris"]
names = ["benimaru"]
for name in names:
    mkdir(os.path.join(dir,name))
for name in names:
    for p in d:
        for i in range(0,50):
            if i==0 :
                if p!="" and p!="a":
                    continue
                else:
                    print("")
            file = p+"%02d" % i+".gif"
            work_path=os.path.join(dir,name+'/'+file)
            if os.path.exists(work_path):
                print(name+'  '+file +' exists!!')
                continue
            else:
                print(file +' not download!')
            
            img_src = 'http://oss.emugif.com/picture2014/kof97/'+name+'/'+file
            try:
                urllib.request.urlretrieve(img_src,work_path,cbk)
            except urllib.error.HTTPError as e:
                print("下载失败: %d url:%s" % (e.code,file))  
                break
            else:
                print("下载url:%s成功!" % (file))  
            finally:
                time.sleep(2)
                