import { getAxios } from "../Foundation/AxiosUtil";
import qs from 'qs';
import { sendNotifyEx } from "../Foundation/NotificationEx";
import { env } from "../Foundation/Environment";

// ==================== 配置开始 ====================
let AKKCLOUD_EMAIL = env("AKKCLOUD_EMAIL", "");
let AKKCLOUD_PASSWD = env("AKKCLOUD_PASSWD", "");
let AKKCLOUD_HOST = env("AKKCLOUD_HOST", "3.akkcloud1.com");
// ==================== 配置结束 ====================

const axiosInstance = getAxios();

axiosInstance.defaults.headers['referer'] = `https://${AKKCLOUD_HOST}/auth/login`;
axiosInstance.defaults.headers['user-agent'] = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.82 Safari/537.36';
axiosInstance.defaults.headers['x-requested-with'] = 'XMLHttpRequest';
axiosInstance.defaults.headers['Accept-Encoding'] = '';
axiosInstance.defaults.withCredentials = true;

async function signin() {
    if (!AKKCLOUD_EMAIL || !AKKCLOUD_PASSWD) {
        if (!AKKCLOUD_EMAIL) {
            console.log('未配置Akkcloud签到账号名');
        }
        if (!AKKCLOUD_PASSWD) {
            console.log('未配置Akkcloud签到账号密码');
        }
        console.log('跳过Akkcloud签到任务');

        return;
    }

    return axiosInstance.post(`https://${AKKCLOUD_HOST}/auth/login`, qs.stringify({
        email: AKKCLOUD_EMAIL,
        passwd: AKKCLOUD_PASSWD,
        code: '',
    })).then<string>(async response => {
        const data = response.data;

        const ret = data?.ret;
        const msg = data?.msg;

        let message = "";

        if (ret == 1) {
            console.log(`登录成功`);

            return response.headers["set-cookie"].map(v => v.split(';')[0]).join(';');
        } else {
            message = `登录失败：${msg || data}`;
            await sendNotifyEx(null, {
                title: "Akkcloud签到",
                content: message,
            });
        }

        return null;
    }, error => {
        console.log(`登录失败：${error}`);

        return null;
    }).then(cookie => {
        if (!cookie) return null;

        return axiosInstance.post(`https://${AKKCLOUD_HOST}/user/checkin`, qs.stringify({}), {
            headers: {
                "cookie": cookie,
                "referer": `https://${AKKCLOUD_HOST}/user`,
            },
        });
    }).then(async response => {
        if (!response) return null;

        const data = response.data;

        const ret = data?.ret;
        const msg = data?.msg;

        let message = "";

        if (ret == 1) {
            message = `签到成功：${msg || data}`;
        } else {
            message = `签到失败：${msg || data}`;
        }

        await sendNotifyEx(null, {
            title: "Akkcloud签到",
            content: message,
        });
    }, error => {
        console.log(`签到失败：${error}`);
    });
}

signin();
