package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"io"
	"io/ioutil"
	"net/http"
	"os"
	"regexp"
	"strings"
)

func main() {
	// text := "id='js_content ' style='visibility: hidden; '><p style='text - align: center; margin - bottom: 0em; '><img class='rich_pages wxw-img' data-galleryid='' data-ratio='0.145' data-src='https://mmbiz.qpic.cn/mmbiz_gif/TbQs0QUaPaGPOEB0SvVcISrIzXEoJiaBTYy6aNYkbgday5MZoxfajbSiazgAK0ZW4ic59qHZwsEQ7JVVCNib8Npbsg/640?wx_fmt=gif' data-type='gif' data-w='600' style=''></p><section style='margin: 0px;padding: 0px;clear: both;min-height: 1em;color: rgb(51, 51, 51);font-family: mp-quote, -apple-system-font, BlinkMacSystemFont, &quot;Helvetica Neue&quot;, &quot;PingFang SC&quot;, &quot;Hiragino Sans GB&quot;, &quot;Microsoft YaHei UI&quot;, &quot;Microsoft YaHei&quot;, Arial, sans-serif;font-size: 17px;font-style: normal;font-variant-ligatures: normal;font-variant-caps: normal;font-weight: 400;letter-spacing: normal;orphans: 2;text-align: justify;text-indent: 0px;text-transform: none;white-space: normal;widows: 2;word-spacing: 0px;-webkit-text-stroke-width: 0px;text-decoration-thickness: initial;text-decoration-style: initial;text-decoration-color: initial;'><span style='margin: 0px;padding: 0px;font-family: 宋体;color: rgb(61, 170, 214);font-size: 18px;'><span style='color: rgb(61, 170, 214);margin: 0px;padding: 0px;font-family: Calibri;'>1</span>、观潮</span></section><section><iframe class='video_iframe rich_pages' data-vidtype='2' data-mpvid='wxv_2471423457168441345' data-cover='http%3A%2F%2Fmmbiz.qpic.cn%2Fmmbiz_jpg%2FTbQs0QUaPaH1YEkKYIakvKCxy58kFa3waxscLydsAiaDTfhe2uI4ALM8shHibfGaqUeY1kPFVbOKLqMg80ADOSbg%2F0%3Fwx_fmt%3Djpeg' allowfullscreen='' frameborder='0' data-ratio='1.7777777777777777' data-w='1024' style='border-radius: 4px;' data-src='https://mp.weixin.qq.com/mp/readtemplate?t=pages/video_player_tmpl&amp;action=mpvideo&amp;auto=0&amp;vid=wxv_2471423457168441345'></iframe></section><section style='margin: 0px;padding: 0px;clear: both;min-height: 1em;color: rgb(51, 51, 51);font-family: mp-quote, -apple-system-font, BlinkMacSystemFont, &quot;Helvetica Neue&quot;, &quot;PingFang SC&quot;, &quot;Hiragino Sans GB&quot;, &quot;Microsoft YaHei UI&quot;, &quot;Microsoft YaHei&quot;, Arial, sans-serif;font-size: 17px;font-style: normal;font-variant-ligatures: normal;font-variant-caps: normal;font-weight: 400;letter-spacing: normal;orphans: 2;text-align: justify;text-indent: 0px;text-transform: none;white-space: normal;widows: 2;word-spacing: 0px;-webkit-text-stroke-width: 0px;text-decoration-thickness: initial;text-decoration-style: initial;text-decoration-color: initial;'><span style='color: rgb(61, 170, 214);font-size: 18px;font-family: Calibri;'>2</span><span style='color: rgb(61, 170, 214);font-family: 宋体;font-size: 18px;'>、走月亮</span></section><section><iframe class='video_iframe rich_pages' data-vidtype='2' data-mpvid='wxv_2471423575313596417' data-cover='http%3A%2F%2Fmmbiz.qpic.cn%2Fmmbiz_jpg%2FTbQs0QUaPaH1YEkKYIakvKCxy58kFa3wnNiaGph71EbtjGM3nzJElnVhaPicjqRSgBiaoicbSqdGp56rFiaMt9zZKOw%2F0%3Fwx_fmt%3Djpeg' allowfullscreen='' frameborder='0' data-ratio='1.7777777777777777' data-w='1024' style='border-radius: 4px;' data-src='https://mp.weixin.qq.com/mp/readtemplate?t=pages/video_player_tmpl&amp;action=mpvideo&amp;auto=0&amp;vid=wxv_2471423575313596417'></iframe></section><section style='margin: 0px;padding: 0px;clear: both;min-height: 1em;color: rgb(51, 51, 51);font-family: mp-quote, -apple-system-font, BlinkMacSystemFont, &quot;Helvetica Neue&quot;, &quot;PingFang SC&quot;, &quot;Hiragino Sans GB&quot;, &quot;Microsoft YaHei UI&quot;, &quot;Microsoft YaHei&quot;, Arial, sans-serif;font-size: 17px;font-style: normal;font-variant-ligatures: normal;font-variant-caps: normal;font-weight: 400;letter-spacing: normal;orphans: 2;text-align: justify;text-indent: 0px;text-transform: none;white-space: normal;widows: 2;word-spacing: 0px;-webkit-text-stroke-width: 0px;text-decoration-thickness: initial;text-decoration-style: initial;text-decoration-color: initial;'><span style='color: rgb(61, 170, 214);font-size: 18px;font-family: Calibri;'>3</span><span style='color: rgb(61, 170, 214);font-family: 宋体;font-size: 18px;'>、现代诗二首</span></section><section><iframe class='video_iframe rich_pages' data-vidtype='2' data-mpvid='wxv_2471423732985872384' data-cover='http%3A%2F%2Fmmbiz.qpic.cn%2Fmmbiz_jpg%2FTbQs0QUaPaH1YEkKYIakvKCxy58kFa3w9iby7YXnuyncERHaFibrfKEvlAcnNClDJicicsr3bSvvAsFnCAUS3p6ZTQ%2F0%3Fwx_fmt%3Djpeg' allowfullscreen='' frameborder='0' data-ratio='1.7777777777777777' data-w='1024' style='border-radius: 4px;' data-src='https://mp.weixin.qq.com/mp/readtemplate?t=pages/video_player_tmpl&amp;action=mpvideo&amp;auto=0&amp;vid=wxv_2471423732985872384'></iframe></section><section style='margin: 0px;padding: 0px;clear: both;min-height: 1em;color: rgb(51, 51, 51);font-family: mp-quote, -apple-system-font, BlinkMacSystemFont, &quot;Helvetica Neue&quot;, &quot;PingFang SC&quot;, &quot;Hiragino Sans GB&quot;, &quot;Microsoft YaHei UI&quot;, &quot;Microsoft YaHei&quot;, Arial, sans-serif;font-size: 17px;font-style: normal;font-variant-ligatures: normal;font-variant-caps: normal;font-weight: 400;letter-spacing: normal;orphans: 2;text-align: justify;text-indent: 0px;text-transform: none;white-space: normal;widows: 2;word-spacing: 0px;-webkit-text-stroke-width: 0px;text-decoration-thickness: initial;text-decoration-style: initial;text-decoration-color: initial;'><span style='color: rgb(61, 170, 214);font-size: 18px;font-family: Calibri;'>4</span><span style='color: rgb(61, 170, 214);font-family: 宋体;font-size: 18px;'>、繁星</span></section><section><iframe class='video_iframe rich_pages' data-vidtype='2' data-mpvid='wxv_2471423887621472258' data-cover='http%3A%2F%2Fmmbiz.qpic.cn%2Fmmbiz_jpg%2FTbQs0QUaPaH1YEkKYIakvKCxy58kFa3w4ibD8WwdNZ031psDHLcC6VLXQX2TW3tfuW2DYSibMAfiaJzzzMylE58Yg%2F0%3Fwx_fmt%3Djpeg' allowfullscreen='' frameborder='0' data-ratio='1.7777777777777777' data-w='1024' style='border-radius: 4px;' data-src='https://mp.weixin.qq.com/mp/readtemplate?t=pages/video_player_tmpl&amp;action=mpvideo&amp;auto=0&amp;vid=wxv_2471423887621472258'></iframe></section><section style='margin: 0px;padding: 0px;clear: both;min-height: 1em;color: rgb(51, 51, 51);font-family: mp-quote, -apple-system-font, BlinkMacSystemFont, &quot;Helvetica Neue&quot;, &quot;PingFang SC&quot;, &quot;Hiragino Sans GB&quot;, &quot;Microsoft YaHei UI&quot;, &quot;Microsoft YaHei&quot;, Arial, sans-serif;font-size: 17px;font-style: normal;font-variant-ligatures: normal;font-variant-caps: normal;font-weight: 400;letter-spacing: normal;orphans: 2;text-align: justify;text-indent: 0px;text-transform: none;white-space: normal;widows: 2;word-spacing: 0px;-webkit-text-stroke-width: 0px;text-decoration-thickness: initial;text-decoration-style: initial;text-decoration-color: initial;'><span style='color: rgb(61, 170, 214);font-family: 宋体;font-size: 18px;'>习作（一）</span></section><section><iframe class='video_iframe rich_pages' data-vidtype='2' data-mpvid='wxv_2471426978202763264' data-cover='http%3A%2F%2Fmmbiz.qpic.cn%2Fmmbiz_jpg%2FTbQs0QUaPaH1YEkKYIakvKCxy58kFa3w4WFgzKRaJzGbJTz83icVw3xERHJ3Z09AeEwibXoHG1jIJPKTyyRpNchg%2F0%3Fwx_fmt%3Djpeg' allowfullscreen='' frameborder='0' data-ratio='1.7777777777777777' data-w='1024' style='border-radius: 4px;' data-src='https://mp.weixin.qq.com/mp/readtemplate?t=pages/video_player_tmpl&amp;action=mpvideo&amp;auto=0&amp;vid=wxv_2471426978202763264'></iframe></section><section style='margin: 0px;padding: 0px;clear: both;min-height: 1em;color: rgb(51, 51, 51);font-family: mp-quote, -apple-system-font, BlinkMacSystemFont, &quot;Helvetica Neue&quot;, &quot;PingFang SC&quot;, &quot;Hiragino Sans GB&quot;, &quot;Microsoft YaHei UI&quot;, &quot;Microsoft YaHei&quot;, Arial, sans-serif;font-size: 17px;font-style: normal;font-variant-ligatures: normal;font-variant-caps: normal;font-weight: 400;letter-spacing: normal;orphans: 2;text-align: justify;text-indent: 0px;text-transform: none;white-space: normal;widows: 2;word-spacing: 0px;-webkit-text-stroke-width: 0px;text-decoration-thickness: initial;text-decoration-style: initial;text-decoration-color: initial;'><span style='color: rgb(61, 170, 214);font-family: 宋体;font-size: 18px;'>语文园地（一）</span></section><section><iframe class='video_iframe rich_pages' data-vidtype='2' data-mpvid='wxv_2471427187582418944' data-cover='http%3A%2F%2Fmmbiz.qpic.cn%2Fmmbiz_jpg%2FTbQs0QUaPaH1YEkKYIakvKCxy58kFa3wtRxF26cgJXOeiaJ6ROCOJicP66UO2HEmK8AnBRHjcFDWNzxw0bvMUODQ%2F0%3Fwx_fmt%3Djpeg' allowfullscreen='' frameborder='0' data-ratio='1.7777777777777777' data-w='1024' style='border-radius: 4px;' data-src='https://mp.weixin.qq.com/mp/readtemplate?t=pages/video_player_tmpl&amp;action=mpvideo&amp;auto=0&amp;vid=wxv_2471427187582418944'></iframe></section><section style='margin: 0px;padding: 0px;clear: both;min-height: 1em;color: rgb(51, 51, 51);font-family: mp-quote, -apple-system-font, BlinkMacSystemFont, &quot;Helvetica Neue&quot;, &quot;PingFang SC&quot;, &quot;Hiragino Sans GB&quot;, &quot;Microsoft YaHei UI&quot;, &quot;Microsoft YaHei&quot;, Arial, sans-serif;font-size: 17px;font-style: normal;font-variant-ligatures: normal;font-variant-caps: normal;font-weight: 400;letter-spacing: normal;orphans: 2;text-align: justify;text-indent: 0px;text-transform: none;white-space: normal;widows: 2;word-spacing: 0px;-webkit-text-stroke-width: 0px;text-decoration-thickness: initial;text-decoration-style: initial;text-decoration-color: initial;'><span style='color: rgb(61, 170, 214);font-family: 宋体;font-size: 18px;'>单元总结（一）</span></section><p style='margin-bottom: 0px;'><iframe class='video_iframe rich_pages' data-vidtype='2' data-mpvid='wxv_2471427383255089153' data-cover='http%3A%2F%2Fmmbiz.qpic.cn%2Fmmbiz_jpg%2FTbQs0QUaPaH1YEkKYIakvKCxy58kFa3whaR3IXvcInm16Fcico7icpftQxsZeicfXqgmw9uepfgONNufq3Ty60Vnw%2F0%3Fwx_fmt%3Djpeg' allowfullscreen='' frameborder='0' data-ratio='1.7777777777777777' data-w='1024' style='border-radius: 4px;' data-src='https://mp.weixin.qq.com/mp/readtemplate?t=pages/video_player_tmpl&amp;action=mpvideo&amp;auto=0&amp;vid=wxv_2471427383255089153'></iframe></p><p style='text-align: center;margin-bottom: 0px;'><span style='font-size: 14px;text-align: center;'>以上视频为步步高与PASS绿卡合作内容</span></p><p style='text-align: center;margin-bottom: 0em;'><img class='rich_pages wxw-img' data-backh='94' data-backw='578' data-fileid='100014811' data-galleryid='' data-ratio='0.16296296296296298' data-s='300,640' data-src='https://mmbiz.qpic.cn/mmbiz_jpg/TbQs0QUaPaHkQsrzCS7ib0zjDWhajbBjsPNHFhB4xD5LjwwNoyvIXKRtKGQfk81EZiafszicSO53R04JMeyFV3GxQ/640?wx_fmt=jpeg' data-type='jpeg' data-w='1080' style='width: 100%;height: auto;'  /></p></div>"
	// text = strings.Replace(text, "(false)", "false", -1)
	// fmt.Println("text:", text)
	// regmpvids := regexp.MustCompile("data-mpvid='(.*?)' data-cover='")
	// mpvids := regmpvids.FindAllStringSubmatch(text, -1)
	// for i := 0; i < len(mpvids); i++ {
	// 	title := mpvids[i][1]
	// 	fmt.Printf("%s \r\n", title)
	// }
	// text := "{ title: '一《圆》 六年级数学 上册（北师大版）'.html(false), item_show_type: '0',url: 'http://mp.weixin.qq.com/s?__biz=MzU0MjkyMzcwMg==&amp;amp;mid=2247506020&amp;amp;idx=1&amp;amp;sn=b4b54ad0cbd0c72c07ef5e87f5f0f2a5&amp;amp;chksm=fb11d6bdcc665fabc5f7439e7fcfdbfd12824d7e831df6817d8bb0dc11f5d3d07bbdea3496e7&amp;amp;scene=21#wechat_redirect'.html(false).html(false), // 后台给的数据被encode了两次subject_name: '青苹果课堂',link_type: 'LINK_TYPE_MP_APPMSG',}"
	// text = strings.Replace(text, "(false)", "false", -1)
	// fmt.Println("text:", text)
	// regtitles := regexp.MustCompile("title: '(.*?)'.htmlfalse,")
	// titles := regtitles.FindAllStringSubmatch(text, -1)
	// regurls := regexp.MustCompile("url: '(.*?)'.htmlfalse.htmlfalse,")
	// urls := regurls.FindAllStringSubmatch(text, -1)
	// for i := 0; i < len(titles); i++ {
	// 	title := titles[i][1]
	// 	url := urls[i][1]
	// 	fmt.Printf("%s \r\n", title)
	// 	fmt.Printf("%s", url)
	// }
	jsonfile, err := os.Open("json.json")
	if err != nil {
		fmt.Println("error:", err)
		return
	}
	defer jsonfile.Close()
	jsonData, err := ioutil.ReadAll(jsonfile)
	if err != nil {
		fmt.Println("error:", err)
		return
	}
	var bloglist []blogList
	json.Unmarshal(jsonData, &bloglist)
	fmt.Println(bloglist)
	for _, childval := range bloglist {
		fmt.Println("childval.Link:", childval.Link)
		resp, err := http.Get(childval.Link)
		defer resp.Body.Close()
		htmlBytes, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			fmt.Println("error:", err)
		}
		strbody := string(htmlBytes)
		fmt.Println("strbody:", strbody)
		strbody = strings.Replace(strbody, "(false)", "false", -1)
		regtitles := regexp.MustCompile("title: '(.*?)'.htmlfalse,")
		titles := regtitles.FindAllStringSubmatch(strbody, -1)
		regurls := regexp.MustCompile("url: '(.*?)'.htmlfalse.htmlfalse,")
		urls := regurls.FindAllStringSubmatch(strbody, -1)
		if len(titles) > 0 {
			for i := 0; i < len(titles); i++ {
				title := titles[i][1]
				url := urls[i][1]
				fmt.Printf("%s \r\n", title)
				fmt.Printf("%s", url)
				getvedios(title, url, childval.Appmsgid, childval.Title)
			}
		} else {
			jiexi(strbody, childval.Appmsgid, childval.Title)
		}
	}
}

