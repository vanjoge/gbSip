- [gbSip](#gbSip)

# gbSip

本项目为 GB28181 sip 网关，主要实现 sip 信令，用于国标设备接入，流媒体服务器采用 RTVS，可开箱即用。

可与 RTVS 和 JT808GW 配合，实现 JT1078 和 GB28181 的同时接入和互相转换。

本项目已集成 RTVS.JS 播放器，可直接播放接入设备的实时视频。

## UI

[vueUi 见此](/ui) ， 基于 vue3 + antd-design-vue3 + typescript 实现，主要演示如何 vue 如何调用 gbSip 和 RTVS.JS，仅挑选一些有代表性的核心功能实现

## 部署

部署见 RTVS 项目的部署文档，也可以直接参考此处的
[阿里云部署例子](https://blog.csdn.net/vanjoge/article/details/108319078)

## 其他项目地址

RTVS

[https://github.com/vanjoge/RTVS](https://github.com/vanjoge/RTVS)

[https://gitee.com/vanjoge/RTVS](https://gitee.com/vanjoge/RTVS)

JT808GW(808 接入网关 实现 808 协议接入并演示如何实现 RTVS 所需接口)

[https://github.com/vanjoge/JT808GW](https://github.com/vanjoge/JT808GW)

[https://gitee.com/vanjoge/JT808GW](https://gitee.com/vanjoge/JT808GW)

QQ 交流群：614308923
