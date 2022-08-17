package main

import (
	"encoding/json"
	"fmt"
	"io"
	"io/ioutil"
	"net/http"
	"os"
	"strconv"
	"strings"
)

const cookie = " __gads=ID=xxxxx:T=1649728913:S=ALNI_MbOOVEJx5s2Puf6CEs5qYSkW_aHwg; UM_distinctid=1805aabf9bd238-08b22c569116a1-3a67551f-144000-1805aabf9be934; Hm_lpvt_5e692fc8cef6db021a22bd470d660e4c=1651548396; Hm_lvt_5e692fc8cef6db021a22bd470d660e4c=1651548396; .AspNetCore.Antiforgery.b8-pDmTq1XM=CfDJ8AuMt_3FvyxIgNOR82PHE4kGyR-4aSs739sPowluSIbWBXRwvByozasPNheZKXorGKFxlQ2XMwL2PoW9QSwXcJS2PX1srKH7Fa9naKg7dhqQywIeSl4dYqyikYKZH4e61r02vJbatsXG76kQEqh_vUo; .AspNetCore.Session=CfDJ8AuMt%2F3FvyxIgNOR82PHE4luRKm0AX8iUW7HmvQ8B%2BVY3t4ucprrn4Mu3jD4G%2B%2BufQxZZtdKQbvlaZnSDa6wkIP3oZiLhlEaGmF0MJPSm%2FjvekMt%2FlPeLQ5fSxmz5rtmzj7AGjgKu1bCViLfVGink8N4v4fsafdaldUsUF%2BkpcWz; __utma=2642927.1176019125.1649728912.1652923332.1652923332.1; __utmc=2642927; __utmz=2642927.1652923332.1.1.utmcsr=cn.bing.com|utmccn=(referral)|utmcmd=referral|utmcct=/; Hm_lvt_0daf1d1987de95558f12b56df149bfda=1653285692; Hm_lpvt_0daf1d1987de95558f12b56df149bfda=1653285692; Hm_lvt_851cdd44d7a836d43196b0bfa8c0c3bb=1654159502; Hm_lpvt_851cdd44d7a836d43196b0bfa8c0c3bb=1654475406; Hm_lvt_c897bd902d294bb3778d9ab55b85a256=1655199408; Hm_lpvt_c897bd902d294bb3778d9ab55b85a256=1655199408; _ga_4CQQXWHK3C=GS1.1.1655860922.2.0.1655860929.0; Hm_lvt_866c9be12d4a814454792b1fd0fed295=1655862584; _gid=GA1.2.635015422.1656895021; .Cnblogs.AspNetCore.Cookies=CfDJ8EOBBtWq0dNFoDS-ZHPSe52F_8BJMOrmRhMhLRmcJeNPNYAZShhv1vAYzNFl6joFyY3SRRhFhwjE5BPBm7hfwj_tt0TumcucOm41KSwNDo1_kmd2MFpnU5Eyr3jtlHzv1GyAGY2rUQ1ZVzxLFAm-r9LLSS18TD6VNK7cYXelXpwku8dT1Scv5LwWRHdNFROMu6WRmyz3SoqNIVogMSZ76IH-4HrOrxdd41atPXA9QLIYkQnj2bw7KIfNMOtcB5WeJbWMRColZgTWXEFfm16LIQLANqw9N1AKxVll_33HzisEZvd4i4wW31O_R2AcgQCaKAFajKer56gxUUb2qaXOAphOZZCZ3cklrvup-YXl_ldqQK7By_gQZvbHRZMrbSVMaBGo1ER7xmedA_qwRaQY3F6gdTennRV-m66cBYcdbn_s6UMgh4LSiOgEgvzxWUvZ6-8Kw3K6aCIaM0tW-xiFruVEM0HTBrKbzSkTkGRRk1A1i83WasxnbsjZBd3OPMkdd-59mHtgsleV-qkyqST_VgNRMc1m6pQ_qq9r4UpQQhSyDwEoiyjYTEFLZ5I4ffJMdA; .CNBlogsCookie=00F3EEBE5A6020C27D5652484C074B9064B1589BB206D2799428696F5DD095354DD663D4BF03B81A8A5C7C07D40485065CAB1DAEA82E15966360555453A0285FBB90FE08E9AB8F4356AE17B272013398342F0F59; _ga_3Q0DVSGN10=GS1.1.1657080090.2.1.1657080130.20; _ga=GA1.2.1176019125.1649728912; __gpi=UID=000004c28bf2c485:T=1649729335:RT=1657432736:S=ALNI_MY4bFzL-Q4DF7EivtB4voDkSrLb2w; Hm_lpvt_866c9be12d4a814454792b1fd0fed295=1657433641; _gat_gtag_UA_476124_1=1; XSRF-TOKEN=CfDJ8EOBBtWq0dNFoDS-ZHPSe53llSAdg_OtqjPc5GroeWSJuHpQnBWnDGN6612ujO60FL8yjKQ4jCWvn5p6jZSXEzmZPg7yr5gGY42iErwFVbQXMaCtGMmLsl2usLAJjSnOj2QOifSOwptRUc1peAJS4WBMUazb3hj3Hv8thHH-sWhWZtsOp-YDxh4omnmJIn1iog; _gat_gtag_UA_48445196_1=1"

