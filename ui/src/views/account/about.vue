<template>
  <div>
    <Card>
      <Card.Meta title="关于">
        <template #description>
          <BlankLink
            :url="pkg.repository.url"
            :text="pkg.name"
          />是一个GB28181的前端实现，流媒体依赖RTVS
        </template>
      </Card.Meta>
    </Card>
    <Card class="mt-3">
      <Descriptions title="项目信息" :column="2" bordered>
        <Descriptions.Item label="版本">
          <Tag color="processing">{{ pkg.version }}</Tag>
        </Descriptions.Item>
        <Descriptions.Item label="最后编译时间">
          <Tag color="processing">{{ lastBuildTime }}</Tag>
        </Descriptions.Item>
        <Descriptions.Item label="GitHub">
          <BlankLink :url="pkg.repository.url" text="GitHub" />
        </Descriptions.Item>
        <Descriptions.Item label="预览地址">
          <BlankLink :url="pkg.homepage" text="预览地址" />
        </Descriptions.Item>
        <Descriptions.Item label="QQ交流群" label-align="left" align="left">
          <a
            target="_blank"
            href="https://qm.qq.com/cgi-bin/qm/qr?k=rj_2z2kSdi3U_wZMVKOL5l--wSZtMBJu&jump_from=webapi"
            ><img border="0" src="//pub.idqqimg.com/wpa/images/group.png"
          /></a>
        </Descriptions.Item>
      </Descriptions>
    </Card>
    <Card class="mt-3">
      <Descriptions title="生产环境依赖" bordered>
        <template v-for="(value, key) in pkg.dependencies" :key="key">
          <Descriptions.Item :label="key">
            <BlankLink :url="key" :text="value" />
          </Descriptions.Item>
        </template>
      </Descriptions>
    </Card>
    <Card class="mt-3">
      <Descriptions title="开发环境依赖" bordered>
        <template v-for="(value, key) in pkg.devDependencies" :key="key">
          <Descriptions.Item :label="key">
            <BlankLink :url="key" :text="value" />
          </Descriptions.Item>
        </template>
      </Descriptions>
    </Card>
  </div>
</template>

<script setup lang="tsx">
  import { Descriptions, Card, Tag } from 'ant-design-vue';
  const { pkg, lastBuildTime } = __APP_INFO__;

  const BlankLink = ({ url = '', text }) => {
    const target = /^http(s)?:/.test(url) ? url : `https://www.npmjs.com/package/${url}`;
    return (
      <a href={target} target="_blank">
        {text}
      </a>
    );
  };
</script>
