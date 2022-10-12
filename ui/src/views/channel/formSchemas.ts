import type { FormSchema } from '@/components/core/schema-form/';

export const channelSchemas: FormSchema<API.TChannel>[] = [
  {
    field: 'ChannelId',
    component: 'Input',
    label: '通道ID',
    dynamicDisabled: true,
    rules: [{ required: true }],
  },
  {
    field: 'DeviceId',
    component: 'Input',
    label: '设备ID',
    dynamicDisabled: true,
    vShow: false,
    rules: [{ required: true }],
  },
  {
    field: 'NickName',
    component: 'Input',
    label: '别名',
  },
  {
    field: 'NetType',
    component: 'RadioGroup',
    label: '网络传输模式',
    defaultValue: 0,
    componentProps: {
      options: [
        {
          label: 'TCP被动',
          value: 0,
        },
        {
          label: 'TCP主动',
          value: 1,
        },
        {
          label: 'UDP',
          value: 2,
        },
      ],
    },
  },
  {
    field: 'TalkType',
    component: 'RadioGroup',
    label: '对讲模式',
    defaultValue: 0,
    componentProps: {
      options: [
        {
          label: '自动',
          value: 0,
        },
        {
          label: 'Invite被动',
          value: 1,
        },
        {
          label: 'Invite主动',
          value: 2,
        },
        {
          label: '广播',
          value: 3,
        },
      ],
    },
  },
  {
    field: 'PlayerMode',
    component: 'RadioGroup',
    label: '播放模式',
    defaultValue: 0,
    componentProps: {
      options: [
        {
          label: '自动',
          value: 0,
        },
        {
          label: '软解',
          value: 3,
        },
        {
          label: 'FMP4',
          value: 4,
        },
        {
          label: 'Webrtc',
          value: 5,
        },
        {
          label: 'Hls',
          value: 6,
        },
        {
          label: 'Codec',
          value: 7,
        },
      ],
    },
  },
];
