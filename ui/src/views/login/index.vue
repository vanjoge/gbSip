<template>
  <div class="login-box">
    <div class="login-logo">
      <!-- <svg-icon name="logo" :size="45" /> -->
      <img src="~@/assets/images/gbs.svg" width="100" />
      <h1 class="mb-0 ml-2 text-3xl font-bold"></h1>
    </div>
    <a-form layout="horizontal" :model="state.formInline" @submit.prevent="handleSubmit">
      <a-form-item>
        <a-input v-model:value="state.formInline.UserName" size="large" placeholder="请输入用户名">
          <template #prefix><user-outlined type="user" /></template>
        </a-input>
      </a-form-item>
      <a-form-item>
        <a-input
          v-model:value="state.formInline.Password"
          size="large"
          type="password"
          placeholder="请输入密码"
          autocomplete="new-password"
        >
          <template #prefix><lock-outlined type="user" /></template>
        </a-input>
      </a-form-item>
      <a-form-item>
        <div class="captcha-wrapper">
          <a-input
            v-model:value="state.formInline.captcha"
            size="large"
            placeholder="请输入验证码（不区分大小写）"
            style="flex: 1"
          >
            <template #prefix><lock-outlined /></template>
          </a-input>
          <img
            v-if="state.captcha"
            :src="state.captcha"
            alt="验证码"
            class="captcha-img"
            @click="setCaptcha"
            title="点击刷新验证码"
          />
        </div>
      </a-form-item>
      <a-form-item>
        <a-button type="primary" html-type="submit" size="large" :loading="state.loading" block>
          登录
        </a-button>
      </a-form-item>
    </a-form>
  </div>
</template>

<script setup lang="ts">
  import { reactive, onMounted } from 'vue';
  import { UserOutlined, LockOutlined } from '@ant-design/icons-vue';
  import { useRoute, useRouter } from 'vue-router';
  import md5 from 'js-md5';
  import { message, Modal } from 'ant-design-vue';
  import { useUserStore } from '@/store/modules/user';
  import { getImageCaptcha } from '@/api/login';
  import { to } from '@/utils/awaitTo';

  const state = reactive({
    loading: false,
    captcha: '',
    captchaId: '',
    formInline: {
      UserName: '',
      Password: '',
      captcha: '',
    },
  });

  const route = useRoute();
  const router = useRouter();

  const userStore = useUserStore();

  // 获取验证码
  const setCaptcha = async () => {
    try {
      const { Id, Img } = await getImageCaptcha({ width: 120, height: 40 });
      state.captcha = Img;
      state.captchaId = Id;
      state.formInline.captcha = '';
    } catch (error) {
      console.error('获取验证码失败:', error);
    }
  };

  // 页面加载时获取验证码
  onMounted(() => {
    setCaptcha();
  });

  const handleSubmit = async () => {
    const { UserName: username, Password: password, captcha } = state.formInline;
    if (username.trim() == '' || password.trim() == '') {
      return message.warning('用户名或密码不能为空！');
    }
    if (!captcha || captcha.trim() == '') {
      return message.warning('请输入验证码！');
    }

    message.loading('登录中...', 0);
    state.loading = true;

    // 密码加密
    const loginParams = {
      UserName: username,
      Password: md5(password),
      captchaId: state.captchaId,
      captcha,
    };

    const [err] = await to(userStore.login(loginParams));
    if (err) {
      Modal.error({
        title: () => '提示',
        content: () => err.message,
      });
      setCaptcha(); // 登录失败刷新验证码
    } else {
      message.success('登录成功！');
      setTimeout(() => router.replace((route.query.redirect as string) ?? '/'));
    }
    state.loading = false;
    message.destroy();
  };
</script>

<style lang="less" scoped>
  .login-box {
    display: flex;
    width: 100vw;
    height: 100vh;
    padding-top: 240px;
    background: url('~@/assets/login.svg');
    background-size: 100%;
    flex-direction: column;
    align-items: center;

    .login-logo {
      display: flex;
      margin-bottom: 30px;
      align-items: center;

      .svg-icon {
        font-size: 48px;
      }
    }

    :deep(.ant-form) {
      width: 400px;

      .ant-col {
        width: 100%;
      }

      .ant-form-item-label {
        padding-right: 6px;
      }
    }
  }

  .captcha-wrapper {
    display: flex;
    gap: 12px;
    align-items: center;

    .captcha-img {
      height: 40px;
      cursor: pointer;
      border: 1px solid #d9d9d9;
      border-radius: 2px;
      transition: all 0.3s;

      &:hover {
        border-color: #40a9ff;
      }
    }
  }
</style>