func getvedios(title string, url string, mid string, root string) {
	resp, err := http.Get(url)
	defer resp.Body.Close()
	htmlBytes, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		fmt.Println("error:", err)
	}
	strbody := string(htmlBytes)
	jiexi(strbody, mid, root)
}
func jiexi(strbody string, mid string, root string) {
	strbody = strings.Replace(strbody, "(false)", "false", -1)
	regmpvids := regexp.MustCompile("data-mpvid=\"(.*?)\" data-cover=\"")
	mpvids := regmpvids.FindAllStringSubmatch(strbody, -1)
	for i := 0; i < len(mpvids); i++ {
		mpvid := mpvids[i][1]
		getvediourl := fmt.Sprintf("https://mp.weixin.qq.com/mp/videoplayer?action=get_mp_video_play_url&preview=0&__biz=MzU0MjkyMzcwMg==&mid=%s&idx=4&vid=%s&uin=&key=&pass_ticket=&wxtoken=777&devicetype=&clientversion=&__biz=MzU0MjkyMzcwMg%3D%3D&appmsg_token=&x5=0&f=json", mid, mpvid)
		fmt.Printf("%s \r\n", getvediourl)
		getvediojson(getvediourl, root)
	}
}
func getvediojson(url string, root string) {
	recordbody := getData(url)
	fmt.Printf("\r\n recordbody:%s \n", recordbody)

	var conf videoConf
	err := json.Unmarshal(recordbody, &conf)
	if err != nil {
		fmt.Println("error:", err)
	}
	for _, childval := range conf.Url_info {
		if childval.Video_quality_level == 2 {
			downloadmp4(childval.Url, root, fmt.Sprintf("%s .mp4", conf.Title))
		}
	}
}

