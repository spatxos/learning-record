package main

import (
	"bytes"
	"compress/gzip"
	"encoding/base64"
	"fmt"
	"io/ioutil"
)

func main() {
	var b bytes.Buffer
	gz := gzip.NewWriter(&b)
	if _, err := gz.Write([]byte("__gads=ID=xxx:T=1649728913:S=ALNI_MbOOVEJx5s2Puf6CEs5qYSkW_aHwg; UM_distinctid=1805aabf9bd238-08b22c569116a1-3a67551f-144000-1805aabf9be934; Hm_lpvt_5e692fc8cef6db021a22bd470d660e4c=1651548396; Hm_lvt_5e692fc8cef6db021a22bd470d660e4c=1651548396; .AspNetCore.Antiforgery.b8-pDmTq1XM=CfDJ8AuMt_3FvyxIgNOR82PHE4kGyR-4aSs739sPowluSIbWBXRwvByozasPNheZKXorGKFxlQ2XMwL2PoW9QSwXcJS2PX1srKH7Fa9naKg7dhqQywIeSl4dYqyikYKZH4e61r02vJbatsXG76kQEqh_vUo; .AspNetCore.Session=CfDJ8AuMt/3FvyxIgNOR82PHE4luRKm0AX8iUW7HmvQ8B+VY3t4ucprrn4Mu3jD4G++ufQxZZtdKQbvlaZnSDa6wkIP3oZiLhlEaGmF0MJPSm/jvekMt/lPeLQ5fSxmz5rtmzj7AGjgKu1bCViLfVGink8N4v4fsafdaldUsUF+kpcWz; __utmz=2642927.1652923332.1.1.utmcsr=cn.bing.com|utmccn=(referral)|utmcmd=referral|utmcct=/; __utmc=2642927; __utma=2642927.1176019125.1649728912.1652923332.1652923332.1; Hm_lvt_0daf1d1987de95558f12b56df149bfda=1653285692; Hm_lpvt_0daf1d1987de95558f12b56df149bfda=1653285692; Hm_lvt_851cdd44d7a836d43196b0bfa8c0c3bb=1654159502; Hm_lpvt_851cdd44d7a836d43196b0bfa8c0c3bb=1654475406; Hm_lvt_c897bd902d294bb3778d9ab55b85a256=1655199408; Hm_lpvt_c897bd902d294bb3778d9ab55b85a256=1655199408; Hm_lvt_866c9be12d4a814454792b1fd0fed295=1655862584; _gid=GA1.2.635015422.1656895021; .Cnblogs.AspNetCore.Cookies=CfDJ8EOBBtWq0dNFoDS-ZHPSe5251Pvp1fD4MFhyd_PD4tNcGD_u2h0PSkDcNGhyUMRd1zyFDyEZ6kSQ9PGFTgwOqqzt9s7WozoAgC3wf2pU7fkRY59RV9cCP6HrSQeE-jHTo-1AFJNcEE1on6pVxzRETNegcBcpYbBBK97Dyz0wwErnYlgrkbkK6jUUFD4ajAUAP7O9nhT6keL_3mmLaLd9VA3JJHk22rvNVoYAWJ0c_nQFq4ysvSLS4UnM3LpKqmeV8XdOj9PJ59-GX_gXq6IVIJJcyxRiQmBRSgItn6GDXgg5WW3skY2HcTYlOXQzDYg_DssEnqLp5ETJhK6n_xBYIkM7z3oZNp-94VnYZuNHSolvNGnjE4fCNIjKGUYRNu-p1QEJGpcCdNrct9oEKwNtNAC9juCaB1B-dZkIX9GDds7w8uAeJP-PP2hNGJRoXwwvqhSxwvMfnNPKuUUfHM4E35TDeh0Uc5iM0dR92oao4_IPxx_yZbPmYmOzJ0K9-hFGfcRvlm0yPt0nALLNA1fM1QRyfxna5zWXz1TJkLs0hVX3ETtjIZ_Bo02WlyldpKs2_Q; .CNBlogsCookie=00570C45AB9DECCA0F43D2D63ECBA80584565487C0150E1B16B249C3C4C77A4B092D76EED817FF9F26485C86C144A03FC85262706ED3A218614FA4A93EBD1AD55B64EF37F16DFBB40D9244F428F757D00C08BCFD; _ga_4CQQXWHK3C=GS1.1.1657523114.3.0.1657523237.0; _ga=GA1.2.1176019125.1649728912; _ga_3Q0DVSGN10=GS1.1.1657523009.4.0.1657523341.60; __gpi=UID=000004c28bf2c485:T=1649729335:RT=1657845954:S=ALNI_MY4bFzL-Q4DF7EivtB4voDkSrLb2w; Hm_lpvt_866c9be12d4a814454792b1fd0fed295=1657846488; XSRF-TOKEN=CfDJ8EOBBtWq0dNFoDS-ZHPSe50orG6Mybq5yio-A9J8ZgkgwbhnxGRVbB3l0EnCMqIJdhUCXl8sdCvB29O1akd_LlpaPo1xYiDYuNnn6f5PIInYdb-Yma2TxucA_785aeuqSi4ZfEP7mDQPDwDGd4YmpbPrJAy0e9VUVt7usnIj1xpcz5GCfR8vePALsao_NP1u7w; _gat_gtag_UA_48445196_1=1")); err != nil {
		panic(err)
	}
	if err := gz.Flush(); err != nil {
		panic(err)
	}
	if err := gz.Close(); err != nil {
		panic(err)
	}
	str := base64.StdEncoding.EncodeToString(b.Bytes())
	fmt.Println(str)
	data, _ := base64.StdEncoding.DecodeString(str)
	fmt.Println(data)
	rdata := bytes.NewReader(data)
	r, _ := gzip.NewReader(rdata)
	s, _ := ioutil.ReadAll(r)
	fmt.Println(string(s))
}
