import type { RouteRecordRaw } from 'vue-router';
import RouterView from '@/layout/routerView/index.vue';
import { t } from '@/hooks/useI18n';

const moduleName = 'gbs';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/realtime',
    name: `${moduleName}-realtime`,
    // redirect: '/video/realtime',
    // component: RouterView,
    meta: {
      title: t('gbs.realtime'),
      icon: 'icon-yibiaopan',
    },
    component: () => import('@/views/realtime/index.vue'),
    // children: [
    //   {
    //     path: 'welcome',
    //     name: `${moduleName}-welcome`,
    //     meta: {
    //       title: t('routes.dashboard.workbench'),
    //       icon: 'icon-shouye',
    //     },
    //     component: () =>
    //       import(/* webpackChunkName: "dashboard-welcome" */ '@/views/dashboard/welcome/index.vue'),
    //   },
    // ],
  },
  {
    path: '/devManager',
    name: `${moduleName}-devManager`,
    meta: {
      title: t('gbs.devManager'),
      icon: 'icon-zhuomian',
      // keepAlive: true,
      // hideInTabs: true,
    },
    component: () => import(/* webpackChunkName: "dashboard-welcome" */ '@/views/device/index.vue'),
  },
  {
    path: '/channelManager/:deviceId',
    name: `${moduleName}-channelManager`,

    meta: {
      title: t('gbs.channelManager'),
      hideInMenu: true,
      hideInTabs: true,
      icon: 'icon-zhuomian',
    },
    component: () =>
      import(/* webpackChunkName: "dashboard-welcome" */ '@/views/channel/index.vue'),
  },
  {
    path: '/superior',
    name: `${moduleName}-superior`,
    meta: {
      title: t('gbs.superior'),
      icon: 'icon-zhuomian',
      // keepAlive: true,
      // hideInTabs: true,
    },
    component: () =>
      import(/* webpackChunkName: "dashboard-welcome" */ '@/views/superior/index.vue'),
  },
  {
    path: '/swagger',
    name: `${window.location.protocol}//${window.location.host}/swagger`,
    component: RouterView,
    meta: {
      title: 'swagger接口文档',
      icon: 'icon-externa-link',
    },
  },
  {
    path: '/knife4',
    name: `${window.location.protocol}//${window.location.host}/Help`,
    component: RouterView,
    meta: {
      title: 'Knife4UI',
      icon: 'icon-externa-link',
    },
  },
];

export default routes;