func downloadmp4(imgurl string, rootpath string, fileName string) {
	filepath := fmt.Sprintf("../教材/%s/%s", rootpath, fileName)
	isexists, err := exists(filepath)
	if isexists {
		return
	}
	res, err := http.Get(imgurl)
	if err != nil {
		fmt.Println("A error occurred!")
		return
	}
	defer res.Body.Close()
	// 获得get请求响应的reader对象
	reader := bufio.NewReaderSize(res.Body, 3200*1024)

	if _, err := os.Stat(fmt.Sprintf("../教材/%s", rootpath)); os.IsNotExist(err) {
		// 必须分成两步：先创建文件夹、再修改权限
		os.MkdirAll(fmt.Sprintf("../教材/%s", rootpath), 0777) //0777也可以os.ModePerm
		os.Chmod(fmt.Sprintf("../教材/%s", rootpath), 0777)
	}
	file, err := os.Create(filepath)
	if err != nil {
		panic(err)
	}
	// 获得文件的writer对象
	writer := bufio.NewWriter(file)

	written, _ := io.Copy(writer, reader)
	fmt.Printf("Total length: %d", written)
}
func getData(urlstr string) []byte {
	client := &http.Client{}
	fmt.Printf("\r\n urlstr:%s \n", urlstr)
	req, _ := http.NewRequest("GET", urlstr, nil)

	resp, _ := client.Do(req)
	defer resp.Body.Close()
	body, _ := ioutil.ReadAll(resp.Body)
	return body
}
func exists(path string) (bool, error) {
	_, err := os.Stat(path)
	if err == nil {
		return true, nil
	}
	if os.IsNotExist(err) {
		return false, nil
	}
	return true, err
}

type blogList struct {
	Title    string `json:"title"`
	Link     string `json:"link"`
	Appmsgid string `json:"appmsgid"`
}

type videoConf struct {
	Url_info []Url_info `json:"url_info"`
	Title    string     `json:"title"`
}

type Url_info struct {
	Url                 string `json:"url"`
	Video_quality_level int    `json:"video_quality_level"`
}