func main() {
	fmt.Printf("开始执行")
	getBlogList(1)
}
func geturl(pageno int) string {
	return fmt.Sprintf("https://i.cnblogs.com/api/posts/list?p=%s&cid=&tid=&t=1&cfg=0&search=&orderBy=&s=&scid=", strconv.Itoa(pageno))
}
func getBlogList(pageindex int) {
	var urlstr = geturl(pageindex)

	recordbody := getData(urlstr)
	fmt.Printf("\r\n recordbody:%s \n", recordbody)

	var conf blogList
	err := json.Unmarshal(recordbody, &conf)
	if err != nil {
		fmt.Println("error:", err)
	}

	fmt.Printf("\r\n PageIndex:%s，PageSize:%s，PostsCount:%s \n", strconv.Itoa(conf.PageIndex), strconv.Itoa(conf.PageSize), strconv.Itoa(conf.PostsCount))
	for _, childval := range conf.PostList {
		childbody := getData(fmt.Sprintf("https://i.cnblogs.com/api/posts/%s", strconv.Itoa(childval.Id)))
		fmt.Printf("childbody:%s \n", childbody)
		var jsconf blogbodyConf
		err := json.Unmarshal(childbody, &jsconf)
		if err != nil {
			fmt.Println("error:", err)
		}
		downloadFile(strings.NewReader(string(jsconf.BlogPost.PostBody)), strconv.Itoa(jsconf.BlogPost.Id), fmt.Sprintf("%s.md", strconv.Itoa(jsconf.BlogPost.Id)))

	}
	if conf.PageIndex > 0 && conf.PageIndex*conf.PageSize <= conf.PostsCount {
		getBlogList(conf.PageIndex + 1)
	}
	fmt.Println("执行完毕")
}
func getData(urlstr string) []byte {
	client := &http.Client{}
	fmt.Printf("\r\n urlstr:%s \n", urlstr)
	req, _ := http.NewRequest("GET", urlstr, nil)
	req.Header.Add("cookie", cookie)

	resp, _ := client.Do(req)
	defer resp.Body.Close()
	body, _ := ioutil.ReadAll(resp.Body)
	return body
}
func downloadFile(body io.Reader, path string, name string) {
	filepath := fmt.Sprintf("./cnblogs/%s/%s", path, name)
	// Create output file
	if path != "" {
		if _, err := os.Stat(fmt.Sprintf("./cnblogs/%s", path)); os.IsNotExist(err) {
			fmt.Printf("\r\n path:%s \n", fmt.Sprintf("./cnblogs/%s", path))
			// 必须分成两步：先创建文件夹、再修改权限
			os.MkdirAll(fmt.Sprintf("./cnblogs/%s", path), 0777) //0777也可以os.ModePerm
			os.Chmod(fmt.Sprintf("./cnblogs/%s", path), 0777)
		}
		filepath = fmt.Sprintf("./cnblogs/%s/%s", path, name)
	}
	out, err := os.Create(filepath)
	if err != nil {
		panic(err)
	}
	defer out.Close()
	// copy stream
	_, err = io.Copy(out, body)
	if err != nil {
		panic(err)
	}
}

type blogList struct {
	PageIndex  int `json:"pageIndex"`
	PageSize   int `json:"pageSize"`
	PostsCount int `json:"postsCount"`

	PostList []blogbodymsg `json:"postList"`
}
type blogbodymsg struct {
	Id int `json:"id"`

	DatePublished string `json:"datePublished"`

	DateUpdated string `json:"dateUpdated"`

	Title string `json:"title"`
}

type blogbodyConf struct {
	BlogPost blogPostEntity `json:"blogPost"`
}
type blogPostEntity struct {
	Id            int    `json:"id"`
	AutoDesc      string `json:"autoDesc"`
	DatePublished string `json:"datePublished"`
	PostBody      string `json:"postBody"`
	Title         string `json:"title"`
	Url           string `json:"url"`
}
